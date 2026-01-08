using System;
using System.Collections.Generic;
using System.Linq;

using Actions;

using App;

using BLPvpMode.Data;
using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.GameView;
using BLPvpMode.Manager;
using BLPvpMode.Test;
using BLPvpMode.UI;

using BomberLand.InGame;

using Constant;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using Engine.Camera;
using Engine.Entities;
using Engine.Manager;

using Game.UI;

using Newtonsoft.Json;

using PvpMode.Manager;
using PvpMode.UI;

using Scenes.PvpModeScene.Scripts;

using Senspark;

using Services;

using SuperTiled2Unity;

using UnityEngine;

using Utils;

using ICommandManager = BLPvpMode.Manager.ICommandManager;

namespace Scenes.TutorialScene.Scripts {
    public class BLLevelScenePvpSimulator : MonoBehaviour {
        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Transform parent;

        [SerializeField]
        protected Transform mapParent;

        [SerializeField]
        protected StartCountDown startCountDown;

        [SerializeField]
        protected TextAsset textPvpMapDetail;

        [SerializeField]
        protected GameObject prefabMap;

        [SerializeField]
        private Transform guiParent;

        [SerializeField]
        private BLGuiResource guiResource;

        private bool _pause = true;
        private BLevelViewPvp _levelView;
        private ProCamera _proCamera;
        public ProCamera ProCamera => _proCamera;

        private int _participantSlot;
        public int ParticipantSlot => _participantSlot;

        private ObserverHandle _handle;

        private IMapInfo _mapDetails;
        private IMatchHeroInfo[] _pvpHeroes;
        private PvpSyncing[] _syncHeroes;

        private bool _updateSendSpawnBomb;
        private PvpStorageData _pvpStorageData;

        private BoosterStatus _boosterStatus;

        private Dictionary<int, IBLParticipantGui> _inputController;

        private PvpSimulator _pvpSimulator;

        private SuperMap _superMap;

        private IBLGui _guiPvp;

        private Action _onRequestQuitPvp;
        public BLGuiPvp GuiPvp => _guiPvp as BLGuiPvp;

        public ILevelViewPvp LevelView => _levelView;

        public StartCountDown StartCountDown => startCountDown;

        private void Awake() {
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = Vector2.zero;
            _pvpStorageData = new PvpStorageData();
            _handle = new ObserverHandle();
        }

        public ICommandManager GetCommandManager(int slot) {
            return _syncHeroes[slot].CommandManager;
        }

        private void Start() {
            if (_levelView) {
                return;
            }
            SetPvpMapDetails("Tutorial", textPvpMapDetail.text, 0);
            // _guiPvp.InitEmojiUi(new[] { 132, 133, 134, 135 });
            LoadLevel();
            InitForSimulator();
            UpdateBoostersInfinity();
            CreateSyncHeroes();
            StartGame();
        }

        private void InitGui() {
            _guiPvp = guiResource.CreateBLGui(GuiType.PvpPlay, guiParent);
            _handle.AddObserver(_guiPvp.GetParticipantGui(), new BLGuiObserver() {
                    SpawnBomb = OnBombButtonClicked, UseBooster = OnBoosterButtonClicked
                    // UseEmoji = OnEmojiClicked,
                    // RequestQuit = RequestQuitPvp
                });
        }

        private void OnDestroy() {
            DOTween.KillAll(true);
            ServiceLocator.Instance.Resolve<ISoundManager>().StopMusic();
            _handle.Dispose();
        }

        private void Update() {
            if (_pause) {
                return;
            }
            Step();
        }

        private void Step() {
            var delta = Time.deltaTime;
            if (delta > BLLevelScenePvp.FixedDeltaTime) {
                delta = BLLevelScenePvp.FixedDeltaTime;
            }
            _levelView.Step(delta);
            _guiPvp.CheckInputKeyDown();
            _proCamera.Process(delta);

            foreach (var slot in _inputController.Keys) {
                // process movement from direction input 
                if (!_levelView.GetPvpHeroBySlot(slot).IsAlive) {
                    continue;
                }
                _levelView.ProcessMovement(slot, _inputController[slot].GetDirectionFromInput());
            }

            for (var slot = 0; slot < _pvpHeroes.Length; slot++) {
                // update BombLoseControl
                _levelView.ProcessUpdatePlayer(slot);
            }

            if (_syncHeroes == null) {
                return;
            }
            _pvpSimulator?.Step(delta);
            UiUpdateAvailableBomb();
        }

