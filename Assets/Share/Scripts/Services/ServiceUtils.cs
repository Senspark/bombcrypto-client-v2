// #define DEBUG_ANALYTICS

using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics;

using App;

using Communicate;
using Communicate.Encrypt;

using CustomSmartFox;

using Engine.Input;

using Game.Dialog;
using Game.Manager;

using Notification;

using Scenes.TreasureModeScene.Scripts.Service;

using Senspark;

using Services;
using Services.DeepLink;
using Services.IapAds;
using Services.RemoteConfig;
using Services.WebGL;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using Ton.Task;

using UnityEngine;

using Utils;

using IDataManager = App.IDataManager;

namespace Share.Scripts.Services {
    public static class ServiceUtils {
        public static bool IsInitialized => _initializedBaseServices;

        private static bool _initializedBaseServices;
        private static readonly List<IService> DestroyAfterUseServices = new();
        private static bool _initializedAnalytics;
        private static bool _initializedRemoteConfig;

        public static async Task InitializeRemoteConfig(bool production, bool enableLog) {
            if (_initializedRemoteConfig) {
                return;
            }
            _initializedRemoteConfig = true;
            var logManager = new DefaultLogManager(enableLog);
            await logManager.Initialize();
            ServiceLocator.Instance.Provide(logManager);
#if UNITY_WEBGL || UNITY_STANDALONE
            var remoteConfig = new NullRemoteConfig();
#else
            var remoteConfig = new DefaultRemoteConfig(logManager);
            await remoteConfig.Initialize();
            await InitRemoteConfig(logManager, remoteConfig);
#endif
            ServiceLocator.Instance.Provide(remoteConfig);
        }

        private static async Task InitRemoteConfig(
            ILogManager logManager,
            IRemoteConfig remoteConfig
        ) {
            var defaultValues = await RemoteConfigHelper.ReadRemoteConfigDefaultValues(logManager);
            remoteConfig.SetDefaultValues(defaultValues);
            await remoteConfig.SyncData();
        }

        public static async Task InitializeAnalytics(bool production, bool enableLog) {
            if (_initializedAnalytics) {
                return;
            }
            _initializedAnalytics = true;
            ThreadUltils.Thread.Initialize();
            var result = await Platform.RequestTrackingAuthorization();
            var logManager = new SimpleLogManager(enableLog);
            var fullAnalytics = result != TrackingAuthorizationStatus.Denied &&
                                result != TrackingAuthorizationStatus.Restricted;
#if UNITY_STANDALONE
            fullAnalytics = false;
#endif
            if (fullAnalytics) {
                var multiAnalytics = new MultiAnalytics();
                ServiceLocator.Instance.Provide(multiAnalytics);
#if DEBUG_ANALYTICS
                production = true;
#endif
                await InitializeNotification(production, enableLog);
                var revenueValidator = new AppsFlyerValidator.Builder().Build();
                var firebase = new BaseFirebaseAnalytics(production, logManager);
                var appsFlyer = new AppsFlyerAnalytics(production, logManager);
                var costCenter = new CostCenterAnalytics(production, logManager, revenueValidator);
                multiAnalytics.AddBridge(firebase);
                multiAnalytics.AddBridge(appsFlyer);
                multiAnalytics.AddBridge(costCenter);
                multiAnalytics.SetDependencies(new NullAnalyticsDependency());
                await multiAnalytics.Initialize();
            } else {
                await InitializeNotification(production, enableLog);
                var nullAnalytics = new NullAnalytics(logManager);
                ServiceLocator.Instance.Provide(nullAnalytics);
            }
        }

        private static async Task InitializeNotification(bool production, bool enableLog) {
#if UNITY_WEBGL || UNITY_EDITOR || UNTIY_STANDALONE
            var notificationManager = new NullNotificationManager();
            ServiceLocator.Instance.Provide(notificationManager);
#else
            var logManager = new SimpleLogManager(enableLog);
            var analyticsManager = new Senspark.MultiAnalyticsManager(
                new FirebaseAnalyticsManager.Builder().Build() 
                );
            var notificationManager = new NotificationManager(analyticsManager);
            //var messagingManager = new FirebaseMessagingManager(notificationManager);
            //await messagingManager.Initialize();
            await notificationManager.Initialize();
            ServiceLocator.Instance.Provide(notificationManager);
#endif
        }

