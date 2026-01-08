using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Animation;

using App;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Engine.Components;
using Engine.Entities;
using Engine.Strategy.Provider;

using PvpMode.Entities;
using PvpMode.Services;

using Senspark;

using Services;

using UnityEngine;

using BotManager = Engine.Components.BotManager;

namespace Engine.Manager {
    public struct HeroTakeDamageInfo {
        public Vector2Int LastPlayerLocation;
        public bool AllowRevive;
        public bool IsReviveAds;
        public int ReviveGemValue;
    }

    public class DefaultPlayerManager : IPlayerManager {
        private readonly IEntityManager _entityManager;
        private IProvider _bomberProvider;
        private IPlayerStorageManager _playerStoreManager;

        private readonly Dictionary<int, PlayerData> _dicPlayerData = new();
        private readonly Dictionary<int, Player> _dicPlayers = new();
        private readonly List<Player> _players = new();
        // do khi active, deactive _players không thêm hay remove ngay nên cần biến này khi cần get active hero
        private readonly Dictionary<HeroId, bool> _pendingActivePlayers = new();

        public List<Player> Players => _players;

        private readonly Transform _container;

        private readonly ISoundManager _soundManager;
        private readonly IHeroStatsManager _heroStatsManager;
        private readonly IHeroIdManager _heroIdManager;

        private readonly Dictionary<HeroId, bool> _thunderStrikes;

        private EquipmentData[] _equipments;
        private readonly Dictionary<StatId, int> _maximumStats;
        private Vector2Int _startingPosition;

        public HeroTakeDamageInfo TakeDamageInfo { get; private set; }

        public static PlayerType TrPlayerType(int skin) {
            // Skin của tr hero mới + 100 vì tạo enum mới ở phần cuối (11,...,18 => 111,...,118)
            return (PlayerType) (skin >= (int) PlayerType.Poo - 100 ? skin + 100 : skin);
        }

        private int GetItemIdFromSkinId(int skinId) {
            return _heroIdManager.GetItemId(skinId);
        }

        public DefaultPlayerManager(
            IEntityManager entityManager,
            Transform container,
            IMapInfo mapInfo,
            IMatchHeroInfo[] heroes) {
            _entityManager = entityManager;
            _container = container;

            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            _heroIdManager = ServiceLocator.Instance.Resolve<IHeroIdManager>();
            _thunderStrikes = new Dictionary<HeroId, bool>();
            _bomberProvider = new ExhaustedProvider("Prefabs/Allies/PlayerPvp");
            for (var i = 0; i < heroes.Length; ++i) {
                var hero = heroes[i];
                var location = new Vector2Int(
                    mapInfo.StartingPositions[i].x,
                    mapInfo.StartingPositions[i].y
                );
                var skinChests = hero.SkinChests;
                var playerType = TrPlayerType(hero.Skin);
                var itemId = GetItemIdFromSkinId(hero.Skin);
                var maximumStats = new Dictionary<StatId, int> {
                    [StatId.Speed] = hero.MaxSpeed,
                    [StatId.Range] = hero.MaxBombRange,
                    [StatId.Count] = hero.MaxBombCount,
                };
                var playerData = new PlayerData {
                    itemId = itemId,
                    playerType = playerType,
                    playercolor = PlayerColor.HeroTr,
                    bombSkin = skinChests.TryGetValue((int) SkinChestType.Bomb, out var bombs) ? bombs[0] : -1,
                    explosionSkin =
                        skinChests.TryGetValue((int) SkinChestType.Explosion, out var explosions) ? explosions[0] : 0,
                    speed = hero.Speed,
                    bombRange = hero.BombRange,
                    bombNum = hero.BombCount,
                    hp = hero.Health,
                    bombDamage = hero.Damage,
                    heroId = new HeroId(1, HeroAccountType.Tr),
                    MaximumStats = maximumStats,
                    skinChests = skinChests.ToDictionary(
                        it => it.Key,
                        it => it.Value.ToList()
                    ),
                };
                AddPlayer(location, playerData, i, false);
            }
        }

        public DefaultPlayerManager(
            IEntityManager entityManager,
            Transform container,
            EquipmentData[] equipments = null,
            Dictionary<StatId, int> maximumStats = null) {
            _entityManager = entityManager;
            _container = container;
            _equipments = equipments;
            _maximumStats = maximumStats;

            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            _heroIdManager = ServiceLocator.Instance.Resolve<IHeroIdManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _thunderStrikes = new Dictionary<HeroId, bool>();
        }

