using Analytics;

using App;

using BLPvpMode.Data;

using Constant;

using Cysharp.Threading.Tasks;

using Game.UI;

using Reconnect;

using Senspark;

using Services;

using Share.Scripts.Utils;

namespace Scenes.MainMenuScene.Scripts.Controller {
    public class MainMenuController {
        private IServerManager _serverManager;
        private IAnalytics _analytics;
        private IUserAccountManager _userAccountManager;
        private IEarlyConfigManager _earlyConfigManager;
        private ILanguageManager _languageManager;
        private IFeatureManager _featureManager;
        private IStorageManager _storageManager;
        private ISoundManager _soundManager;
        private IReconnectStrategy _reconnectStrategy;

        private ObserverHandle _handleMain;

        public void Initialized(IReconnectStrategy reconnectStrategy,
            System.Action<int> updateLatency,
            System.Action showDialogConfirmUpgrade,
            System.Action showDialogForceUpgrade
        ) {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _earlyConfigManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();

            _handleMain = new ObserverHandle();
            _handleMain.AddObserver(_serverManager, new ServerObserver {
                    OnUpdateLatency = updateLatency, // 
                });
            
            switch ((UpdateStatus) _earlyConfigManager.UpdateStatus) {
                case UpdateStatus.RecommendUpdate:
                    showDialogConfirmUpgrade.Invoke();
                    break;
                case UpdateStatus.ForceUpdate:
                    showDialogForceUpgrade.Invoke();
                    break;
            }

            _analytics.TrackScene(SceneType.VisitHome);
            _reconnectStrategy = reconnectStrategy;
            _reconnectStrategy.Start();
        }
        
        public void OnDestroy() {
            _reconnectStrategy.Dispose();
            _handleMain.Dispose();
        }

        public string GetString(LocalizeKey key) {
            return _languageManager.GetValue(key);
        }

        public bool IsDisableFeature(FeatureId featureId) {
            return _earlyConfigManager.IsDisableFeature(FeatureId.PvE);
        }

        public bool IsEnableStoryMode() {
            return _featureManager.EnableStoryMode;
        }

        public bool IsEnableTreasureHunt() {
            return _featureManager.EnableTreasureHunt;
        }

        public bool IsEnableBHero() {
            return _featureManager.EnableBHero;
        }

        public void CheckRedDot(BLDailyRewardButton dailyGiftButton, BLRedDotInventoryButton inventoryButton) {
            if (dailyGiftButton == null)
                return;
            UniTask.Void(async () => {
                await dailyGiftButton.CheckRedDot();
                //await inventoryButton.CheckRedDot();
            });
        }

        public void SaveBoosterStatus(BoosterStatus boosterStatus) {
            _storageManager.PvPBoosters = boosterStatus;
        }
        
        public void TrackConversionClickMarket() {
            var user = _userAccountManager.GetRememberedAccount();
            var str = user.isUserFi ? ConversionType.UserFiClickMarket : ConversionType.UserTrClickMarket;
            _analytics.TrackConversion(str);
        }

        public void PlaySoundTap() {
            _soundManager?.PlaySound(Audio.Tap);
        }
        
        public void ShowMarket() {
            MarketplaceScene.Scripts.MarketplaceScene.LoadScene();
        }

        public void ShowQuest() { }

        public void ShowShop() {
            ShopScene.Scripts.ShopScene.LoadScene();
        }

        public void ShowDailyGift() {
            const string sceneName = "DailyGiftScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }

        public void ShowAltar() {
            const string sceneName = "AltarScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }

        public void ShowInv(BLTabType tabType) {
            InventoryScene.Scripts.InventoryScene.LoadScene(tabType);
        }

        public void ShowHero() {
            const string sceneName = "HeroesScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }

        public async UniTask ShowTreasureHunt() {
            await LevelScene.LoadScene(GameModeType.TreasureHuntV2);
        }
        
        public void PlayStoryMode() {
            const string sceneName = "AdventureMenuScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }
    }
}