        private void UiUpdateAvailableBomb() {
            var bombable = _levelView.EntityManager.PlayerManager.GetPlayerBySlot(_participantSlot).Bombable;
            _guiPvp.SetQuantityBomb(bombable.CountAvailableBomb());
        }

        public void SetPvpMapDetails(
            string matchId,
            string data,
            int participantSlot
        ) {
            InitGui();
            //var pvpMapInput = JsonConvert.DeserializeObject<PvpMapDetailMod>(textPvpMapDetail.text);
            var pvpMapInput = JsonConvert.DeserializeObject<PvpMapDetailMod>(data);
            if (prefabMap != null) {
                var map = Instantiate(prefabMap, mapParent, true);
                map.SetLayer(mapParent.gameObject.layer, true);
                var superMap = map.GetComponent<SuperMap>();
                pvpMapInput.SetEmptyMap(superMap.m_rectInGame.width, superMap.m_rectInGame.height);
                _superMap = superMap;
            } else {
                pvpMapInput.Map.TryGenItem();
            }
            _boosterStatus = new BoosterStatus(0);
            Debug.Assert(pvpMapInput != null, nameof(pvpMapInput) + " != null");
            IMapInfo mapDetail = new MapFake(pvpMapInput);
            var pvpUsers = new IMatchUserInfo[pvpMapInput.Heroes.Length];
            var heroIdManager = ServiceLocator.Instance.Resolve<IHeroIdManager>();
            var heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            for (var idx = 0; idx < pvpMapInput.Heroes.Length; idx++) {
                var hero = pvpMapInput.Heroes[idx];
                // Check Hero stat
                var stats = heroStatsManager.GetStats(heroIdManager.GetItemId(hero.Skin));
                var dicStats = stats.ToDictionary(it => it.StatId);
                var skinChests = new Dictionary<int, int[]>();

                foreach (var skin in hero.SkinChests) {
                    var key = skin[0];
                    var len = skin.Length;
                    var items = new List<int>();
                    for (var i = 1; i < len; i++) {
                        items.Add(skin[i]);
                    }
                    skinChests[key] = items.ToArray();
                }

                pvpUsers[idx] = new MatchUserInfo {
                    ServerId = "",
                    IsBot = false,
                    UserId = idx,
                    Username = "Address",
                    DisplayName = idx == 0 ? "Player" : "Computer",
                    TotalMatchCount = 0,
                    MatchCount = 0,
                    WinMatchCount = 0,
                    Rank = 0,
                    Point = 0,
                    Boosters = Array.Empty<int>(),
                    AvailableBoosters = new Dictionary<int, int>(),
                    Hero = new MatchHeroInfo {
                        Id = idx,
                        Color = hero.Color,
                        Skin = hero.Skin,
                        SkinChests = skinChests,
                        Health = hero.Health,
                        // Health = dicStats[(int) StatId.Health].Value,
                        Speed = hero.Speed,
                        // Speed = dicStats[(int) StatId.Speed].Value,
                        Damage = hero.Damage,
                        // Damage = dicStats[(int) StatId.Damage].Value,
                        BombCount = hero.BombCount,
                        // BombCount = dicStats[(int) StatId.Count].Value,
                        BombRange = hero.BombRange,
                        // BombRange = dicStats[(int) StatId.Range].Value,
                        MaxHealth = Mathf.Max(dicStats[(int) StatId.Health].Max, hero.Health),
                        MaxSpeed = Mathf.Max(dicStats[(int) StatId.Speed].Max, hero.Speed),
                        MaxDamage = Mathf.Max(dicStats[(int) StatId.Damage].Max, hero.Damage),
                        MaxBombCount = Mathf.Max(dicStats[(int) StatId.Count].Max, hero.BombCount),
                        MaxBombRange = Mathf.Max(dicStats[(int) StatId.Range].Max, hero.BombRange)
                    },
                };
            }
            _participantSlot = participantSlot;
            _inputController = new Dictionary<int, IBLParticipantGui> {
                { _participantSlot, _guiPvp.GetParticipantGui() }
            };
            _pvpHeroes = pvpUsers.Select(item => item.Hero).ToArray();
            _mapDetails = mapDetail;

            foreach (var hero in _pvpHeroes) {
                _guiPvp.InitHealthUi(hero.Id, hero.Health);
            }

            _guiPvp.UpdatePvpInfo(matchId, null, _participantSlot, pvpUsers, _boosterStatus);
            _guiPvp.UpdateDamageUi(_pvpHeroes[_participantSlot].Damage);
        }