        public async Task FirstInitPlayerPVE(List<Vector2Int> locations) {
            if (_entityManager.LevelManager.GameMode == GameModeType.StoryMode) {
#if TEST_PET
                _bomberProvider =
 new ExhaustedProvider(AppConfig.IsProduction ? "Prefabs/Allies/PlayerStoryMode" : "Prefabs/Allies/PlayerStoryModeHasPet");
#else
                _bomberProvider = new ExhaustedProvider("Prefabs/Allies/PlayerStoryMode");
#endif

                var playerData = GetPlayerData(_entityManager.LevelManager.StoryModeHero);
                await AddPlayer(locations[0], playerData, 0, false);
                _startingPosition = locations[0];
            } else if (_entityManager.LevelManager.IsPvpMode) {
                // PvpMode will call other construction with array IPvPHeroDetail
            } else {
                //DevHoang: Open for all platforms
                // if (AppConfig.IsBscOrPolygon()) {
                if (true) {
                    _bomberProvider = new ExhaustedProvider("Prefabs/Allies/Player");
                } 
                var playerStorageManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
                var workingPlayer = playerStorageManager.GetInMapPlayerData();

                for (var i = 0; i < locations.Count && i < workingPlayer.Count; i++) {
                    await AddPlayer(locations[i], workingPlayer[i], i, false);
                }
            }
        }

        private Dictionary<StatId, int> GetMaximumStats(int itemId) {
            var maximumStats = new Dictionary<StatId, int>();
            var stats = _heroStatsManager.GetStats(itemId);
            foreach (var stat in stats) {
                switch (stat.StatId) {
                    case (int) StatId.Range:
                        maximumStats[StatId.Range] = stat.Max;
                        break;
                    case (int) StatId.Speed:
                        maximumStats[StatId.Speed] = stat.Max;
                        break;
                    case (int) StatId.Count:
                        maximumStats[StatId.Count] = stat.Max;
                        break;
                }
            }
            return maximumStats;
        }

        private PlayerData GetPlayerData(IHeroDetails hero) {
            _equipments ??= Array.Empty<EquipmentData>();
            var equipmentFilter = Array.FindAll(_equipments, it => it.Equipped);
            var skinChests = new Dictionary<int, List<int>>();
            foreach (var equip in equipmentFilter) {
                if (!skinChests.ContainsKey(equip.ItemType)) {
                    skinChests[equip.ItemType] = new List<int>();
                }
                skinChests[equip.ItemType].Add(equip.ItemId);
            }
            var playerType = TrPlayerType(hero.Skin);
            var itemId = GetItemIdFromSkinId(hero.Skin);
            var playerData = new PlayerData {
                playerType = playerType,
                playercolor = PlayerColor.HeroTr,
                bombSkin = skinChests.TryGetValue((int) SkinChestType.Bomb, out var bombs) ? bombs[0] : -1,
                explosionSkin =
                    skinChests.TryGetValue((int) SkinChestType.Explosion, out var explosions) ? explosions[0] : 0,
                storyHp = hero.StoryHp,
                bombDamage = hero.BombPower,
                speed = hero.Speed,
                bombRange = hero.BombRange,
                bombNum = hero.BombCount,
                heroId = new HeroId(hero.Id, hero.AccountType),
                itemId = itemId,
                abilities = Array.Empty<PlayerAbility>(),
                MaximumStats = _maximumStats ?? GetMaximumStats(itemId),
                skinChests = skinChests,
            };
            return playerData;
        }

        public async Task AddPlayer(Vector2Int location, PlayerData playerData, int slot, bool isDangerous) {
            if (playerData == null) {
                return;
            }
            if (_entityManager.LevelManager.GameMode != GameModeType.StoryMode &&
                _entityManager.LevelManager.GameMode != GameModeType.PvpMode) {
                if (!playerData.active || playerData.stage == HeroStage.Home) {
                    return;
                }
            }

            var player = await GenerateBomberMan(playerData, location);
            player.MaximumStats = playerData.MaximumStats;

            // if (playerData.stage == HeroStage.Sleep) {
            //     player.GetComponent<BotManager>().ForceSleep();
            // }

            AddBomberman(player, slot);
            SetPropertiesAndAbility(player, playerData, isDangerous);
            _dicPlayerData[slot] = playerData;
        }

        public void RemoveHeroes(HeroId[] heroIds) {
            foreach (var heroId in heroIds) {
                RemoveBomber(heroId);
            }
        }
        
        public Player GetPlayerById(HeroId id) {
            foreach (var player in _dicPlayers)
            {
                if (player.Value.HeroId == id) {
                    return player.Value;
                }
            }
            return null;
        }

