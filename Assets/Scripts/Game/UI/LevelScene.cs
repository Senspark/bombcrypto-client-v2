using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics;
using App;
using Com.LuisPedroFonseca.ProCamera2D;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Senspark;
using Engine.Components;
using Engine.Manager;
using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.Manager;
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
    public class LevelScene : MonoBehaviour {
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
        private BLGachaRes resources;

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
        private IPlayerStorageManager _playerStore;
        private IAnalytics _analytics;
        private IStorageManager _storeManager;
        private ITHModeV2Manager _thModeV2Manager;

        private ISoundManager _soundManager;

        private IChestRewardManager _chestRewardManager;
        private ILaunchPadManager _launchPadManager;
        private IPveModeManager _pveModeManager;
        private IPlayerStorageManager _playerStoreManager;
        private IPveHeroStateManager _pveHeroStateManager;
        private IUserAccountManager _userAccountManager;

        private readonly Queue<IPveHeroDangerous> _heroChangeStateQueue = new();
        private Camera _camera;
        private bool _showBanners;
        private Action<LevelScene> _onLoaded;
        private WaitingUiManager _waiting;
        private bool _waitingAddHeroInMap = true;
        private UniTaskCompletionSource _userInitTcs;
        
        #region UNITY EVENTS

        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _playerStore = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _launchPadManager = ServiceLocator.Instance.Resolve<ILaunchPadManager>();
            _pveModeManager = ServiceLocator.Instance.Resolve<IPveModeManager>();
            _playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();
            _pveHeroStateManager = ServiceLocator.Instance.Resolve<IPveHeroStateManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _thModeV2Manager = ServiceLocator.Instance.Resolve<IServerManager>().ThModeV2Manager;

            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnHeroChangeState = OnHeroStateChanged,
                OnPveExploded = OnPveExploded,
                OnServerStateChanged = OnServerStateChanged,
                OnNewMapResponse = OnNewMapResponse,
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
            _levelView = CreateLevelView();
            _analytics.TrackScene(SceneType.VisitTreasureHunt);
            await LoadLevel();
            _waitingAddHeroInMap = false;

            PauseStatus.SetValue(this, true);
            //var waiting = await DialogWaiting.Create();
            //waiting.Show(DialogCanvas);
            try {
                var result = await StartPve();
                if (result) {
                    PauseStatus.SetValue(this, false);
                    _onLoaded?.Invoke(this);
                    // waiting.HideImmediately();

                }
            } catch (Exception e) {
                //waiting.HideImmediately();
                DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, e.Message);
            }

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
            Instance = null;
            DOTween.KillAll(true);
            _handle.Dispose();
            EventManager.Remove(LoginEvent.UserInitialized, OnUserInitialized);

        }

        private void Update() {
            if (AutoSimulation) {
                ProcessUpdate();
            }
        }

        public async Task<bool> StartPve() {
            IStartPveResponse response;
            try {
                if (AppConfig.IsSolana()) {
                    response = await _serverManager.UserSolanaManager.StartPvESol(Mode);
                } else {
                    response = await _serverManager.Pve.StartPvE(Mode);
                }
                IsTrial = response.IsTrial;
                OnDangerousHero(response);
                return true;
            } catch (Exception) {
                return false;
            }
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
            ServiceLocator.Instance.Resolve<IPlayerStorageManager>().LoadMap(mode);
            await SceneLoader.LoadSceneAsync(sceneName);
        }

        private static async UniTask ReloadScene(GameModeType mode, bool showBanners = true,
            Action<LevelScene> onLoaded = null) {
            await ServiceLocator.Instance.Resolve<IFinanceUserLoader>().LoadAsync();
            ServiceLocator.Instance.Resolve<IPlayerStorageManager>().LoadMap(mode);
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

        public async void EarnReward(BlockRewardType reward, int quantity, Vector2 startPosition, Vector2 endPosition,
            System.Action callbackComplete) {
            _soundManager.PlaySound(Audio.CollectBCoin);
            for (var i = 0; i < quantity; i++) {
                var rewardObject = Instantiate(rewardTokenPrefab, parent);
                var icon = await resources.GetSpriteByRewardType(reward);
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

        #endregion

        #region BUTTONS EVENTS

        public void OnBackButtonClicked() {
            _soundManager.PlaySound(Audio.Tap);
            PauseStatus.SetValue(this, true);
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            _levelView.SaveMap();
            UniTask.Void(async () => {
                try {
                    await _serverManager.Pve.StopPvE();
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
                        //DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, "Failed to reconnect");
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
                        if (AppConfig.IsSolana()) {
                            await _serverManager.UserSolanaManager.GetMapDetailsSol();
                        } else {
                            await _serverManager.Pve.GetMapDetails();
                        }
                        if (timeOuted) {
                            return;
                        }
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

        private void OnPveExploded(IPveExplodeResponse data) {
            CheckTrialEnd(data);
            //update enegy
            var player = _levelView.EntityManager.PlayerManager.GetPlayerById(data.HeroId);

            if (player) {
                var damageFrom = data.Dangerous.DangerousType == PveDangerousType.Danger
                    ? DamageFrom.Thunder
                    : DamageFrom.BombExplode;
                player.Health.SetCurrentHealth(data.Energy, damageFrom);
            }

            // Dangerous Pve V2 Amazon Only
            ShowDangerousEffect(data.Dangerous);

            //update blocks
            var blocks = data.DestroyedBlocks;
            for (var k = 0; k < blocks.Count; k++) {
                var block = blocks[k];

                var i = block.Coord.x;
                var j = block.Coord.y;

                if (_levelView.EntityManager.MapManager.TryGetBlock(i, j, out var blockObj)) {
                    blockObj.health.SetCurrentHealth(block.Hp);

                    if (block.Hp <= 0) {
                        if (_levelView.EntityManager.MapManager.RemoveBrick(i, j)) {
                            blockObj.ShowBrickBreaking();
                            _levelView.EntityManager.MapManager.ClearBlock(i, j);
                        }

                        if (block.Rewards.Count > 0) {
                            var isBigReward =
                                _levelView.EntityManager.MapManager.IsBigRewardBlock(i, j);
                            GetReward(block.Rewards, data.HeroId, blockObj.transform.position, isBigReward,
                                data.AttendPools);
                        }
                    }
                }
            }
        }

        private void CheckTrialEnd(IPveExplodeResponse response) {
            if (IsTrial == TrialState.TrialBegin && response.IsTrial == TrialState.TrialEnd) {
                PauseStatus.SetValue(this, true);
                var waiting = new WaitingUiManager(canvasDialog);
                waiting.Begin();
                UniTask.Void(async () => {
                    await _serverManager.Pve.StopPvE();
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
            _heroChangeStateQueue.Enqueue(data);
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

        private void OnNewMapResponse(bool result) {
            if (Mode != GameModeType.TreasureHuntV2)
                return;
            if (result) {
                OnLevelCompleted(true);
                return;
            }
            if (!_storeManager.EnableAutoMine) {
                DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, "Fail to load new map");
            }
        }

        private void OnActiveHero(IPveHeroDangerous data, HeroId heroId, bool isActive) {
            if (Mode != GameModeType.TreasureHuntV2)
                return;
            var playerManager = _levelView.EntityManager.PlayerManager;
            playerManager.AddPendingActiveHeroes(heroId, isActive);
            OnHeroStateChanged(data);
        }

        private void OnRemoveHeroes(HeroId[] heroIds) {
            if (Mode != GameModeType.TreasureHuntV2)
                return;
            var playerManager = _levelView.EntityManager.PlayerManager;
            playerManager.RemoveHeroes(heroIds);
        }

        private void OnNewMap() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    if (AppConfig.IsSolana()) {
                        await _serverManager.UserSolanaManager.GetMapDetailsSol();
                    } else {
                        await _serverManager.Pve.GetMapDetails();
                    }
                    await ReLoadLevelScene();
                } catch (Exception e) {
                    if (!_storeManager.EnableAutoMine) {
                        DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, e.Message);
                    }
                } finally {
                    waiting.End();
                }
            });
        }

        private void GetReward(List<ITokenReward> rewards, HeroId id, Vector3 position, bool isBigReward,
            List<RewardType> attendPoolsThv2) {
            if (rewards == null) {
                return;
            }

            foreach (var reward in rewards) {
                // reward th v1
                if (_camera == null)
                    _camera = Camera.main;
                var from = _camera.WorldToScreenPoint(position);
                var to = chestIcon.position;
                if (AppConfig.IsTon() && reward.Type.Type == BlockRewardType.BLCoin) { // Star Coin 
                    to = starCoreIcon.position;
                }
                var value = isBigReward ? Random.Range(4, 8) : 1;
                EarnReward(reward, value, from, to, () => {
                    //chestAnim.PlayGettingAnimation();
                });
                _chestRewardManager.AdjustChestReward(reward.Type, reward.Value);

                if (!AppConfig.IsWebGL() && !AppConfig.IsMobile())
                    continue;

                // reward th v2
                foreach (var pool in attendPoolsThv2) {
                    var heroRarity = _playerStoreManager.GetPlayerDataFromId(id).rare;
                    to = _thModeV2Manager.GetPositionPool(heroRarity).position;

                    if (pool == RewardType.Senspark)
                        EarnReward(BlockRewardType.SenTicket, 1, from, to, () => {
                            //chestAnim.PlayGettingAnimation();
                        });
                    else {
                        EarnReward(BlockRewardType.BcoinTicket, 1, from, to, () => {
                            //chestAnim.PlayGettingAnimation();
                        });
                    }
                }
            }
        }

        private async Task ReLoadLevelScene() {
            try {
                PauseStatus.SetValue(this, true);
                if (AppConfig.IsSolana()) {
                    await _serverManager.UserSolanaManager.StopPvESol();
                } else {
                    await _serverManager.Pve.StopPvE();
                }
                var waiting = await DialogWaiting.Create();
                waiting.Show(canvasDialog);
                await ReloadScene(Mode, _showBanners, level => { waiting.HideImmediately(); });
            } catch (Exception e) {
                if (!_storeManager.EnableAutoMine) {
                    DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, e.Message);
                }
            }
        }

        private GameObject GetRewardObject(ITokenReward reward) {
            var network = _userAccountManager.GetRememberedAccount().network;
            var obj = Instantiate(rewardTokenPrefab, parent);
            var data = _launchPadManager.GetData(reward.Type, NetworkSymbol.TR) ??
                       _launchPadManager.GetData(reward.Type, NetworkSymbol.Convert(network));
            if (data != null) {
                obj.Init(data.icon);
            }
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
                } else {
                    // Kết thúc Pause
                    _latestRequester = null;
                    IsPausing = false;
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
            }
        }
    }
}