        public void SetInput(IBLParticipantGui input, int slot) {
            _inputController[slot] = input;
        }

        public void StartGame(Action onStart = null) {
            _pause = true;
            Application.targetFrameRate = 60;
            startCountDown.StartCount(() => {
                _pause = false;
                _pvpStorageData.StartMatchTime = DateTime.Now;
                onStart?.Invoke();
            });
        }

        public void InitForSimulator() {
            _pvpSimulator = new PvpSimulator(this);
            var callback = new SimulatorExplodeEventManager.ExplodeCallback() {
                TakeDamageFromExplosions = _pvpSimulator.TakeDamageFromExplosions
            };
            _levelView.EntityManager.ExplodeEventManager =
                new SimulatorExplodeEventManager(_levelView.EntityManager, callback);
        }

        public void SimulatorSyncSyncHero() {
            _pvpSimulator.SyncHero();
        }

        public void UpdateBoostersInfinity() {
            UpdateBoostersNum(99, 99);
        }

        public void UpdateBoostersNum(int key, int shield) {
            _boosterStatus = new BoosterStatus(2);
            _boosterStatus.UpdatedItem(0, BoosterType.Key, key);
            _boosterStatus.UpdatedItem(1, BoosterType.Shield, shield);
            var coolDown = 20f;
            GuiPvp.UpdateBoosters(_boosterStatus, coolDown);
        }

        public void CreateSyncHeroes() {
            var pvpHeroes = _levelView.GetPvpHeroes();
            _syncHeroes = new PvpSyncing[pvpHeroes.Count];

            for (var idx = 0; idx < pvpHeroes.Count; idx++) {
                var slot = idx;
                var player = pvpHeroes[idx];
                var faction = idx == _participantSlot ? FactionType.Ally : FactionType.Enemy;
                var isBot = idx != _participantSlot;
                var posStart = new Vector2(
                    _mapDetails.StartingPositions[idx].x + 0.5f,
                    _mapDetails.StartingPositions[idx].y + 0.5f
                );
                var participantMoveManager = new ParticipantMoveManagerFake(posStart);
                var participantPlantBombManager = new ParticipantPlantBombManagerFake(
                    (bombId, position, range, damage) => { OnSpawnBomb(slot); }
                );
                var commandManager = new CommandManager(
                    participantMoveManager,
                    participantPlantBombManager
                );

                player.SetSlotAndFaction(idx, faction);
                player.ShowMe(idx == _participantSlot);
                // Setup SetSpawnLocation for Player and Bot
                player.Bombable.SetSpawnLocation(player.GetTileLocation);
                _syncHeroes[idx] = new PvpSyncing(player, commandManager, idx, isBot);
            }
        }

        // private void ResetWeapon(PlayerPvp hero) {
        //     var bombable = hero.Bombable;
        //     var weapon = new BombWeapon(WeaponType.Bomb,
        //         new BombSpawner(),
        //         bombable);
        //     bombable.SetWeapon(weapon);
        //     bombable.SetSpawnLocation(hero.GetTileLocation);
        // }