        public Player GetPlayerBySlot(int slot) {
            Debug.Assert(slot >= 0 && slot < Players.Count);
            return Players[slot];
        }

        public Player GetPlayer(int slot = 0) {
            return GetPlayerBySlot(slot);
        }

        private  async UniTask<Player> GenerateBomberMan(PlayerData playerData, Vector2Int tileLocation) {
            if (AppConfig.IsAirDrop() && _bomberProvider == null) {
                var provider = new ExhaustedProvider();
                _bomberProvider = await provider.GetExhaustedProvider("PlayerTon");
            }
            var entity = _bomberProvider.CreateInstance(_entityManager);
            var player = (Player) entity;
            player.transform.SetParent(_container, false);
            player.transform.localPosition = _entityManager.MapManager.GetTilePosition(tileLocation);
            var animator = player.GetComponent<HeroAnimator>();
            animator.SetTypeAndColor(playerData.playerType, playerData.playercolor, playerData.rare);
            if (player.dropper) {
                player.dropper.SetHeroSprite(playerData.playerType, playerData.playercolor);
            }
            if (player.backlight) {
                player.backlight.sprite = await AnimationResource.GetBacklightImageByRarity(playerData.rare, false);
                player.backlight.enabled = playerData.rare > 0;
            }
            if (_entityManager.LevelManager.IsPvpMode ||
                _entityManager.LevelManager.IsStoryMode) {
                var avatar = (playerData.skinChests.TryGetValue((int) SkinChestType.Avatar, out var avatars)
                     && avatars != null 
                     && avatars.Count > 0)
                        ? avatars[0]
                        : 0;
                var trail = (playerData.skinChests.TryGetValue((int) SkinChestType.Trail, out var trails)
                     && trails != null 
                     && trails.Count > 0)
                        ? trails[0]
                        : 0;
                player.avatar.Initialize(avatar, playerData.playerType);
                animator.SetAvatarId(avatar);

                player.SetTrailEffect(trail);
            }
            player.SetPlayerID(playerData.heroId);
            return player;
        }

        public void UpdateProcess(int slot) {
            var player = Players[slot];
            if (player == null || !player.IsAlive) {
                return;
            }
            if (player.Movable.IsMoving && player.BombLoseControl) {
                SpawnBomb(slot);
            }
        }

        public void SetThunderStrike(HeroId id, bool value) {
            _thunderStrikes[id] = value;
        }

        public bool CanPlaceBomb(HeroId id) {
            return !_thunderStrikes.ContainsKey(id) || _thunderStrikes[id] == false;
        }

        public void SetPropertiesAndAbility(Player player, PlayerData playerData, bool isDangerous) {
            SetProperties(player, playerData, isDangerous);

            if (_entityManager.LevelManager.IsPvpMode) {
                return;
            }
            if(playerData == null) {
                return;
            }
            foreach (var ability in playerData.abilities) {
                SetAbilities(player, ability);
            }
        }

        private void AddBomberman(Player player, int slot) {
            player.Type = EntityType.BomberMan;
            _entityManager.AddEntity(player);

            player.Init(slot);
            _dicPlayers[slot] = player;
            _players.Clear();
            _players.AddRange(_dicPlayers.Values.ToList());
            _players.Sort((a, b) => a.Slot - b.Slot);
        }

        private void RemoveBomber(HeroId heroId) {
            var removePendingActivePlayer =
                _pendingActivePlayers.FirstOrDefault(dicPlayer => dicPlayer.Key.Id == heroId.Id).Key;
            if (removePendingActivePlayer != default) {
                _pendingActivePlayers.Remove(removePendingActivePlayer);
            }
            var player = GetPlayerById(heroId);
            if (player == null) {
                return;
            }
            Players.Remove(player);
            var removeDicPlayer = _dicPlayers.FirstOrDefault(dicPlayer => dicPlayer.Value.HeroId.Id == heroId.Id).Key;
            if (removeDicPlayer != default) {
                _dicPlayers.Remove(removeDicPlayer);
            }
            player.Kill(false);
        }
        
