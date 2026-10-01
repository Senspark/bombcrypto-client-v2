using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics;
using App;
using Com.LuisPedroFonseca.ProCamera2D;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Senspark;
using Animation;
using Engine.Components;
using Engine.Entities;
using Engine.Manager;
using Game.ConnectControl;
using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;
using Game.Treasure;
using JetBrains.Annotations;
using Scenes.FarmingScene.Scripts;
using Scenes.MainMenuScene.Scripts;
using Services;
using Services.Rewards;

using Share.Scripts.Dialog;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using RewardType = Constant.RewardType;

namespace Game.UI {
    public class LevelScene : MonoBehaviour, ITreasurePlaybackHost {
        public bool AutoSimulation = true;
        public static LevelScene Instance { get; private set; }
        public static bool IsFirstLoad { get; set; }
        public static bool IsLoadMapDone { get; set; }

        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        public Transform parent;
        
        [SerializeField]
        public Transform effectCanvas;

        [SerializeField]
        private InGameRewardToken rewardTokenPrefab;

        [SerializeField]
        public Transform chestIcon;

        [SerializeField]
        public Transform starCoreIcon;

        [SerializeField]
        private Transform luckyWheelIcon;

        [SerializeField]
        private ItemGettingAnim chestAnim;

        [SerializeField]
        private TreasureModeRes resources;

        [SerializeField]
        private RectTransform bottomPanner;

        [SerializeField]
        private ProCamera2DPanAndZoom panAndZoom;

        [SerializeField] [CanBeNull]
        private GameObject dialogBackground;

        [SerializeField]
        private GameObject[] walletDisplays;

        [SerializeField]
        private EventTrigger[] buttonEvents;

        public Canvas DialogCanvas => canvasDialog;
        public PauseProperty PauseStatus { get; } = new();
        public CameraProperty CameraStatus { get; } = new();
        public GameModeType Mode { get; private set; }
        public TrialState IsTrial { get; private set; }

        private IServerManager _serverManager;
        private ObserverHandle _handle;
        private LevelView _levelView;
        private IBHeroManager _playerStore;
        private IAnalytics _analytics;
        private IStorageManager _storeManager;
        private ITHModeV2Manager _thModeV2Manager;

        private ISoundManager _soundManager;

        private IChestRewardManager _chestRewardManager;
        private ILaunchPadManager _launchPadManager;
        private IPveModeManager _pveModeManager;
        private IBHeroManager _playerStoreManager;
        private IPveHeroStateManager _pveHeroStateManager;
        private IUserAccountManager _userAccountManager;

        private readonly Queue<IPveHeroDangerous> _heroChangeStateQueue = new();
        private Camera _camera;
        private bool _showBanners;
        private Action<LevelScene> _onLoaded;
        private WaitingUiManager _waiting;
        private bool _waitingAddHeroInMap = true;
        private UniTaskCompletionSource _userInitTcs;
        private TreasurePlayback _treasurePlayback;
        private Dictionary<int, Vector2Int> _spawnTiles = new();
        private int _mapTileset;
        private TreasureSnapshot _loadedSnapshot;
        private bool _reloading;
        // Pause state the server last acknowledged; null until START_TREASURE_MODE answered.
        private bool? _serverPaused;
        private bool _syncingPause;
        // False while the level loads: the server keeps heroes on their spawn tiles until the scene can show them.
        private bool _levelReady;

        private bool WantServerPaused => PauseStatus.IsPausing || !_levelReady;

        public IEntityManager EntityManager => _levelView ? _levelView.EntityManager : null;
        
        #region UNITY EVENTS

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IBHeroManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _launchPadManager = ServiceLocator.Instance.Resolve<ILaunchPadManager>();
            _pveModeManager = ServiceLocator.Instance.Resolve<IPveModeManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IBHeroManager>();
            _pveHeroStateManager = ServiceLocator.Instance.Resolve<IPveHeroStateManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _thModeV2Manager = ServiceLocator.Instance.Resolve<IServerManager>().ThModeV2Manager;