        public void LoadLevel() {
            _levelView = BLevelViewPvp.Create(mapParent);
            var pvpModeCallback = new PvpModeCallback {
                OnKick = OnKick,
                OnPlayerInJail = OnPlayerInJail,
                OnPlayerEndInJail = OnPlayerEndInJail,
                OnUpdateItem = OnUpdateItem,
            };
            _levelView.gameObject.SetLayer(mapParent.gameObject.layer);

            var now = new EpochTimeManager().Timestamp;
            _levelView.Initialize(
                _pvpHeroes,
                _mapDetails,
                new MatchData(
                    id: "",
                    status: (int) MatchStatus.Started,
                    observerCount: 0,
                    startTimestamp: now,
                    readyStartTimestamp: now,
                    roundStartTimestamp: now,
                    round: 0,
                    results: new List<IMatchResultInfo>()
                ),
                pvpModeCallback,
                mapParent.gameObject.layer,
                _superMap != null ? _superMap.m_mapData : null
            );

            // Hard call LevelManager.CreateItems, Item information will show when brick destroyed. 
            _levelView.EntityManager.LevelManager.CreateBlocksUnderBrick(_mapDetails.Blocks);

            _proCamera = GuiPvp.SetUpCamera(_mapDetails.Width, _mapDetails.Height);
            _proCamera.SetTarget(_levelView.EntityManager.PlayerManager.GetPlayer(_participantSlot));

            _levelView.UpdateState(_participantSlot);
            _levelView.ShowHealthBarOnPlayerNotSlot(_participantSlot);
            if (_superMap != null) {
                foreach (var steTilemap in _levelView.TilemapBackground.Tilemaps) {
                    steTilemap.gameObject.SetActive(false);
                }
                _superMap.SetUpPosAndFixSize(_levelView.transform.position);
                _superMap.SetUpSortingOrder();
            }
        }

        public void ExplodeBomb(Vector2Int location) {
            var pos = _levelView.EntityManager.MapManager.GetTilePosition(location);
            foreach (var hero in _levelView.GetPvpHeroes()) {
                hero.ExplodeBomb(pos.x, pos.y, Array.Empty<(int, int)>(), true);
            }
        }

        private void SetSkullHeadToPlayer(int slot, HeroEffect effect, int duration) {
            _levelView.SetSkullHeadToPlayer(ParticipantSlot == slot, slot, effect, duration);
        }

        private void RemoveItem(Vector2Int location) {
            _levelView.RemoveItem(location);
        }

        public void RemoveItemOnMap(Vector2Int location) {
            _levelView.RemoveItem(new Vector2Int(location.x, location.y));
        }

        private void SpawnBomb(long timestamp, int slot, Vector2Int location, int bombId, int bombRange) {
            _levelView.SpawnBomb(slot, bombId, bombRange, location);
        }

        private void DropWall() {
            _levelView.DropWall();
        }

        public TileType GetTileType(int x, int y) {
            return _levelView.GetTileType(x, y);
        }

        public ItemType GetItemType(int x, int y) {
            return _levelView.GetItemType(x, y);
        }

        public void SetItemToPlayer(int slot, int rewardValue, HeroItem item) {
            _levelView.AddItemToPlayer(ParticipantSlot == slot, slot, item, rewardValue);

            // Flying gold reward
            if (ParticipantSlot != slot) {
                return;
            }
            if (rewardValue <= 0) {
                return;
            }

            var position = _levelView.GetPvpHeroBySlot(slot).transform.position;
            _guiPvp.FlyItemReward(parent, position, item);
        }

        public void SetShielded(int slot, bool active, HeroEffectReason reason) {
            if (active) {
                _levelView.SetShieldToPlayer(slot);
                if (reason == HeroEffectReason.UseBooster) {
                    OnUseBooster(BoosterType.Shield);
                }
            } else {
                if (reason == HeroEffectReason.Damaged) {
                    _levelView.HeroShieldBroken(slot);
                }
            }
        }

        public void SetImprisoned(int slot, bool enable) {
            if (enable) {
                _levelView.SetPlayerInJail(slot);
                _pvpSimulator?.AddPrisonCounter(slot);
            } else {
                _levelView.JailBreak(slot);
                _pvpSimulator?.SetHealth(slot, 3);
                if (_participantSlot == slot) {
                    OnUseBooster(BoosterType.Key);
                }
            }
        }

        public void PlayKillEffectOnOther(int slot) {
            _levelView.PlayKillEffectOnOther(slot);
        }

        #region Gui callback

        private void OnBombButtonClicked() {
            SendSpawnBomb(_participantSlot);
        }