        private void SetProperties(Player player, PlayerData playerData, bool isDangerous) {
            if(playerData == null) {
                Players.Remove(player);
                player.Kill(false);
                return;
            }
            player.Movable.Speed = playerData.speed;
            player.Bombable.Damage = playerData.bombDamage;
            player.Bombable.ExplosionLength = playerData.bombRange;
            player.Health.Stamina = playerData.stamina;
            player.Bombable.MaxBombNumber = playerData.bombNum;

            if (_entityManager.LevelManager.GameMode == GameModeType.StoryMode) {
                player.Bombable.Damage += playerData.GetUpgradePower();
                player.Health.SetShowHealthBarWhenFull(false);
                player.Health.MaxHealth = playerData.storyHp;
                player.Health.SetCurrentHealth(playerData.storyHp);
                if (player.Movable.Speed > 10) {
                    player.Movable.Speed = 10;
                }
            } else if (_entityManager.LevelManager.IsPvpMode) {
                var levelManage = _entityManager.LevelManager;

                player.Movable.Speed = playerData.speed;
                player.Bombable.Damage = playerData.bombDamage;
                player.Health.Stamina = 1;
                player.Health.MaxHealth = playerData.hp;
                player.Health.SetCurrentHealth(playerData.hp);
                player.Health.SetShowHealthBarWhenFull(false);
                levelManage.OnUpdateItem(player.Slot, ItemType.Armor, 0);
                levelManage.OnUpdateItem(player.Slot, ItemType.Kick, 0);
            } else {
                player.Health.MaxHealth = playerData.maxHp;
                var damageFrom = isDangerous ? DamageFrom.Thunder : DamageFrom.BombExplode;
                player.Health.SetCurrentHealth(playerData.hp, damageFrom);
            }

            player.Bombable.SetHeroIdAndBombSkin(playerData.heroId, playerData.bombSkin, playerData.explosionSkin);

            var botManager = player.GetComponent<BotManager>();
            if (!botManager) {
                return;
            }

            if (playerData.hp > 0 && playerData.stage == HeroStage.Working) {
                botManager.ForceWork();
            } else if (playerData.stage == HeroStage.Home || playerData.active == false) {
                Players.Remove(player);
                player.Kill(false);
            } else {
                if (isDangerous) {
                    botManager.ForceWork();
                } else if (playerData.stage == HeroStage.Working) {
                    botManager.GoToSleep_SendRequest();
                } else {
                    botManager.ForceSleep();
                }
            }
        }

        private void SetAbilities(Player player, PlayerAbility ability) {
            switch (ability) {
                case PlayerAbility.TreasureHunter:
                    player.Bombable.TreasureHunter = true;
                    break;
                case PlayerAbility.JailBreaker:
                    player.Bombable.JailBreaker = true;
                    break;
                case PlayerAbility.PierceBlock:
                    player.Bombable.ThroughBrick = true;
                    break;
                case PlayerAbility.SaveBattery:
                    player.Health.SaveBattery = true;
                    break;
                case PlayerAbility.FastCharge:
                    break;
                case PlayerAbility.BombPass:
                    player.SetBombThroughAble(true);
                    break;
                case PlayerAbility.BlockPass:
                    player.SetBrickThroughAble(true);
                    break;
            }
        }

        public bool SpawnBomb(int slot) {
            var player = Players[slot];
            if (player == null || !player.IsAlive || player.IsInJail) {
                return false;
            }
            var playerLocation = player.GetTileLocation();
            if (playerLocation == _entityManager.MapManager.GetDoorLocation()) {
                return false;
            }
            if (!_entityManager.MapManager.IsEmpty(player.GetTileLocation())) {
                return false;
            }
            var spawn = player.Bombable.Spawn();
            if (spawn) {
                player.HadOutOfBomb = false;
            }
            return spawn;
        }

        public bool CheckSpawnPvpBomb(int slot) {
            var player = Players[slot];
            if (player == null || !player.IsAlive) {
                return false;
            }
            if (player.IsInJail) {
                // Cannot plant bomb when imprisoned.
                return false;
            }
            var playerLocation = player.GetTileLocation();
            if (playerLocation == _entityManager.MapManager.GetDoorLocation()) {
                return false;
            }
            if (!_entityManager.MapManager.IsEmpty(player.GetTileLocation())) {
                return false;
            }
            return player.Bombable.CanPlantBomb();
        }

        public int CountPlantedBomb(int slot) {
            return Players[slot].Bombable.CountPlantedBomb();
        }

        private PlayerPvp GetPlayerPvp(int slot) {
            return (PlayerPvp) Players[slot];
        }

        public void SpawnPvpBomb(int slot, int id, int range, Vector2Int position) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.SpawnBomb(id, range, position);
        }