        /// <summary>
        /// Chỉ init 1 lần duy nhất
        /// </summary>
        public static async Task InitializeBaseServices(bool production, bool enableLog) {
            if (_initializedBaseServices) {
                DestroyAfterUseServices.ForEach(e => e.Destroy());
                DestroyAfterUseServices.Clear();
                return;
            }
            _initializedBaseServices = true;

            var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            var logManager = ServiceLocator.Instance.Resolve<ILogManager>();

            //var logManager = new DefaultLogManager(enableLog);
            var sceneManager = new SceneManager();
            var languageManager = new DefaultLanguageManager();
            var dataManager = new DefaultDataManager(new LocalDataStorage());
            var soundManager = new DefaultSoundManager(dataManager);
            var networkConfig = new NullNetworkConfig();
            var userAccountManager = new DefaultUserAccountManager(logManager);
            var iapManager = new DefaultUnityPurchaseManager(logManager, analytics);
            var avatarTRManager = new AvatarTRManager();
            var hotKeyManager = new HotkeyControlManager(dataManager);
            var onBoardingObserver = new OnBoardingManager();

            // Các services này nên gộp vào IDialogManager
            var fusionManager = new DefaultFusionManager();
            var repairShieldManager = new DefaultRepairShieldManager();

            // Service này nên bỏ
            var stakeManager = new DefaultStakeManager();
            var ads = new NullUnityAdsManager();
            
            //New services for solana
            var unityCommunication = new MasterUnityCommunication(logManager);
            var encoder = new ExtResponseEncoder(unityCommunication);
            
            var webGLBridge = new DefaultWebGLBridgeUtils(logManager, unityCommunication);
            
            //Init object handle input from controller
            var obj = new GameObject("InputHandler");
            var inputManager = obj.AddComponent<InputManager>();
            //Support for controller input webgl with dialog
            var dialogManager = new DefaultDialogManager(logManager);

            ServiceLocator.Instance.Provide(sceneManager);
            var services = new List<IService> {
                languageManager,
                networkConfig,
                userAccountManager,
                fusionManager,
                repairShieldManager,
                stakeManager,
                iapManager,
                ads,
                avatarTRManager,
                soundManager,
                hotKeyManager,
                dataManager,
                webGLBridge,
                unityCommunication,
                encoder,
                inputManager,
                dialogManager,
                onBoardingObserver,
            };

            foreach (var s in services) {
                await s.Initialize();
                ServiceLocator.Instance.Provide(s);
            }
        }