        private void SendSpawnBomb(int slot) {
            UniTask.Void(async () => {
                if (_levelView.CheckSpawnPvpBomb(slot)) {
                    if (_syncHeroes == null) {
                        return;
                    }
                    await _syncHeroes[slot].PlantBomb();
                    GuiPvp.SetEnableInputPlantBomb(true);
                }
            });
        }

        private void OnSpawnBomb(int slot) {
            var hero = _levelView.GetPvpHeroBySlot(slot);
            hero.Bombable.Spawn();
        }

        private void OnBoosterButtonClicked(BoosterType type) {
            if (_syncHeroes == null) {
                return;
            }
            if (!_syncHeroes[_participantSlot].IsAlive) {
                OnFailedToUseBooster();
                return;
            }
            switch (type) {
                case BoosterType.Key:
                    SetImprisoned(_participantSlot, false);
                    break;
                case BoosterType.Shield:
                    SetShielded(_participantSlot, true, HeroEffectReason.UseBooster);
                    break;
            }
        }

        public void RequestQuitPvp() {
            _onRequestQuitPvp?.Invoke();
        }

        #endregion

        private void GoToPvpMenu() { }

        private void OnConnectionLost(string reason, int code) {
            _guiPvp.ShowErrorAndKick(canvasDialog, reason);
        }

        private void OnKick() {
            ServiceLocator.Instance.Resolve<IServerManager>().Disconnect();
            var reason = "The account is having a data conflict with the Battle mode server";
            _guiPvp.ShowErrorAndKick(canvasDialog, reason);
        }

        private void OnUseBooster(BoosterType type) {
            var isInfinityMode = _boosterStatus.GetQuantity(type) >= 99;
            if (isInfinityMode) {
                _boosterStatus.UpdatedQuantity(type, 100);
            }
            _guiPvp.UpdateButtonBoosterUsed(type, _boosterStatus, () => _levelView.PlayerIsInJail(_participantSlot));
        }

        private void OnPlayerInJail(int slot) {
            if (slot == _participantSlot) {
                _guiPvp.UpdateButtonInJail(_boosterStatus);
            }
        }

        private void OnPlayerEndInJail(int slot) {
            if (slot == _participantSlot) {
                _guiPvp.UpdateButtonEndInJail(_boosterStatus);
            }
        }

        private void OnUpdateItem(int slot, ItemType item, int value) {
            if (slot == _participantSlot) {
                _guiPvp.UpdateItemUi(item, value);
            }
        }

        private void OnFailedToUseBooster() {
            _guiPvp.UpdateButtonBoosterFailToUse();
        }

        private void HideAllDialog() {
            _guiPvp.HideAllDialog(canvasDialog);
        }

        private void ShowDialogWin(int slot, IPvpResultInfo info) {
            GuiPvp.ShowDialogWin(canvasDialog, info, slot, "", false, OnClaim, false);
        }

        private void ShowDialogLose(int slot, LevelResult result, IPvpResultInfo info) {
            if (result == LevelResult.Lose) {
                _guiPvp.ShowDialogPvpDefeat(canvasDialog, result, info, slot, "", false, OnClaim, false);
                return;
            }
            _guiPvp.ShowDialogPvpDrawOrLose(canvasDialog, result, info, slot, "", false, OnClaim, false);
        }

        private static LevelResult ParseLevelResult(IPvpResultInfo info, int slot) {
            // FIXME: draw?
            return info.IsDraw || info.WinningTeam != info.Info[slot].TeamId ? LevelResult.Lose : LevelResult.Win;
        }

        private void OnClaim() {
            GoToPvpMenu();
        }

        public void OnUpdateHealthUi(int slot, int value) {
            _guiPvp.UpdateHealthUi(slot, value);
            if (_participantSlot != slot) {
                _levelView.UpdateHealthBar(slot, value);
            }
            _levelView.SetImmortal(slot);
        }

        public Vector3 GetTilePosition(int tileX, int tileY) {
            var pos = _levelView.GetTilePosition(tileX, tileY);
            return pos;
        }

        public void SetOnRequestQuitPvp(Action onRequestQuitPvp) {
            _onRequestQuitPvp = onRequestQuitPvp;
        }
    }
}