            // Subscribed before START_TREASURE_MODE is sent, so no event is missed while the scene loads.
            _treasurePlayback = new TreasurePlayback(this, StartTreasureModeOnce);
            PauseStatus.OnChanged += paused => {
                _treasurePlayback.SetPaused(paused);
                SyncServerPause();
            };

            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnHeroChangeState = OnHeroStateChanged,
                OnTreasureEvents = _treasurePlayback.OnEvents,
                OnServerStateChanged = OnServerStateChanged,
                OnActiveHero = OnActiveHero,
                OnRemoveHeroes = OnRemoveHeroes
            });

            if (walletDisplays is { Length: 2 }) {
                var normal = !ScreenUtils.IsIPadScreen();
                walletDisplays[0].SetActive(normal);
                walletDisplays[1].SetActive(!normal);
            }

            _camera = Camera.main;
            Physics2D.simulationMode = SimulationMode2D.Script;
            Physics2D.gravity = Vector2.zero;
            Instance = this;
            
            PauseStatus.SetCamera(panAndZoom);
            CameraStatus.SetCamera(panAndZoom);
            EnableDialogBackground(false);

            _soundManager.StopImmediateMusic();
            _soundManager.PlayMusic(Audio.TreasureMusic);
            
            EventManager.Add(LoginEvent.UserInitialized, OnUserInitialized);
        }

        private void OnUserInitialized() {
            _userInitTcs?.TrySetResult();
        }

        private LevelView CreateLevelView() {
            var tileIndex = _playerStoreManager.TileSet;
            //Network airdrop luôn là TreasureHuntV2
            if (AppConfig.IsWebAirdrop()) {
                Mode = GameModeType.TreasureHuntV2;
            }
            return _pveModeManager.CreateLevelView(Mode, tileIndex, transform);
        }

        private async void Start() {
            Mode = GameModeType.TreasureHuntV2;
            _analytics.TrackScene(SceneType.VisitTreasureHunt);

            // Server-driven: the start response carries the map + where every hero stands now.
            TreasureSnapshot snapshot;
            try {
                snapshot = await StartTreasureMode();
            } catch (Exception e) {
                DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, e);
                return;
            }
            if (!this) {
                return;
            }
            _spawnTiles = new Dictionary<int, Vector2Int>();
            foreach (var hero in snapshot.Heroes) {
                _spawnTiles[hero.Id] = hero.Cell;
            }

            _levelView = CreateLevelView();
            _levelView.SpawnTileHint = GetSpawnTileHint;
            await LoadLevel();
            _mapTileset = snapshot.Map.Tileset;
            _loadedSnapshot = snapshot;
            _waitingAddHeroInMap = false;

            IsTrial = snapshot.IsTrial;
            OnDangerousHero(snapshot);
            _levelReady = true;
            _treasurePlayback.ApplySnapshot(snapshot);
            SyncServerPause();
            _spawnTiles.Clear();
            _onLoaded?.Invoke(this);

            Time.timeScale = 1;
            ShowBanners();
            IsLoadMapDone = true;
            //Đợi th scene load xong và unload scene connect rồi mới show offline reward để tránh bị dính lại scene cũ
            ShowOfflineReward();
        }

        private async void ShowOfflineReward() {
            if (AppConfig.IsTon() && !IsFirstLoad) {
                await UniTask.DelayFrame(1);
                IsFirstLoad = true;
                var reward = await _serverManager.General.GetOfflineReward();
                if (reward.amount > 0) {
                    var dialog = await DialogOfflineRewardAirdrop.Create();
                    var hour = (int)reward.offlineTime / 60;
                    dialog.Show(DialogCanvas, hour.ToString(), reward.amount.ToString("0.########"));
                }
            }
        }

        private void OnDestroy() {
            // Additive reload chạy Awake của instance mới (Instance = new) TRƯỚC OnDestroy của instance cũ.
            // Chỉ clear khi mình đúng là instance đang đăng ký, tránh clobber registration của instance mới.
            if (Instance == this) {
                Instance = null;
            }
            _treasurePlayback?.Stop();
            DOTween.KillAll(true);
            _handle.Dispose();
            EventManager.Remove(LoginEvent.UserInitialized, OnUserInitialized);

        }

        private void Update() {
            if (AutoSimulation) {
                ProcessUpdate();
            }
        }

        // MAP_SERVICE_ERROR (map-service briefly unreachable) is worth a couple of retries.
        private async Task<TreasureSnapshot> StartTreasureMode() {
            const int maxAttempts = 3;
            for (var attempt = 1;; attempt++) {
                try {
                    return await StartTreasureModeOnce();
                } catch (Exception) when (attempt < maxAttempts) {
                    await UniTask.Delay(1000 * attempt);
                }
            }
        }

        // Sends the current pause state so a resync never makes the server's heroes walk behind a paused screen.
        private async Task<TreasureSnapshot> StartTreasureModeOnce() {
            var paused = WantServerPaused;
            var snapshot = await _serverManager.Pve.StartTreasureMode(paused);
            _serverPaused = paused;
            SyncServerPause();
            return snapshot;
        }

        // PAUSE/RESUME_TREASURE_MODE, one at a time so they reach the server in order; the latest state wins.
        private async void SyncServerPause() {
            if (_syncingPause) {
                return;
            }
            _syncingPause = true;
            try {
                // A stopped scene may still pause the server (new map, reload), never resume it.
                while (this && _serverPaused != null && _serverPaused != WantServerPaused &&
                       (WantServerPaused || !_treasurePlayback.IsStopped) &&
                       _serverManager.CurrentState == ServerConnectionState.LoggedIn) {
                    var paused = WantServerPaused;
                    await _serverManager.Pve.PauseTreasureMode(paused);
                    _serverPaused = paused;
                }
            } catch (Exception e) {
                // A missed resume is recovered by the playback's silence resync (START carries the pause state).
                Debug.LogWarning($"[TREASURE] pause sync failed: {e.Message}");
            } finally {
                _syncingPause = false;
            }
        }

        // Reconnected: the server may have restarted and lost the game, START_TREASURE_MODE starts it again.
        public async Task ResyncTreasureMode() {
            // The server drops every request sent before USER_INITIALIZED, and never answers it.
            await _serverManager.WaitForUserInitialized();
            if (!this || !_levelReady) {
                // Still loading / reloading: that path sends its own START_TREASURE_MODE.
                return;
            }
            await RefreshHeroesFromServer();
            if (!this || !_levelReady) {
                return;
            }
            await _treasurePlayback.Resync("reconnect", true);
        }

        // A restarted server is back on its last save: take its hero stages and energy, then fix the heroes on the map.
        // Otherwise a hero it plays but the client holds asleep plants bombs and never moves.
        private async Task RefreshHeroesFromServer() {
            try {
                await _serverManager.Pve.GetActiveBomber();
                if (!this || !_levelView) {
                    return;
                }
                var ids = new List<HeroId>();
                foreach (var data in _playerStore.GetInMapPlayerData()) {
                    ids.Add(data.heroId);
                }
                foreach (var player in _levelView.EntityManager.PlayerManager.Players) {
                    if (player && !ids.Contains(player.HeroId)) {
                        ids.Add(player.HeroId);
                    }
                }
                _waitingAddHeroInMap = true;
                try {
                    await _levelView.AddNewPlayersOrRefresh(ids.ToArray());
                } finally {
                    _waitingAddHeroInMap = false;
                }
            } catch (Exception e) {
                Debug.LogWarning($"[TREASURE] hero refresh failed: {e.Message}");
            }
        }

        private Vector2Int? GetSpawnTileHint(HeroId heroId) {
            var tile = _treasurePlayback.GetHeroTile(heroId.Id);
            if (tile != null) {
                return tile;
            }
            return _spawnTiles.TryGetValue(heroId.Id, out var spawn) ? spawn : null;
        }

        public void ProcessUpdate() {
            if (PauseStatus.IsPausing) {
                return;
            }
            
            if(SceneLoader.IsLoading || _waitingAddHeroInMap)
                return;

            CheckAndChangeHeroState();

            var delta = Time.deltaTime;

            _pveHeroStateManager.Update(delta);

            if (_levelView) {
                _levelView.Step(delta);
                // After the physics step, so the tweened hero positions are what gets rendered.
                _treasurePlayback.Update(delta);
            }
        }

        public void EnableDialogBackground(bool state) {
            if (dialogBackground != null) {
                dialogBackground.SetActive(state);
            }
        }

        #endregion

        #region PUBLIC METHODS

        public static async UniTask LoadScene(GameModeType mode, bool showBanners = true) {
            var sceneName = AppConfig.IsAirDrop() ? "TreasureModeScene" : "FarmingScene";
            await ServiceLocator.Instance.Resolve<IFinanceUserLoader>().LoadAsync();
            ServiceLocator.Instance.Resolve<IBHeroManager>().LoadMap(mode);
            await SceneLoader.LoadSceneAsync(sceneName);
        }

        private static async UniTask ReloadScene(GameModeType mode, bool showBanners = true,
            Action<LevelScene> onLoaded = null) {
            await ServiceLocator.Instance.Resolve<IFinanceUserLoader>().LoadAsync();
            ServiceLocator.Instance.Resolve<IBHeroManager>().LoadMap(mode);
            LevelScene levelScene;

            ServiceLocator.Instance.Resolve<ISoundManager>().StopImmediateMusic();
            await SceneLoader.ReloadSceneAsync(() => {
                var levelScene1 = FindObjectOfType<LevelScene>();
                levelScene1.Mode = GameModeType.TreasureHuntV2;
                levelScene1._showBanners = showBanners;
                levelScene1._onLoaded = onLoaded;
            });
        }

        public static async UniTask OpenTreasureHuntWithoutLoad() {
            var sceneName = (AppConfig.IsWebGL() && !AppConfig.IsWebAirdrop()) ? "FarmingScene" : "TreasureModeScene";
            await SceneLoader.LoadSceneAsync(sceneName, g => {
                var levelScene = FindObjectOfType<LevelScene>();
                levelScene.Mode = GameModeType.TreasureHuntV2;
                levelScene._showBanners = true;
                levelScene._onLoaded = null;
            });
        }

        public void EarnReward(ITokenReward reward, int quantity, Vector2 startPosition, Vector2 endPosition,
            System.Action callbackComplete) {
            _soundManager.PlaySound(Audio.CollectBCoin);
            for (var i = 0; i < quantity; i++) {
                var rewardObject = GetRewardObject(reward);

                var x = Random.Range(30f, 60f) * (Random.Range(0, 2) * 2 - 1);
                var y = 0;
                var jumpPower = Random.Range(5f, 20f);
                var numJump = Random.Range(1, 4);
                var jumpDest = startPosition + new Vector2(x, y);

                rewardObject.transform.position = startPosition;
                var move = rewardObject.transform.DOMove(endPosition, 1.0f);
                var jump = rewardObject.transform.DOJump(jumpDest, jumpPower, numJump, 2.0f);
                jump
                    .Append(move)
                    .OnComplete(() => { Destroy(rewardObject); })
                    .SetUpdate(true);
            }
            DOTween.Sequence().SetDelay(3).OnComplete(() => callbackComplete?.Invoke());
        }

        public void EarnReward(BlockRewardType reward, int quantity, Vector2 startPosition, Vector2 endPosition,
            System.Action callbackComplete) {
            _soundManager.PlaySound(Audio.CollectBCoin);
            Sprite icon;
            try {
                icon = resources.GetSpriteByRewardType(reward);
            } catch (KeyNotFoundException e) {
                // Nằm trong gameplay loop — để exception thoát ra sẽ bỏ qua callbackComplete và kẹt chuỗi animation.
                Debug.LogError(e.Message);
                callbackComplete?.Invoke();
                return;
            }
            for (var i = 0; i < quantity; i++) {
                var rewardObject = Instantiate(rewardTokenPrefab, parent);
                rewardObject.Init(icon);

                var x = Random.Range(30f, 60f) * (Random.Range(0, 2) * 2 - 1);
                var y = 0;
                var jumpPower = Random.Range(5f, 20f);
                var numJump = Random.Range(1, 4);
                var jumpDest = startPosition + new Vector2(x, y);

                rewardObject.transform.position = startPosition;
                var move = rewardObject.transform.DOMove(endPosition, 1.0f);
                var jump = rewardObject.transform.DOJump(jumpDest, jumpPower, numJump, 2.0f);
                jump
                    .Append(move)
                    .OnComplete(() => { Destroy(rewardObject.gameObject); })
                    .SetUpdate(true);
            }
            DOTween.Sequence().SetDelay(3).OnComplete(() => callbackComplete?.Invoke());
        }

        public async void AddNewPlayersOrRefresh(HeroId[] newIds) {
            _waitingAddHeroInMap = true;
            await _levelView.AddNewPlayersOrRefresh(newIds);
            _waitingAddHeroInMap = false;
        }

        public void RemoveHeroesFromMap(HeroId[] heroIds) {
            if (!_levelView) {
                return;
            }
            _levelView.EntityManager.PlayerManager.RemoveHeroes(heroIds);
        }

        #endregion

        #region BUTTONS EVENTS

        public void OnBackButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            if (RuntimeConfig.Landing == LandingMode.Treasure) {
                PauseStatus.SetValue(this, true);
                DialogConfirm.Create().ContinueWith(confirm => {
                    confirm.SetInfo(
                        "Do you want to play Adventure/PvP Mode?",
                        "Yes",
                        "No",
                        () => SwitchToAdventure(),
                        () => PauseStatus.SetValue(this, false));
                    confirm.Show(DialogCanvas);
                });
                return;
            }
            BackToMainMenu();
        }

        private void SwitchToAdventure() {
            GameModeSwitcher.Switch(LandingMode.Adventure, DialogCanvas);
            PauseStatus.SetValue(this, false);
        }

        private void BackToMainMenu() {
            PauseStatus.SetValue(this, true);
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            _levelView.SaveMap();
            _treasurePlayback.Stop();
            UniTask.Void(async () => {
                try {
                    await _serverManager.Pve.StopTreasureMode();
                    _soundManager.StopImmediateMusic();
                    const string sceneName = "MainMenuScene";
                    await SceneLoader.LoadSceneAsync(sceneName);
                } catch (Exception e) {
                    Debug.Log(e.Message);
                    // ignore
                } finally {
                    waiting.End();
                }
            });
        }

        public void OnSettingClicked() {
            PauseStatus.SetValue(this, true);
            _soundManager.PlaySound(Audio.Tap);
            DialogSetting.Create().ContinueWith((dialog) => {
                dialog.OnDidHide(() => PauseStatus.SetValue(this, false));
                dialog.Show(canvasDialog);
            });
        }

        public void ResetButtonEvents() {
            foreach (var item in buttonEvents) {
                item.OnPointerEnter(null);
                item.OnPointerExit(null);
            }
        }

        #endregion

        #region SERVER

        private void OnServerStateChanged(ServerConnectionState state) {
            if (state == ServerConnectionState.LostConnection) {
                PauseStatus.SetValue(this, true);
                _userInitTcs = new UniTaskCompletionSource();
                _waiting?.End();
            } else if (state == ServerConnectionState.LoggedIn) {
                UniTask.Void(async () => {
                    _waiting = new WaitingUiManager(canvasDialog);
                    _waiting.ChangeText("Reloading Data");
                    _waiting.Begin();

                    await UniTask.Delay(500);

                    var timeOuted = false;

                    void Disconnect() {
                        _serverManager.Disconnect();
                    }
                    async Task TimeOut() {
                        await WebGLTaskDelay.Instance.Delay(60 * 1000);
                        timeOuted = true;
                    }
                    async Task Job() {
                        if(_userInitTcs != null) {
                            await _userInitTcs.Task;
                        }
                        _userInitTcs = null;
                         await _serverManager.UserSolanaManager.SyncHouseSol();
                 
                        if (timeOuted) {
                            return;
                        }
           
                        await _serverManager.UserSolanaManager.GetActiveBomberSol();
         
                        if (timeOuted) {
                            return;
                        }
                        // The reloaded scene calls START_TREASURE_MODE, which returns the map.
                        await ReLoadLevelScene();
                    }
                    try {
                        await Task.WhenAny(TimeOut(), Job());
                        if (timeOuted) {
                            Disconnect();
                        }
                    } catch (Exception e) {
                        Disconnect();
                    } finally {
                        if (_waiting != null) {
                            _waiting.End();
                            _waiting = null;
                            PauseStatus.SetValue(this, false);
                        }
                    }
                });
            }
        }

        #region TREASURE PLAYBACK

        // An EXPLODE event reached its turn in the playback: blocks, rewards, energy, dangerous.
        public void ApplyExplode(ITreasureExplode data, HeroId heroId, bool showEffects, bool markBreak) {
            if (!_levelView) {
                return;
            }
            CheckTrialEnd(data);
            var player = _levelView.EntityManager.PlayerManager.GetPlayerById(heroId);

            // No energy = the hero is no longer credited: the map still changes, nothing else does.
            if (data.HasEnergy) {
                if (player) {
                    var damageFrom = data.Dangerous.DangerousType == PveDangerousType.Danger
                        ? DamageFrom.Thunder
                        : DamageFrom.BombExplode;
                    player.Health.SetCurrentHealth(data.Energy, damageFrom);
                    ShowDangerousEffect(data.Dangerous);
                }
            }

            var mapManager = _levelView.EntityManager.MapManager;
            var blocks = data.DestroyedBlocks;
            for (var k = 0; k < blocks.Count; k++) {
                var block = blocks[k];
                var i = block.Coord.x;
                var j = block.Coord.y;
                if (!mapManager.TryGetBlock(i, j, out var blockObj)) {
                    continue;
                }
                blockObj.health.SetCurrentHealth(block.Hp);
                if (block.Hp > 0) {
                    continue;
                }
                if (markBreak) {
                    mapManager.MarkBreakBrick(i, j);
                }
                var isBigReward = mapManager.IsBigRewardBlock(i, j);
                var position = blockObj.transform.position;
                if (mapManager.RemoveBrick(i, j)) {
                    if (showEffects) {
                        blockObj.ShowBrickBreaking();
                    }
                    mapManager.ClearBlock(i, j);
                }
                if (block.Rewards.Count == 0) {
                    continue;
                }
                if (showEffects) {
                    GetReward(block.Rewards, heroId, position, isBigReward, data.AttendPools);
                } else {
                    foreach (var reward in block.Rewards) {
                        _chestRewardManager.AdjustChestReward(reward.Type, reward.Value);
                    }
                }
            }
        }

        public void OnHeroLeave(HeroId heroId, Player player, string reason) {
            switch (reason) {
                case "no_energy":
                    if (player) {
                        var botManager = player.GetComponent<BotManager>();
                        if (botManager && !botManager.IsSleeping) {
                            botManager.GoToSleep_SendRequest();
                        }
                    }
                    break;
                case "inactive":
                case "removed":
                    RemoveHeroesFromMap(new[] { heroId });
                    break;
                // not_working: the stage UI already moved it; limit / stopped: it just stands still.
            }
        }

        // Heroes wait on the new map's spawn tiles through the win dialog; the reloaded scene resumes them.
        public void OnNewMapReceived() {
            _levelReady = false;
            SyncServerPause();
        }

        public void OnNewMap(TreasureEvent newMap) {
            OnLevelCompleted(true);
        }

        // Resync snapshot: update the current map in place when it is the same map, else reload the scene.
        public bool TryApplySnapshotMap(TreasureSnapshot snapshot) {
            if (!_levelView || _reloading) {
                return false;
            }
            if (snapshot == _loadedSnapshot) {
                // The scene's map was built from this very snapshot.
                return true;
            }
            var mapManager = _levelView.EntityManager.MapManager;
            var target = new Dictionary<Vector2Int, IMapBlock>();
            foreach (var b in snapshot.Map.Blocks) {
                if (b.Health > 0) {
                    target[b.Position] = b;
                }
            }
            var sameMap = snapshot.Map.Tileset == _mapTileset;
            foreach (var (cell, b) in target) {
                if (!sameMap) {
                    break;
                }
                sameMap = mapManager.TryGetBlock(cell.x, cell.y, out var block) && block &&
                          block.blockType == (EntityType) (b.Type + 5);
            }
            if (!sameMap) {
                _ = ReLoadLevelScene();
                return false;
            }
            for (var i = 0; i < mapManager.Col; i++) {
                for (var j = 0; j < mapManager.Row; j++) {
                    if (!mapManager.TryGetBlock(i, j, out var block)) {
                        continue;
                    }
                    if (target.TryGetValue(new Vector2Int(i, j), out var b)) {
                        block.health.SetCurrentHealth(b.Health);
                    } else if (mapManager.RemoveBrick(i, j)) {
                        mapManager.ClearBlock(i, j);
                    }
                }
            }
            _playerStore.SetMapDetails(snapshot.Map);
            _playerStore.LoadMap(Mode);
            IsTrial = snapshot.IsTrial;
            return true;
        }

        #endregion

        private void CheckTrialEnd(IPveExplodeResponse response) {
            if (IsTrial == TrialState.TrialBegin && response.IsTrial == TrialState.TrialEnd) {
                PauseStatus.SetValue(this, true);
                var waiting = new WaitingUiManager(canvasDialog);
                waiting.Begin();
                UniTask.Void(async () => {
                    _treasurePlayback.Stop();
                    await _serverManager.Pve.StopTreasureMode();
                    await _serverManager.General.SyncHero(false);
                    waiting.End();
                });
            }
        }

        private void OnDangerousHero(IStartPveResponse result) {
            foreach (var d in result.DangerousData) {
                if (d.DangerousType == PveDangerousType.NoDanger) {
                    continue;
                }
                var player = _levelView.EntityManager.PlayerManager.GetPlayerById(d.HeroId);
                var playerData = _playerStore.GetPlayerDataFromId(d.HeroId);
                if (player) {
                    var (hp, damageFrom) = d.DangerousType == PveDangerousType.Danger
                        ? (0, DamageFrom.Thunder)
                        : (playerData.hp, DamageFrom.BombExplode);
                    player.Health.SetCurrentHealth(hp, damageFrom);
                }
                ShowDangerousEffect(d);
            }
        }

        private void ShowDangerousEffect(IPveHeroDangerous data) {
            var t = data.DangerousType;
            if (t == PveDangerousType.NoDanger) {
                return;
            }
            if (!AppConfig.IsWebGL() || AppConfig.IsWebAirdrop())
                return;

            _levelView.ShowThunder(data.HeroId);
        }

        private void OnHeroStateChanged(IPveHeroDangerous data) {
            // Full-load sprite hero NGAY khi nhận tín hiệu enable (state-change), TRƯỚC khi tới lượt spawn ở
            // CheckAndChangeHeroState → tới lúc spawn cache đã ấm hẳn → spawn instant, không khựng gameplay.
            PrewarmHeroSprite(data.HeroId);
            _heroChangeStateQueue.Enqueue(data);
        }

        private async void PrewarmHeroSprite(HeroId heroId) {
            var pd = _playerStore.GetPlayerDataFromId(heroId);
            if (pd == null || !HeroSpriteCatalog.Has(pd.playerType)) {
                return;
            }
            var loader = ServiceLocator.Instance.Resolve<IHeroSpriteLoader>();
            await loader.Preload(pd.playerType, pd.playercolor, (HeroRarity)pd.rare);
        }

        private async UniTask CheckAndChangeHeroState() {
            if (_heroChangeStateQueue.Count > 0) {
                var data = _heroChangeStateQueue.Dequeue();
                ShowDangerousEffect(data);
                _waitingAddHeroInMap = true;
                await _levelView.AddNewPlayersOrRefresh(data);
                _waitingAddHeroInMap = false;
            }
        }

        #endregion

        #region PRIVATE METHODS

        private async UniTask LoadLevel() {
            var sceneCallback = new SceneCallback { OnLevelCompleted = OnLevelCompleted, };
            await _levelView.Initialize(Mode, sceneCallback, bottomPanner, panAndZoom);
        }

        private async void OnLevelCompleted(bool win) {
            _analytics.TreasureHunt_TrackCompleteMap();
            var dialog = await DialogWin.Create();
            dialog.OnDidHide(OnNewMap);
            dialog.Show(canvasDialog);
        }

        private void OnActiveHero(IPveHeroDangerous data, HeroId heroId, bool isActive) {
            if (Mode != GameModeType.TreasureHuntV2 || !_levelView)
                return;
            var playerManager = _levelView.EntityManager.PlayerManager;
            playerManager.AddPendingActiveHeroes(heroId, isActive);
            OnHeroStateChanged(data);
        }

        private void OnRemoveHeroes(HeroId[] heroIds) {
            if (Mode != GameModeType.TreasureHuntV2 || !_levelView)
                return;
            var playerManager = _levelView.EntityManager.PlayerManager;
            playerManager.RemoveHeroes(heroIds);
        }

        // The server already switched maps; the reloaded scene gets it from START_TREASURE_MODE.
        private void OnNewMap() {
            _ = ReLoadLevelScene();
        }

        private void GetReward(List<ITokenReward> rewards, HeroId id, Vector3 position, bool isBigReward,
            List<RewardType> attendPoolsThv2) {
            if (rewards == null) {
                return;
            }

            if (_camera == null)
                _camera = Camera.main;
            var from = _camera.WorldToScreenPoint(position);
            var emitActions = new List<Action>();

            foreach (var reward in rewards) {
                var to = chestIcon.position;
                if (AppConfig.IsTon() && reward.Type.Type == BlockRewardType.BLCoin) {
                    to = starCoreIcon.position;
                }
                var value = isBigReward ? Random.Range(4, 8) : 1;
                emitActions.Add(() => EarnReward(reward, value, from, to, null));

                _chestRewardManager.AdjustChestReward(reward.Type, reward.Value);

                if (!AppConfig.IsWebGL() && !AppConfig.IsMobile())
                    continue;

                var heroData = _playerStoreManager.GetPlayerDataFromId(id);
                if (heroData == null) {
                    continue;
                }
                foreach (var pool in attendPoolsThv2) {
                    var heroRarity = heroData.rare;
                    var poolTo = _thModeV2Manager.GetPositionPool(heroRarity).position;
                    if (pool == RewardType.Senspark) {
                        emitActions.Add(() => EarnReward(BlockRewardType.SenTicket, 1, from, poolTo, null));
                    } else {
                        emitActions.Add(() => EarnReward(BlockRewardType.BcoinTicket, 1, from, poolTo, null));
                    }
                }
            }
            
            // Random thứ tự hiển thị cho đẹp mắt
            for (var i = emitActions.Count - 1; i > 0; i--) {
                var j = Random.Range(0, i + 1);
                (emitActions[i], emitActions[j]) = (emitActions[j], emitActions[i]);
            }
            foreach (var action in emitActions) {
                action();
            }
        }

        private async Task ReLoadLevelScene() {
            if (_reloading) {
                return;
            }
            _reloading = true;
            try {
                _treasurePlayback.Stop();
                PauseStatus.SetValue(this, true);
                var waiting = await DialogWaiting.Create();
                waiting.Show(canvasDialog);
                await ReloadScene(Mode, _showBanners, level => { waiting.HideImmediately(); });
            } catch (Exception e) {
                if (!_storeManager.EnableAutoMine) {
                    DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, e);
                }
            }
        }

        private GameObject GetRewardObject(ITokenReward reward) {
            var network = _userAccountManager.GetRememberedAccount().network;
            var obj = Instantiate(rewardTokenPrefab, parent);
            var data = _launchPadManager.GetData(reward.Type, DataType.TR) ??
                       _launchPadManager.GetData(reward.Type, RewardUtils.ConvertNetworkToDatatype(network));
            obj.Init(data);
            return obj.gameObject;
        }

        private async void ShowBanners() {
            if (!_showBanners) {
                return;
            }
            if (DialogAutoMine.CanShow()) {
                var dialog = await DialogAutoMine.Create();
                dialog.Show(canvasDialog);
            }
        }

        #endregion
    }

    public class CameraProperty {
        private ProCamera2DPanAndZoom _panAndZoom;

        public void SetCamera(ProCamera2DPanAndZoom panAndZoom) {
            _panAndZoom = panAndZoom;
        }

        public void SetAllowPan(bool value) {
            if (_panAndZoom) {
                _panAndZoom.AllowPan = value;
            }
        }
    }

    public class PauseProperty {
        // Mục tiêu là object nào set Pause = true thì object đó phải có trách nhiệm set Pause = false
        public bool IsPausing { get; private set; } = false;
        public event Action<bool> OnChanged;
        private object _latestRequester;
        private ProCamera2DPanAndZoom _panAndZoom;

        public void SetCamera(ProCamera2DPanAndZoom panAndZoom) {
            _panAndZoom = panAndZoom;
        }

        public void SetValue(object requester, bool value) {
            if (_latestRequester == null) {
                if (_panAndZoom) {
                    _panAndZoom.AllowPan = !value;
                }
                // Nếu _requester == null thì cho set mới
                if (IsPausing == value) {
                    return;
                }
                if (IsPausing == false) {
                    // Bắt đầu Pause
                    _latestRequester = requester;
                    IsPausing = true;
                    OnChanged?.Invoke(true);
                } else {
                    // Kết thúc Pause
                    _latestRequester = null;
                    IsPausing = false;
                    OnChanged?.Invoke(false);
                }
            } else if (_latestRequester == requester) {
                if (_panAndZoom) {
                    _panAndZoom.AllowPan = !value;
                }
                // Nếu _requester != null thì chỉ cho set khi nào _requester == requester;
                if (IsPausing == value) {
                    return;
                }
                if (IsPausing != true) {
                    return;
                }
                // Kết thúc Pause
                _latestRequester = null;
                IsPausing = false;
                OnChanged?.Invoke(false);
            }
        }
    }
}