        /// <summary>
        /// Init lại khi cần
        /// </summary>
        public static async Task InitializeGameServices(
            bool production,
            bool enableLog,
            IServerConfig serverConfig,
            NetworkType networkType
        ) {
            // Base Services
            var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            var purchaseManager = ServiceLocator.Instance.Resolve<IUnityPurchaseManager>();
            var languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            var dataManager = ServiceLocator.Instance.Resolve<IDataManager>();
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            var encoder = ServiceLocator.Instance.Resolve<IExtResponseEncoder>();
            var onBoardingManager = ServiceLocator.Instance.Resolve<IOnBoardingManager>();


            // Game Services
            var networkConfig = new DefaultNetworkConfig(production, networkType);
            var apiManager = new DefaultApiManager(networkConfig, logManager, production);
            var gameDataRemoteManager = new DefaultGameDataRemoteManager(logManager, networkConfig);
            var storeManager = new DefaultStoreManager(dataManager);
            var houseStoreManager = new DefaultHouseStoreManager(dataManager, analytics);
            var playerStoreManager = new DefaultPlayerStoreManager(houseStoreManager);
            var chestRewardManager = new DefaultChestRewardManager(networkType);
            var claimTokenServerBridge = new DefaultClaimTokenServerBridge(networkType);


            // Server
#if UNITY_WEBGL
            ITaskDelay taskDelay = WebGLTaskDelay.Instance;
#else
            ITaskDelay taskDelay = new EditorTaskDelay();
#endif
            //Telegram Only
            ITaskTonManager taskTonManager = AppConfig.IsTon()
                ? new TaskTonManager(playerStoreManager, houseStoreManager, logManager)
                : new TaskNone();
            
            var serverNotifyManger = new DefaultServerNotifyManager(logManager);
            var serverManager = new GameServerManager(enableLog, serverConfig, logManager, storeManager,
                playerStoreManager, houseStoreManager, chestRewardManager, claimTokenServerBridge, taskDelay,
                taskTonManager, onBoardingManager, userAccountManager, unityCommunication, encoder, serverNotifyManger);
            var pvpBoosterManager = new PvpMode.Manager.DefaultBoosterManager();
            var storyModeManager = new DefaultStoryModeManager(storeManager, playerStoreManager, languageManager,
                serverManager, chestRewardManager);
            var blockchainStorageManager = new DefaultBlockchainStorageManager(networkType);
            var informationManager = new DefaultInformationManager(logManager, serverManager.CacheRequestManager);
            var launchpadManager =
                new DefaultLaunchPadManager(logManager, chestRewardManager, blockchainStorageManager,
                    serverManager.CacheRequestManager);
            var airDropManager = new DefaultAirDropManager(logManager);
            var pveManager = new DefaultPveModeManager(networkType);
            var equipmentManager = new DefaultEquipmentManager();
            var pveHeroStateManager = new DefaultPveHeroStateManager(logManager, serverManager);
            var newsManager = new DefaultNewsManager(logManager, apiManager);
            var emailManager = new DefaultEmailManager(serverManager);
            var dailyMissionManager = new DefaultDailyMissionManager(serverManager);
            var chestNameManager = new GachaChestNameManager();
            var skinManager = new SkinManager(serverManager);
            var itemUseDurationManager = new ItemUseDurationManager(logManager);
            var pvpBombRankManager = new PvPBombRankManager();
            var heroAbilityManager = new EmptyHeroAbilityManager();
            var productManager = new ProductManager(logManager);
            var heroColorManager = new HeroColorManager(logManager);
            var financeUserLoader = new FinanceUserLoader();
            var abilityManager = new AbilityManager();
            var profileManager = new NullProfileManager();
            var tutorialManager = new BLTutorialManager();
            var gachaChestItemManager = new UniqueGachaChestItemManager();

            // Need Gacha Chest Item Manager
            var serverRequester = new NewServerRequester(logManager, serverManager, 
                gachaChestItemManager, serverManager, chestRewardManager);
            var inventoryManager = new InventoryManager(gachaChestItemManager, chestNameManager,
                logManager, serverRequester, userAccountManager);

            // Need Server Requester
            var earlyConfigManager = new EarlyConfigManager(serverRequester);
            var iapItemManager = new IAPItemManager(serverRequester, serverManager, purchaseManager, production);
            var boosterManager = new BoosterManager(serverRequester);
            var trHeroManager = new TRHeroManager(logManager, serverRequester, serverManager);
            var rankInfoManager = new RankInfoManager(serverRequester);
            var freeTRHeroManager = new FreeTRHeroManager(logManager, serverRequester);
            var luckyWheelManager = new LuckyWheelManager(serverRequester);
            var pvPServerConfigManager = new PvPServerConfigManager(serverRequester, taskDelay);
            var shopManager = new ShopManager(serverRequester);
            var offlineRewardManager = new DefaultOfflineRewardManager(serverRequester);
            var dailyRewardManager = new DailyRewardManager(analytics, serverRequester);
            var dailyTaskManager = new DailyTaskManager(logManager, serverRequester);

            // Need Early Config
            var heroIdManager = new HeroIdManager(earlyConfigManager);
            var heroStatsManager = new HeroStatsManager(logManager, earlyConfigManager);
            var itemDescriptionManager = new ProductItemManager(earlyConfigManager);

            // Other
            var deeplink = new DefaultDeepLinkListener();
            var clientLogManager = new ClientLogManager(serverNotifyManger, serverManager);

            var services = new List<IService> {
                networkConfig,
                languageManager,
                apiManager,
                informationManager,
                gameDataRemoteManager,
                storeManager,
                playerStoreManager,
                houseStoreManager,
                chestRewardManager,
                serverManager,
                serverRequester,
                pvpBoosterManager,
                storyModeManager,
                launchpadManager,
                airDropManager,
                pveManager,
                equipmentManager,
                pveHeroStateManager,
                newsManager,
                emailManager,
                dailyMissionManager,
                chestNameManager,
                gachaChestItemManager,
                inventoryManager,
                blockchainStorageManager,
                skinManager,
                iapItemManager,
                itemUseDurationManager,
                boosterManager,
                trHeroManager,
                itemDescriptionManager,
                earlyConfigManager,
                rankInfoManager,
                pvpBombRankManager,
                freeTRHeroManager,
                heroIdManager,
                heroAbilityManager,
                heroStatsManager,
                productManager,
                heroColorManager,
                dailyRewardManager,
                luckyWheelManager,
                pvPServerConfigManager,
                financeUserLoader,
                abilityManager,
                profileManager,
                shopManager,
                tutorialManager,
                offlineRewardManager,
                deeplink,
                taskTonManager,
                dailyTaskManager,
                serverNotifyManger,
                clientLogManager
            };

            foreach (var s in services) {
                await s.Initialize();
                ServiceLocator.Instance.Provide(s);
            }

            DestroyAfterUseServices.AddRange(services);
        }

