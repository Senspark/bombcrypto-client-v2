using System.Linq;
using System.Threading.Tasks;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using Data;
using Engine.Manager;
using Senspark;
using Services;
using Services.IapAds;
using UnityEngine;

namespace Scenes.MainMenuScene.Scripts {
    public class MainMenuScene : MonoBehaviour {
        [SerializeField]
        private GuiMainMenu gui;

        private static bool _isDataLoaded;
        private static UniTaskCompletionSource _userInitTcs;
        
        private IServerManager _serverManager;
        private ITRHeroManager _trHeroManager;
        private IServerRequester _serverRequester;
        private IPvPServerConfigManager _pvPServerConfigManager;
        private IBLTutorialManager _tutorialManager;
        private ObserverHandle _handle;

        public NewcomerGiftData[] NewcomerGift { set; private get; } = null;
        public static bool IsDataLoaded {
            set => _isDataLoaded = value;
        }
        
        private void Awake() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _trHeroManager = ServiceLocator.Instance.Resolve<ITRHeroManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _pvPServerConfigManager = ServiceLocator.Instance.Resolve<IPvPServerConfigManager>();
            _tutorialManager = ServiceLocator.Instance.Resolve<IBLTutorialManager>();
            
            _handle = new ObserverHandle();
            _handle.AddObserver(_serverManager, new ServerObserver {
                OnServerStateChanged = OnServerStateChanged
            });
            EventManager.Add(LoginEvent.UserInitialized, OnUserInitialized);
        }

        private void Start() {
            UniTask.Void(async () => {
                gui.CreateSkeletonElements();
                await Initialized();
                var isForceNewUserPlayPvp = _tutorialManager.TimePlayPvp() < GameConstant.MatchPvpRequiredForNewUser;
                if (isForceNewUserPlayPvp) {
                    gui.HideStarterPack();
                    gui.ForceNewUserPlayPvp();
                } else {
                    gui.CheckAndShowOffers();
                }
                await LoadForMain();
                _isDataLoaded = true;
            });
        }
        
        private void OnDestroy() {
            EventManager.Remove(LoginEvent.UserInitialized, OnUserInitialized);
        }
        
        private void OnServerStateChanged(ServerConnectionState state) {
            if (state == ServerConnectionState.LostConnection) {
                _userInitTcs = new UniTaskCompletionSource();
            } else if (state == ServerConnectionState.LoggedIn) {
                
            }
        }
        
        private void OnUserInitialized() {
            _userInitTcs?.TrySetResult();
        }

        private async Task Initialized() {
            await _serverManager.WaitForUserInitialized();
            // Token banner
            if (!_isDataLoaded) {
                await _serverManager.General.GetChestReward();
            }
            
            // Profile
            //DevHoang: Open for all platforms
            // if (!_isDataLoaded && AppConfig.IsBscOrPolygon()) {
            if (!_isDataLoaded) {
                var earlyConfigManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
                await _serverManager.General.GetConfig();
                await earlyConfigManager.InitializeAsync();
            }
            
            async Task TimeOut() {
                await WebGLTaskDelay.Instance.Delay(3000);
                _userInitTcs = null;
            }
            
            async Task Job() {
                await _userInitTcs.Task;
                _userInitTcs = null;
            }

            if (_userInitTcs != null) {
                await Task.WhenAny(TimeOut(), Job());
            }
            await gui.Initialized();
            gui.InitControllers(NewcomerGift);
            gui.InitHeroChoose();
            gui.InitPointDecay();

            if (!_isDataLoaded) {
                var avatarTRManager = ServiceLocator.Instance.Resolve<IAvatarTRManager>();
                await avatarTRManager.Init();
            }
            gui.TryLoadProFileCard();
            
            if (!_isDataLoaded) {
                var result = await _trHeroManager.GetHeroesAsync("HERO");
                var trHeroes = result.ToArray();
                if (trHeroes.Length <= 0) {
                    await _serverRequester.GetNewcomerGift();
                    // User mới nên chưa có hero nào, load new gift xong phải upload lại mới có hero
                    await _trHeroManager.GetHeroesAsync("HERO");
                    gui.InitHeroChoose();
                }
                await _pvPServerConfigManager.InitializeAsync();
            }
        }

        private static async UniTask LoadForMain() {
            if (_isDataLoaded) {
                return;
            }
            var launchPadManager = ServiceLocator.Instance.Resolve<ILaunchPadManager>();
            var informationManager = ServiceLocator.Instance.Resolve<IInformationManager>();
            var gameDataRemoteManager = ServiceLocator.Instance.Resolve<IGameDataRemoteManager>();
            var iapItemManager = ServiceLocator.Instance.Resolve<IIAPItemManager>();
            var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            var purchaseManager = ServiceLocator.Instance.Resolve<IUnityPurchaseManager>();
            await Task.WhenAll(
                launchPadManager.SyncRemoteData(),
                launchPadManager.SyncRemoteData(),
                informationManager.SyncRemoteData(),
                gameDataRemoteManager.SyncRemoteData(),
                productItemManager.InitializeAsync(),
                iapItemManager.SyncOfferShops(),
                purchaseManager.SyncData()
            );
        }

        public void ShowDialogPvpSchedule() {
            //TODO:
        }

        public void ShowEquip() {
            gui.OpenEquip();
        }

        public void ShowProfile() {
            gui.OpenProfile();
        }
    }
}