        public void ExplodePvpBomb(int slot, int id, Dictionary<Direction, int> ranges) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.ExplodeBomb(id, ranges, true);
        }

        public void RemoveBomb(int slot, int id) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.RemoveBomb(id);
        }

        public void SetPlayerInJail(int slot) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.SetPlayerInJail();
        }

        public void AddItemToPlayer(bool playSound, int slot, HeroItem item, int value) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.AddItem(playSound, item, value);
        }

        public void SetShieldToPlayer(int slot) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.SetShield(8, null);
        }

        public void JailBreak(int slot) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.JailBreak();
        }

        public void SetSkullHeadToPlayer(bool playSound, int slot, HeroEffect effect, int duration) {
            var playerPvp = GetPlayerPvp(slot);
            playerPvp.StartSkullHeadEffect(playSound, effect, duration);
        }

        public void RequestEnterDoor(int slot, Door door) {
            var player = Players[slot];
            player.SetFreeze(true);
            player.transform.localPosition = door.transform.localPosition;
            _entityManager.LevelManager.EnterDoor();
        }

        public void RequestTakeItem(Item item) {
            _entityManager.LevelManager.RequestTakeItem(item);
        }

        public void ExitPlayer(int slot) {
            var player = Players[slot];
            if (player == null || !player.IsAlive) {
                return;
            }

            _soundManager.StopMusic();
            _soundManager.PlaySound(Audio.DoorExit);
            player.ForceUpdateFace(Vector2.down);
            player.GetComponent<Dropper>().IsExitFromDoor = true;
            player.Kill(true);
        }

        public void OnAfterPlayerDie() {
            if (_entityManager.LevelManager.IsPvpMode) {
                return;
            }
            _entityManager.LevelManager.LevelCompleted(false, null);
        }

        public void SaveHeroTakeDamageInfo(bool allowRevive, bool isAds, int gemsValue, Vector2Int lastPlayerLocation) {
            TakeDamageInfo = new HeroTakeDamageInfo {
                LastPlayerLocation = lastPlayerLocation,
                AllowRevive = allowRevive,
                IsReviveAds = isAds,
                ReviveGemValue = gemsValue
            };
        }

        public void Revive(int slot, bool isTesting) {
            Players.RemoveAt(slot);
            var playerData = GetPlayerData(_entityManager.LevelManager.StoryModeHero);
            AddPlayer(isTesting? _startingPosition : TakeDamageInfo.LastPlayerLocation, playerData, slot, false);
            Players[slot].SetItem(ItemType.Armor);
            _entityManager.LevelManager.OnUpdateItem(slot, ItemType.BombUp, playerData.bombNum);
            _entityManager.LevelManager.OnUpdateItem(slot, ItemType.FireUp, playerData.bombRange);
            _entityManager.LevelManager.OnUpdateItem(slot, ItemType.Boots, (int) playerData.speed);
        }

        public void ShowHealthBarOnPlayerNotSlot(int slot) {
            for (var i = 0; i < Players.Count; i++) {
                if (i == slot) {
                    Players[i].Health.SetShowHealthBar(false);
                    continue;
                }
                Players[i].Health.SetShowHealthBarWhenFull(true);
            }
        }

        public void PlayKillEffectOnOther(int slot) {
            for (var i = 0; i < Players.Count; i++) {
                if (i == slot) {
                    continue;
                }
                Players[i].PlayKillEffect();
            }
        }

        public PlayerData GetPlayerDataRaw(int slot) {
            return _dicPlayerData[slot];
        }
        
        public int GetActivePlayerQuantity() {
            var quantity = 0;
            foreach (var player in _players)
            {
                if (player != null) {
                    quantity += 1;
                }
            }
            return quantity;
        }

        public int GetDicPlayersSlot(HeroId id) {
            var slot = 0;
            foreach (var player in _dicPlayers) {
                if (player.Value.HeroId == id) {
                    return player.Key;
                }
                if (player.Value.Slot >= slot) {
                    slot = player.Value.Slot + 1;
                }
            }
            return slot;
        }

        //Hàm này tính active player cả trong pending để tự động thêm hero khi buy
        public int GetAllActivePlayerQuantity() {
            var quantity = 0;
            foreach (var player in _players) {
                if (player == null)
                    continue;
                // kiểm tra hero bị deactive
                if (_pendingActivePlayers.ContainsKey(player.HeroId)) {
                    if (!_pendingActivePlayers[player.HeroId]) {
                        continue;
                    }
                }
                quantity += 1;
            }

            // kiểm tra hero được active mới
            foreach (var heroID in _pendingActivePlayers) {
                if (!heroID.Value)
                    continue;
                if (GetPlayerById(heroID.Key) != null) {
                    continue;
                }
                quantity += 1;
            }
            return quantity;
        }

        public void AddPendingActiveHeroes(HeroId player, bool isActive) {
            _pendingActivePlayers[player] = isActive;
        }
    }
}