        public static async Task InitializeAccount(
            bool production,
            bool simulated,
            UserAccount acc
        ) {
            var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            var apiManager = ServiceLocator.Instance.Resolve<IApiManager>();
            var storeManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            var chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            var blockchainStorageManager = ServiceLocator.Instance.Resolve<IBlockchainStorageManager>();
            var networkType = acc.network;
            var earlyConfigManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            var remoteConfig = ServiceLocator.Instance.Resolve<IRemoteConfig>();
            var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();

            IBlockchainManager blockchainManager;
            IClaimManager claimManager;

            var accountManager = new BomberLandAccountManager(acc);
#if UNITY_EDITOR && FORCE_WALLET_LOGIN_ON_EDITOR
            acc.loginType = acc.isUserFi ? LoginType.Wallet : acc.loginType;
#endif
            var featureManager = new DefaultFeatureManager(acc);
            var signManager = new NewSignManager(logManager);
            var authManager = new DefaultAuthManager(
                logManager,
                signManager,
                new NullEncoder(logManager),
                unityCommunication,
                production);

#if UNITY_EDITOR
            blockchainManager = new EditorBlockchainManager(accountManager, simulated);
            claimManager = new MobileClaimManager();
#elif UNITY_WEBGL
            if (acc.loginType == LoginType.UsernamePassword) {
                blockchainManager = new MobileBlockchainManager(logManager, accountManager, apiManager);
                claimManager = new MobileClaimManager();
            } else {
                IBlockchainBridge blockChainBridge = new WebGLBlockchainBridge(logManager, unityCommunication);
                blockchainManager = new WebGLBlockchainManager(accountManager, logManager, blockChainBridge, apiManager,
                    networkType, acc.walletType, production);
                claimManager = new DefaultClaimManager(production, featureManager.EnableClaim, accountManager,
                    storeManager, blockChainBridge);
            }
#else // android & ios
            blockchainManager = new MobileBlockchainManager(logManager, accountManager, apiManager);
            claimManager = new MobileClaimManager();
#endif
            var autoUpdateBlockchainManager = new AutoUpdateBlockchainManager(blockchainManager,
                blockchainStorageManager, storeManager, featureManager);
            var claimTokenManager = new DefaultClaimTokenManager(serverManager, autoUpdateBlockchainManager,
                chestRewardManager);

            IUnityAdsManager ads;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            ads = new MobileUnityAdsManager(logManager, remoteConfig, acc, analytics);
#else
            ads = new NullUnityAdsManager();
#endif
            var services = new List<IService>() {
                accountManager,
                claimManager,
                autoUpdateBlockchainManager,
                featureManager,
                claimTokenManager,
                authManager,
                ads,
            };

            foreach (var s in services) {
                await s.Initialize();
                ServiceLocator.Instance.Provide(s);
            }

            DestroyAfterUseServices.AddRange(services);
        }
    }
}