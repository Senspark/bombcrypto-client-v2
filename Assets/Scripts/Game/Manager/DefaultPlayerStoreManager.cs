using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeStage.AntiCheat.ObscuredTypes;
using Constant;
using Engine.Entities;
using Engine.Manager;
using Game.Dialog;
using PvpMode.Services;
using Scenes.FarmingScene.Scripts;
using Senspark;
using Sfs2X.Entities.Data;
using UnityEngine;

namespace App {
    [Serializable]
    public class PlayerData {
        public delegate PowerData GetPowerData(int rare);

        public string genId;
        public int itemId;
        public HeroId heroId;
        public PlayerType playerType;
        public PlayerColor playercolor;

        public ObscuredFloat bombDamage;
        public ObscuredFloat speed;
        public ObscuredFloat stamina;
        public ObscuredInt bombNum;
        public ObscuredInt bombRange;
        public ObscuredInt bombSkin;
        public ObscuredInt explosionSkin;

        public PlayerAbility[] abilities;

        public ObscuredInt level;
        public ObscuredInt levelShield;
        public ObscuredInt numResetShield;
        public int rare;

        public ObscuredFloat hp;
        public ObscuredFloat maxHp;
        public HeroStage stage;
        public ObscuredBool active;
        public ObscuredFloat storyHp;
        public ObscuredBool storyIsPlayed;
        public ObscuredLong timeUnlock;
        public ObscuredInt battery = 3;
        public ObscuredLong timeRefillBattery;

        public ObscuredString timeSync;
        public int randomAbilityCounter;
        public bool IsHeroS;
        public HeroAccountType AccountType;
        public IHeroSAbility Shield;
        public IDictionary<int, List<int>> skinChests;
        public long timeLockSince;
        public int timeLockSeconds;
        
        public double stakeBcoin;
        public double stakeSen;

        public Dictionary<StatId, int> MaximumStats { get; set; }
        public Dictionary<StatId, int> UpgradeStats { get; set; }
        public Dictionary<StatId, int> MaxUpgradeStats { get; set; }

        public GetPowerData getPowerData =
            parameter => ServiceLocator.Instance.Resolve<IStorageManager>().GetPowerData(parameter);

        public float GetUpgradePower(int lv = 0) {
            if (lv == 0) {
                lv = level - 1;
            }

            var powers = getPowerData(rare);
            if (powers != null) {
                if (lv >= 0 && lv < powers.power.Length) {
                    return powers.power[lv];
                }
            }
            return 0;
        }
        
        public bool HaveAnyStaked() {
            return Math.Floor(stakeBcoin * 1e9) / 1e9 > 0 || Math.Floor(stakeSen * 1e9) / 1e9 > 0;
        }

        public float GetMaxUpgrade() {
            var storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            var powers = storeManager.GetPowerData(rare);
            if (powers != null) {
                return powers.power.Length;
            }
            return 0;
        }
    }

    public class MapData {
        public EntityType entityType;
        public float hp { get; }
        public float maxHp { get; }

        public MapData(EntityType t, float h, float maxH) {
            entityType = t;
            hp = h;
            maxHp = maxH;
        }
    }

    public class DefaultPlayerStoreManager : IPlayerStorageManager {
        public class BlockData {
            public int i;
            public int j;
            public int type;
            public float hp;
            public float maxHp;
            public BlockReward[] rewards;
        }

        public class BlockReward {
            public string type;
            public float value;
        }

        private class MapDataBundle {
            public MapData[,] MapData;
            public IMapDetails MapDetails;
        }

        public MapData[,] MapDatas { get; private set; }
        public int TileSet { get; private set; }

        private int _totalHeroes;
        private int _totalHeroesSize;

        private MapDataBundle _mapDataBundle;
        private IHouseStorageManager _houseStorage;

        private readonly Dictionary<HeroId, IHeroDetails> _playerDataById;
        private readonly Dictionary<HeroAccountType, List<IHeroDetails>> _playerData;
        private readonly List<HeroId> _activePlayers;
        private readonly List<HeroId> _homePlayers;

        private Dictionary<HeroAccountType, List<IHeroDetails>> _lockedHeroesData;
        private Dictionary<HeroAccountType, List<List<IHeroDetails>>> _activeFirst;
        private Dictionary<HeroAccountType, List<List<IHeroDetails>>> _unActiveFirst;
        private Dictionary<HeroAccountType, List<List<IHeroDetails>>> _activeLockedFirst;
        private Dictionary<HeroAccountType, List<List<IHeroDetails>>> _unActiveLockedFirst;
        
        public DefaultPlayerStoreManager(IHouseStorageManager houseStorage) {
            _houseStorage = houseStorage;
            _playerDataById = new Dictionary<HeroId, IHeroDetails>();
            _playerData = new Dictionary<HeroAccountType, List<IHeroDetails>>();
            _activeFirst = new Dictionary<HeroAccountType, List<List<IHeroDetails>>>();
            _unActiveFirst = new Dictionary<HeroAccountType, List<List<IHeroDetails>>>();
            _activeLockedFirst = new Dictionary<HeroAccountType, List<List<IHeroDetails>>>();
            _unActiveLockedFirst = new Dictionary<HeroAccountType, List<List<IHeroDetails>>>();
            foreach (HeroAccountType v in Enum.GetValues(typeof(HeroAccountType))) {
                _playerData[v] = new List<IHeroDetails>();
                _activeFirst[v] = new List<List<IHeroDetails>>();
                _unActiveFirst[v] = new List<List<IHeroDetails>>();
                _activeLockedFirst[v] = new List<List<IHeroDetails>>();
                _unActiveLockedFirst[v] = new List<List<IHeroDetails>>();
            }
            _activePlayers = new List<HeroId>();
            _homePlayers = new List<HeroId>();
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void LoadMap(GameModeType type) {
            var i = type switch {
                GameModeType.TreasureHunt => 0,
                GameModeType.TreasureHuntV2 => 1,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            LoadMap(_mapDataBundle);
            TileSet = _mapDataBundle.MapDetails.Tileset;
            MapDatas = _mapDataBundle.MapData;
        }

        private static void LoadMap(MapDataBundle bundle) {
            var details = bundle.MapDetails;

            if (bundle.MapData != null) {
                return;
            }

            const int col = DefaultMapManager.R_COL;
            const int row = DefaultMapManager.R_ROW;

            bundle.MapData = new MapData[col, row];
            var data = bundle.MapData;
            for (var i = 0; i < col; i++) {
                for (var j = 0; j < row; j++) {
                    data[i, j] = null;
                }
            }

            for (var k = 0; k < details.Blocks.Length; k++) {
                var b = details.Blocks[k];

                var entityType = (EntityType)(b.Type + 5);
                var mapData = new MapData(entityType, b.Health, b.MaxHealth);

                // fix non-fatal: Index was outside the bounds of the array.
                if ((b.Position.x >= 0 && b.Position.y < col) &&
                    (b.Position.y >= 0 && b.Position.y < row)) {
                    data[b.Position.x, b.Position.y] = mapData;
                }
            }
        }

        public void SetMapDetails(IMapDetails details) {
            _mapDataBundle = new MapDataBundle {
                MapData = null,
                MapDetails = details
            };
        }

        public void LoadPlayerFromServer(IHeroDetails[] details) {
            _playerDataById.Clear();
            foreach (var kv in _playerData) {
                kv.Value.Clear();
            }
            _activePlayers.Clear();
            _homePlayers.Clear();
            _totalHeroes = 0;

            AddHeroes(details);
        }

        public void LoadLockedHeroesFromServer(IHeroDetails[] details) {
            _lockedHeroesData = new Dictionary<HeroAccountType, List<IHeroDetails>>();
            foreach (HeroAccountType v in Enum.GetValues(typeof(HeroAccountType))) {
                _lockedHeroesData[v] = new List<IHeroDetails>();
            }

            foreach (var kv in _lockedHeroesData) {
                kv.Value.Clear();
            }

            AddHeroLocked(details);
        }

        public void AddHeroServer(IHeroDetails[] details) {
            AddHeroes(details);
            AdjustTotalHeroesSize(details.Length);
        }

        public Dictionary<HeroAccountType, List<IHeroDetails>> GetLockedHeroesData() {
            return _lockedHeroesData;
        }

        public void SetTotalHeroesSize(int value) {
            _totalHeroesSize = value;
        }

        public void AdjustTotalHeroesSize(int value) {
            _totalHeroesSize += value;
        }

        private void AddHeroes(IHeroDetails[] heroes) {
            foreach (var t in heroes) {
                var id = new HeroId(t.Id, t.AccountType);
                _playerDataById[id] = t;
                _playerData[t.AccountType].Add(t);
                UpdateHeroState(id, t.Stage, false);
                UpdateHeroActiveState(id, t.IsActive, false);
                _totalHeroes++;
            }
            RefreshSortedData();
        }
        
        private void AddHeroLocked(IHeroDetails[] heroes) {
            foreach (var t in heroes) {
                _lockedHeroesData[t.AccountType].Add(t);
            }
            AddSortHeroLockedData();
        }
        
        private void AddSortHeroLockedData() {
            foreach (var kv in _lockedHeroesData) {
                var active = kv.Value.OrderByDescending(e => e.IsActive);
                var unActive = kv.Value.OrderByDescending(e => !e.IsActive);

                var activeStats = active.ThenByDescending(e => e.BombPower + e.Speed + e.Stamina + e.BombCount + e.BombRange);
                var activeRarity = active.ThenByDescending(e => e.Rarity);
                var activeNewest = active.ThenByDescending(e => e.Id);
                
                var unActiveStats = unActive.ThenByDescending(e => e.BombPower + e.Speed + e.Stamina + e.BombCount + e.BombRange);
                var unActiveRarity = unActive.ThenByDescending(e => e.Rarity);
                var unActiveNewest = unActive.ThenByDescending(e => e.Id);
                
                var activeList = new List<List<IHeroDetails>>();
                activeList.Add(activeStats.ToList());
                activeList.Add(activeRarity.ToList());
                activeList.Add(activeNewest.ToList());
                _activeLockedFirst[kv.Key] = activeList;

                var unActiveList = new List<List<IHeroDetails>>();
                unActiveList.Add(unActiveStats.ToList());
                unActiveList.Add(unActiveRarity.ToList());
                unActiveList.Add(unActiveNewest.ToList());
                _unActiveLockedFirst[kv.Key] = unActiveList;
            }
        }

        public void RemoveBurnHeroes(HeroId[] lstHeroesIdBurn) {
            foreach (var heroId in lstHeroesIdBurn) {
                _playerDataById.Remove(heroId);
                _activePlayers.Remove(heroId);
                _homePlayers.Remove(heroId);
                _playerData[heroId.Type].RemoveAll(data => data.Id == heroId.Id);
                _totalHeroes--;
            }
            RefreshSortedData();
        }

        private void RefreshSortedData() {
            ClearSortedData();
            AddSortData();
        }
        
        private void ClearSortedData() {
            foreach (var kv in _activeFirst) {
                kv.Value.Clear();
            }
            foreach (var kv in _unActiveFirst) {
                kv.Value.Clear();
            }
        }

        private void AddSortData() {
            foreach (var kv in _playerData) {
                var active = kv.Value.OrderByDescending(e => e.IsActive);
                var unActive = kv.Value.OrderByDescending(e => !e.IsActive);

                var activeStats = active.ThenByDescending(e => e.BombPower + e.Speed + e.Stamina + e.BombCount + e.BombRange);
                var activeRarity = active.ThenByDescending(e => e.Rarity);
                var activeNewest = active.ThenByDescending(e => e.Id);
                
                var unActiveStats = unActive.ThenByDescending(e => e.BombPower + e.Speed + e.Stamina + e.BombCount + e.BombRange);
                var unActiveRarity = unActive.ThenByDescending(e => e.Rarity);
                var unActiveNewest = unActive.ThenByDescending(e => e.Id);
                
                var activeList = new List<List<IHeroDetails>>();
                activeList.Add(activeStats.ToList());
                activeList.Add(activeRarity.ToList());
                activeList.Add(activeNewest.ToList());
                _activeFirst[kv.Key] = activeList;

                var unActiveList = new List<List<IHeroDetails>>();
                unActiveList.Add(unActiveStats.ToList());
                unActiveList.Add(unActiveRarity.ToList());
                unActiveList.Add(unActiveNewest.ToList());
                _unActiveFirst[kv.Key] = unActiveList;
            }
        }

        public void UpdatePlayerHpFromServer(IHeroDetails detail) {
            var player = GetHeroDetailsFromId(new HeroId(detail.Id, detail.AccountType));
            if (player == null) {
                return;
            }
            player.Energy = detail.Energy;
            player.TimeSync = DateTime.Now.ToBinary();    
        }

        public void UpdateHeroSShield(HeroId heroId, List<IHeroSAbility> abilities) {
            var player = GetHeroDetailsFromId(heroId);
            if (player == null) {
                return;
            }
            // Check ko cần thiết trên editor, bỏ qua
            // if (player.IsHeroS || player.Shield != null) {
            //     Assert.IsTrue(abilities.Count > 0);
            // }
            player.Shield = abilities.Find(e => e.AbilityType == HeroSAbilityType.AvoidThunder);
        }

        public void UpdateHeroActiveState(HeroId heroId, bool active, bool refreshSort = true) {
            var player = GetHeroDetailsFromId(heroId);
            if (player == null) {
                return;
            }
            player.IsActive = active;
            var playerHeroId = new HeroId(player.Id, player.AccountType);
            _activePlayers.Remove(playerHeroId);
            if (player.IsActive) {
                _activePlayers.Add(playerHeroId);
            }
            if (refreshSort) {
                RefreshSortedData();
            }
        }

        public void UpdateHeroState(HeroId heroId, HeroStage newState, bool refreshSort = true) {
            var player = GetHeroDetailsFromId(heroId);
            if (player == null) {
                return;
            }
            player.Stage = newState;
            var playerHeroId = new HeroId(player.Id, player.AccountType);
            _homePlayers.Remove(playerHeroId);
            if (player.Stage == HeroStage.Home) {
                _homePlayers.Add(playerHeroId);
                _houseStorage.HeroRestInHouse(heroId.Id);
            } else {
                _houseStorage.HeroLeaveHouse(heroId.Id);
            }
            if (refreshSort) {
                RefreshSortedData();
            }
        }

        public List<PlayerData> GetInHomePlayers() {
            var list = _homePlayers.Select(e => _playerDataById[e]).ToList();
            var result = new List<PlayerData>();
            foreach (var t in list) {
                result.Add(GeneratePlayerData(t));
            }
            return result;
        }

        public List<PlayerData> GetPlayerDataList(HeroAccountType type, params HeroAccountType[] types) {
            var list = new List<IHeroDetails>();
            var allTypes = new List<HeroAccountType>();
            allTypes.Add(type);
            allTypes.AddRange(types);
            allTypes = allTypes.Distinct().ToList();
            foreach (var t in allTypes) {
                list.AddRange(_playerData[t]);
            }

            var result = new List<PlayerData>();
            foreach (var t in list) {
                result.Add(GeneratePlayerData(t));
            }
            return result;
        }

        public (List<IHeroDetails>, int, List<int>) GetSortedPlayerDataList(
            DialogInventory.SortRarity orderRarity,
            int targetRarity,
            DialogInventory.SortOrder1 order1,
            DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3,
            DialogInventory.ActiveFilter filterActive,
            int[] selectedHeroIds,
            HeroId[] excludeHeroIds,
            int pageIndex, int itemsPage,
            HeroAccountType type, params HeroAccountType[] types
            ) {

            var list = new List<IHeroDetails>();
            var allTypes = new List<HeroAccountType>();
            allTypes.Add(type);
            allTypes.AddRange(types);
            allTypes = allTypes.Distinct().ToList();
            foreach (var t in allTypes) {
                var sortedList = new List<IHeroDetails>();
                if (filterActive == DialogInventory.ActiveFilter.Locked) {
                    sortedList = order1 switch {
                        DialogInventory.SortOrder1.ActiveFirst => _activeLockedFirst[t].Count == 0 ? new List<IHeroDetails>() : order2 switch {
                            DialogInventory.SortOrder2.HighStatsFirst => _activeLockedFirst[t][0],
                            DialogInventory.SortOrder2.HighRarityFirst => _activeLockedFirst[t][1],
                            DialogInventory.SortOrder2.NewestFirst => _activeLockedFirst[t][2],
                            _ => throw new ArgumentOutOfRangeException(nameof(order2), order2, null)
                        },
                        DialogInventory.SortOrder1.UnActiveFirst => _unActiveLockedFirst[t].Count == 0 ? new List<IHeroDetails>() : order2 switch {
                            DialogInventory.SortOrder2.HighStatsFirst => _unActiveLockedFirst[t][0],
                            DialogInventory.SortOrder2.HighRarityFirst => _unActiveLockedFirst[t][1],
                            DialogInventory.SortOrder2.NewestFirst => _unActiveLockedFirst[t][2],
                            _ => throw new ArgumentOutOfRangeException(nameof(order2), order2, null)
                        },
                        _ => throw new ArgumentOutOfRangeException(nameof(order1), order1, null)
                    };
                } else {
                    sortedList = order1 switch {
                        DialogInventory.SortOrder1.ActiveFirst => _activeFirst[t].Count == 0 ? new List<IHeroDetails>() : order2 switch {
                            DialogInventory.SortOrder2.HighStatsFirst => _activeFirst[t][0],
                            DialogInventory.SortOrder2.HighRarityFirst => _activeFirst[t][1],
                            DialogInventory.SortOrder2.NewestFirst => _activeFirst[t][2],
                            _ => throw new ArgumentOutOfRangeException(nameof(order2), order2, null)
                        },
                        DialogInventory.SortOrder1.UnActiveFirst => _unActiveFirst[t].Count == 0 ? new List<IHeroDetails>() : order2 switch {
                            DialogInventory.SortOrder2.HighStatsFirst => _unActiveFirst[t][0],
                            DialogInventory.SortOrder2.HighRarityFirst => _unActiveFirst[t][1],
                            DialogInventory.SortOrder2.NewestFirst => _unActiveFirst[t][2],
                            _ => throw new ArgumentOutOfRangeException(nameof(order2), order2, null)
                        },
                        _ => throw new ArgumentOutOfRangeException(nameof(order1), order1, null)
                    };
                }
                
                sortedList = order3 switch {
                    DialogInventory.HeroTypeFilter.AllHeroesType => sortedList,
                    DialogInventory.HeroTypeFilter.HeroOnly => sortedList.Where(e => !e.IsHeroS).ToList(),
                    DialogInventory.HeroTypeFilter.HeroSOnly => sortedList.Where(e => e.IsHeroS || (!e.IsHeroS && e.Shield != null)).ToList(),
                    _ => throw new ArgumentOutOfRangeException(nameof(order3), order3, null),
                };

                sortedList = filterActive switch {
                    DialogInventory.ActiveFilter.All => sortedList,
                    DialogInventory.ActiveFilter.Selected => FilterSelectedOnly(sortedList, selectedHeroIds),
                    DialogInventory.ActiveFilter.Active => sortedList.Where(e => e.IsActive).ToList(),
                    DialogInventory.ActiveFilter.UnActive => sortedList.Where(e => !e.IsActive).ToList(),
                    DialogInventory.ActiveFilter.Locked => sortedList,
                    _ => throw new ArgumentOutOfRangeException(nameof(filterActive), filterActive, null),
                };

                sortedList = orderRarity switch {
                    DialogInventory.SortRarity.Default => sortedList,
                    DialogInventory.SortRarity.BelowOneLevel => sortedList.Where(e => e.Rarity == targetRarity).ToList(),
                    DialogInventory.SortRarity.BelowThanOneLevel => sortedList.Where(e => e.Rarity < targetRarity).ToList(),
                    _ => throw new ArgumentOutOfRangeException(nameof(orderRarity), orderRarity, null)
                };
                
                list.AddRange(sortedList);
            }
            
            // exclude heroids trước khi phân trang
            list = FilterExcludeHeroes(list, excludeHeroIds);
            var totalHeroIds = new List<int>();
            foreach (var item in list) {
                totalHeroIds.Add(item.Id);
            }
            
            var totalPage = GetTotalPage(list.Count, itemsPage);
            var index = pageIndex * itemsPage;
            var result = list.Skip(index)
                .Take(itemsPage).ToList();
            
            return (result, totalPage, totalHeroIds);
        }
        
        public (List<PlayerData>, int, List<int>) GetSortedPlayerDataList(
            List<PlayerData> input,
            DialogInventory.SortRarity orderRarity,
            int targetRarity,
            DialogInventory.SortOrder2 order2,
            DialogInventory.HeroTypeFilter order3,
            int pageIndex, int itemsPage
            ) {
            input = orderRarity switch {
                DialogInventory.SortRarity.Default => input,
                DialogInventory.SortRarity.BelowOneLevel => input.Where(e => e.rare == targetRarity).ToList(),
                DialogInventory.SortRarity.BelowThanOneLevel => input.Where(e => e.rare < targetRarity).ToList(),
                _ => throw new ArgumentOutOfRangeException(nameof(orderRarity), orderRarity, null)
            };

            var result1 = order3 switch {
                DialogInventory.HeroTypeFilter.AllHeroesType => input,
                DialogInventory.HeroTypeFilter.HeroOnly => input.Where(e => !e.IsHeroS),
                DialogInventory.HeroTypeFilter.HeroSOnly => input.Where(e => e.IsHeroS || (!e.IsHeroS && e.Shield != null)),
                _ => throw new ArgumentOutOfRangeException(nameof(order3), order3, null),
            };

            var result2 = order2 switch {
                DialogInventory.SortOrder2.HighStatsFirst =>
                    result1.OrderByDescending(
                        e => e.bombDamage + e.speed + e.stamina + e.bombNum + e.bombRange),
                DialogInventory.SortOrder2.HighRarityFirst =>
                    result1.OrderByDescending(e => e.rare),
                DialogInventory.SortOrder2.NewestFirst =>
                    result1.OrderByDescending(e => e.heroId),
                _ => throw new ArgumentOutOfRangeException(nameof(order2), order2, null)
            };

            var sortList = result2.ToList();
            var totalHeroIds = new List<int>();
            foreach (var item in sortList) {
                totalHeroIds.Add(item.itemId);
            }
            var totalPage = GetTotalPage(sortList.Count, itemsPage);
            var index = pageIndex * itemsPage;
            var result = sortList.Skip(index)
                .Take(itemsPage).ToList();
            
            return (result, totalPage, totalHeroIds);
        }
        
        public List<PlayerData> GetSortedPlayerForSkinFusion(
            int targetRarity,
            PlayerType playerType,
            HeroId[] excludeHeroIds,
            HeroAccountType type
            ) {
            var sortedList = _unActiveFirst[type][0];
            sortedList = sortedList.Where(e => e.Rarity == targetRarity).ToList();
            sortedList = FilterExcludeHeroes(sortedList, excludeHeroIds);
            var result = new List<PlayerData>();
            foreach (var t in sortedList) {
                result.Add(GeneratePlayerData(t));
            }
            result = result.Where(e => e.playerType == playerType).ToList();
            return result;
        }

        private List<IHeroDetails> FilterExcludeHeroes(List<IHeroDetails> list, HeroId[] excludeHeroIds) {
            if (excludeHeroIds == null) {
                return list;
            }
            foreach (var id in excludeHeroIds) {
                var item = list.FirstOrDefault(e => 
                    new HeroId(e.Id, e.AccountType) == id);
                if (item != null) {
                    list.Remove(item);
                }
            }
            return list;
        }

        private List<IHeroDetails> FilterSelectedOnly(List<IHeroDetails> list, int[] selectedIds) {
            var result = new List<IHeroDetails>();
            if (selectedIds == null || selectedIds.Length == 0) {
                return result;
            }
            result.AddRange(list.Where(hero => selectedIds.Contains(hero.Id)));
            return result;
        }
        
        private static int GetTotalPage(int count, int itemsPage) {
            if (count > 0) {
                return ((count - 1) / itemsPage) + 1;
            }
            return 0;
        }
        
        public List<PlayerData> GetActivePlayerData() {
            var list = _activePlayers.Select(e => _playerDataById[e]).ToList();
            var result = new List<PlayerData>();
            foreach (var t in list) {
                result.Add(GeneratePlayerData(t));
            }
            return result;
        }

        public List<PlayerData> GetInMapPlayerData() {
            return GetActivePlayerData().Where(e => e.stage != HeroStage.Home).ToList();
        }

        public List<PlayerData> GetPlayerDataList(HeroId[] ids) {
            var list = _playerDataById.Where(e => ids.Contains(e.Key)).Select(e => e.Value).ToList();
            var result = new List<PlayerData>();
            foreach (var t in list) {
                result.Add(GeneratePlayerData(t));
            }
            return result;
        }

        private IHeroDetails GetHeroDetailsFromId(HeroId id) {
            if (id.IsInvalid()) {
                return null;
            }
            if (!_playerDataById.ContainsKey(id)) {
                return null;
            }
            return _playerDataById[id];
        }
        
        public PlayerData GetPlayerDataFromId(HeroId id) {
            if (id.IsInvalid()) {
                return null;
            }
            if (!_playerDataById.ContainsKey(id)) {
                return null;
            }
            return GeneratePlayerData(_playerDataById[id]);
        }

        public static PlayerData GeneratePlayerData(IHeroDetails data) {
            var playerData = new PlayerData();

            playerData.playerType = GetPlayerType(data.Skin - 1);
            playerData.playercolor = GetPlayerColor(data.Color - 1);

            playerData.genId = data.Details;
            playerData.heroId = new HeroId(data.Id, data.AccountType);

            playerData.bombDamage = data.BombPower;
            playerData.speed = data.Speed;
            playerData.stamina = data.Stamina;
            playerData.bombNum = data.BombCount;
            playerData.bombRange = data.BombRange;
            // All Heroes has default bombSkin = 0 and explosionSkin = 0;
            playerData.bombSkin = 0; //data.BombSkin - 1;
            playerData.explosionSkin = 0;

            playerData.level = data.Level;
            playerData.levelShield = data.LevelShield;
            playerData.numResetShield = data.NumResetShield;
            playerData.rare = data.Rarity;
            playerData.maxHp = data.Stamina * 50;
            playerData.hp = Mathf.Clamp(data.Energy, 0, playerData.maxHp);

            playerData.stage = data.Stage;
            playerData.active = data.IsActive;
            playerData.storyHp = data.StoryHp;
            playerData.storyIsPlayed = data.StoryIsPlayed;

            playerData.abilities = new Engine.Entities.PlayerAbility[data.Abilities.Length];
            for (var i = 0; i < data.Abilities.Length; i++) {
                playerData.abilities[i] = (PlayerAbility)(data.Abilities[i] - 1);
            }

            playerData.timeSync = data.TimeSync.ToString();
            playerData.randomAbilityCounter = data.RandomizeAbilityCounter;
            playerData.IsHeroS = data.IsHeroS;
            playerData.AccountType = data.AccountType;
            playerData.Shield = data.Shield;
            playerData.timeLockSince = data.TimeLockSince;
            playerData.timeLockSeconds = data.TimeLockSeconds;
            
            playerData.stakeBcoin = data.StakeBcoin;
            playerData.stakeSen = data.StakeSen;
            return playerData;
        }


        public static PlayerData GenerateOtherPlayerData(ISFSObject data) {
            var itemId = 15; // 15 => PlayerType.Ninja,
            var range = 0;
            var speed = 0;
            var bomb = 0;
            var hp = 0;
            var damage = 0;
            if (data != null) {
                itemId = data.GetInt("item_id");
                range = data.GetInt("range_upgrade");;
                speed = data.GetInt("speed_upgrade");
                bomb = data.GetInt("bomb_upgrade");
                hp = data.GetInt("hp_upgrade");
                damage = data.GetInt("dmg_upgrade");
            }
            
            var playerData = new PlayerData() {
                itemId = itemId,
                playerType = UIHeroData.ConvertFromHeroId(itemId),
                playercolor = PlayerColor.HeroTr
            };

            var upgradeStats = new Dictionary<StatId, int>();
            upgradeStats[StatId.Range] = range;
            upgradeStats[StatId.Speed] = speed;
            upgradeStats[StatId.Count] = bomb;
            upgradeStats[StatId.Health] = hp;
            upgradeStats[StatId.Damage] = damage;
            playerData.UpgradeStats = upgradeStats;

            return playerData;
        }
        
        public int GetTotalHeroesSize() {
            return _totalHeroesSize;
        }
        
        public int GetPlayerCount() {
            return _totalHeroes;
        }

        public int GetActivePlayersAmount() {
            return _activePlayers.Count;
        }

        public int GetHomePlayerCount() {
            return _homePlayers.Count;
        }

        public int GetInMapPlayerCount() {
            return GetActivePlayerData().Count(e => e.stage != HeroStage.Home);
        }

        public void UpdatePlayerPvpBattery(ISyncPvPHeroesResult pvpDetails) {
            foreach (var hero in pvpDetails.HeroEnergies) {
                var heroId = new HeroId((int)hero.Id, HeroAccountType.Nft);
                var player = GetPlayerDataFromId(heroId);
                if (player == null) {
                    continue;
                }

                player.battery = hero.Balance;
                player.timeRefillBattery = hero.RemainingTime;
            }
        }
        
        public void ForceUpdateHero(HeroId heroId, IHeroDetails detail) {
            var player = GetHeroDetailsFromId(heroId);
            if (player == null) {
                return;
            }
            player.StakeBcoin = detail.StakeBcoin;
            player.StakeSen = detail.StakeSen;
            if (detail.HeroSAbilities.Count == 0) {
                player.Shield = null;
                return;
            }
            player.Shield = detail.HeroSAbilities.Find(e => e.AbilityType == HeroSAbilityType.AvoidThunder);
        }

        public HeroRarity GetHeroRarity(PlayerData player) {
            return (HeroRarity)player.rare;
        }

        public string GetHeroIndex(PlayerData player) {
            //power, range, stamina, speed, bomb
            var str = "" + player.bombDamage;
            str += "-" + player.bombRange;
            str += "-" + player.stamina;
            str += "-" + player.speed;
            str += "-" + player.bombNum;
            return str;
        }

        public string GetHeroAbility(PlayerData player) {
            var str = "";

            for (var i = 0; i < player.abilities.Length; i++) {
                var ability = (int)player.abilities[i] + 1;

                if (i == 0) {
                    str += ability.ToString();
                } else {
                    str += "-" + ability;
                }
            }
            return str;
        }

        public float SimulatePlayerHpOverTime(PlayerData player, float houseCharge) {
            if (player.stage == HeroStage.Working) {
                return player.hp;
            }

            var syncTime = DateTime.FromBinary(Convert.ToInt64(player.timeSync));
            var minutes = (float)(DateTime.Now - syncTime).TotalMinutes;
            float a;

            if (player.stage == HeroStage.Sleep) {
                a = 0.5f;
            } else {
                a = houseCharge > 0 ? houseCharge : 0.5f;
            }
            var fastCharge = Array.Exists(player.abilities, e => e == PlayerAbility.FastCharge);
            if (fastCharge) {
                a += 0.5f;
            }

            var hp = player.hp + (a * minutes);
            hp = Mathf.Min(hp, player.maxHp);
            return hp;
        }

        public string GetBlockTrackingName(EntityType entityType) {
            return entityType switch {
                EntityType.normalBlock => "normal",
                EntityType.jailHouse => "prison",
                EntityType.woodenChest => "wooden_chest",
                EntityType.silverChest => "silver_chest",
                EntityType.goldenChest => "gold_chest",
                EntityType.diamondChest => "diamond_chest",
                EntityType.legendChest => "legend_chest",
                EntityType.keyChest => "key_chest",
                EntityType.BcoinDiamondChest => "bcoin_diamond_chest",
                _ => "normal"
            };
        }

        private static PlayerType GetPlayerType(int index) {
            return index switch {
                0 => PlayerType.BomberMan,
                1 => PlayerType.Knight,
                2 => PlayerType.Man,
                3 => PlayerType.Vampire,
                4 => PlayerType.Witch,
                5 => PlayerType.Doge,
                6 => PlayerType.Pepe,
                7 => PlayerType.Ninja,
                8 => PlayerType.King,
                9 => PlayerType.PilotRabit,
                10 => PlayerType.Meo2,
                11 => PlayerType.Monkey,
                12 => PlayerType.Pilot,
                13 => PlayerType.BlackCat,
                14 => PlayerType.Tiger,
                15 => PlayerType.PugDog,
                16 => PlayerType.SailorMoon,
                17 => PlayerType.PepeClown,
                18 => PlayerType.FrogGentlemen,
                19 => PlayerType.Dragoon,
                20 => PlayerType.Ghost,
                21 => PlayerType.Pumpkin,
                22 => PlayerType.Werewolves,
                23 => PlayerType.FootballFrog,
                24 => PlayerType.FootballKnight,
                25 => PlayerType.FootballMan,
                26 => PlayerType.FootballVampire,
                27 => PlayerType.FootballWitch,
                28 => PlayerType.FootballDoge,
                29 => PlayerType.FootballPepe,
                30 => PlayerType.FootballNinja,
                31 => PlayerType.Hesman,
                _ => PlayerType.BomberMan
            };
        }

        private static PlayerColor GetPlayerColor(int index) {
            return index switch {
                0 => PlayerColor.Blue,
                1 => PlayerColor.Green,
                2 => PlayerColor.Red,
                3 => PlayerColor.White,
                4 => PlayerColor.Yellow,
                5 => PlayerColor.Skin,
                _ => PlayerColor.Blue
            };
        }
    }
}