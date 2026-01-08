using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Analytics;
using Analytics.Modules;
using App;
using CustomSmartFox;
using Cysharp.Threading.Tasks;
using Exceptions;
using Game.ConnectControl;
using Game.Manager;
using Senspark;
using Services;
using Services.Server;
using Services.WebGL;
using Share.Scripts.Communicate;
using Share.Scripts.Services;
using UnityEngine;
using UnityEngine.Assertions;
using Utils;
using LoginType = App.LoginType;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public class GameReadyController : IGameReadyController {
        public static bool IgnoreMaintenance;

        private readonly Action<GameReadyProgress> _onProgressUpdate;
        private readonly CancellationTokenSource _cancellation;
        private readonly Canvas _canvasDialog;

        private IAnalyticsModuleLogin _analyticsModuleLogin;
        private Analytics.Modules.LoginType _analyticsLoginType = Analytics.Modules.LoginType.Unknown;
        private UniTaskCompletionSource _task;

        private bool _isPlayAnimationLoading;
        private int _progress;
        //Start, End, Title, Step
        private readonly Queue<(int,int, string, int)> _loadingQueue = new();
        
        public GameReadyController(Canvas canvasDialog, Action<GameReadyProgress> onProgressUpdate) {
            _canvasDialog = canvasDialog;
            _onProgressUpdate = onProgressUpdate;
            _cancellation = new CancellationTokenSource();
        }

        public void ResetProgress() {
            UpdateProgress(0, "");
        }

        /// <summary>
        /// Init tất cả service & account cần thiết để connect, login và..
        /// ..vào TH mode nếu acc IsUserFi
        /// ..vào MainMenu nếu ngược lại hoặc vào TH bị lỗi và reload với force to Main Menu 
        /// </summary>
        /// Exception: ServerMaintenanceException, TaskCanceledException, LoginException
        public async Task Start(int progressEnd, bool forceToMainMenu = false, bool isForceLogin = false) {
            if (_task != null) {
                await _task.Task;
                return;
            }
            try {
                _task = new UniTaskCompletionSource();
                var cancel = _cancellation.Token;
                UniTask.Void(async c => {
                    try {
                        var acc = await InitServices();
                        forceToMainMenu = AppConfig.IsTournament();
                        var keepContinue = await LogUserIn(acc, isForceLogin);
                        if (keepContinue) {
                            ActionType actionType;
                            if ((!forceToMainMenu && acc.loginType == LoginType.Wallet) || IsGoStraitToThMode()) {
                                actionType = ActionType.EnteringTreasureHunt;
                                keepContinue = await LoadForTh();
                            } else {
                                actionType = ActionType.EnteringMainMenu;
                                keepContinue = true; // Đem loadForMain vào Main Menu --- await LoadForMain();
                            }
                            if (keepContinue) {
                                await InitBeforeComplete(actionType, progressEnd);
                                
                                //DevHoang_20250725: Load blockchain data here to avoid login fail
                                if (AppConfig.IsBscOrPolygon()) {
                                    InitBlockchainData();
                                }
                            }
                        }

                        _task.TrySetResult();
                    } catch (Exception e) {
                        if (e is not TaskCanceledException && c.IsCancellationRequested) {
                            _task.TrySetException(new TaskCanceledException());
                            return;
                        }
                        _task.TrySetException(e);
                    }
                }, cancel);
                await _task.Task;
            } finally {
                _cancellation.Dispose();
            }
        }
        
        private void InitBlockchainData() {
            var blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            
            blockchainManager.GetBalance(RpcTokenCategory.Bcoin);
            blockchainManager.GetBalance(RpcTokenCategory.Bomb);
            blockchainManager.GetBalance(RpcTokenCategory.SenBsc);
            blockchainManager.GetBalance(RpcTokenCategory.SenPolygon);

            blockchainManager.GetHeroIdCounter();
            blockchainManager.GetClaimableHero();
            blockchainManager.GetPendingHero();
            blockchainManager.GetAvailableHouse();
            
            serverManager.General.SyncDeposited();
        }


        private bool IsGoStraitToThMode() {
            return AppConfig.IsAirDrop();
        }

        public void Cancel() {
            _isPlayAnimationLoading = false;
            App.Utils.Logout();
            if (!_cancellation.IsCancellationRequested) {
                try {
                    _cancellation.Cancel();
                    _task.TrySetException(new TaskCanceledException());
                    var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
                    var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            
                    serverManager?.Disconnect();
                    userAccountManager?.EraseData();
                } catch (Exception e) {
                    // ignore
                }
            }
        }

        private async UniTask<UserAccount> InitServices() {
            // Chưa biết tại sao đoạn này lại xuất hiện icon chờ
            // => tạm ẩn không cho hiện
            WaitingUiManager.Enable = false;
            var isProdBuild = AppConfig.IsProduction;
            var isEnableLog = AppConfig.EnableLog;
            var isSimulated = AppConfig.Simulated;
            var isEditor = Application.isEditor;
            var webParams = WebGLUtils.GetUrlParams();
            if (webParams.TryGetValue("enable_log", out var val)) {
                if (val == "true") {
                    isEnableLog = true;
                }
            }
            await ServiceUtils.InitializeBaseServices(isProdBuild, isEnableLog);
            
            var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            var encoder = ServiceLocator.Instance.Resolve<IExtResponseEncoder>();
            var unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            var logManager = new SimpleLogManager(isEnableLog);
            _analyticsModuleLogin = new FirebaseAnalyticsModuleLogin(isProdBuild || isEditor, logManager);
            
            if (AppConfig.IsSolana() || AppConfig.IsTon()) {
                await unityCommunication.Handshake();
            }
            
            var webglUtils = ServiceLocator.Instance.Resolve<IWebGLBridgeUtils>();
            var connectController = new DefaultConnectController(encoder, unityCommunication, userAccountManager, logManager, webglUtils,
                _analyticsModuleLogin, _canvasDialog, isProdBuild, IgnoreMaintenance);
            var acc = await connectController.StartFlow();
            unityCommunication.JwtSession.SetAccount(acc);
            UpdateLoadingBar(0,30,"Validate data",150);

            if (AppConfig.IsMobile()) {
                //Đã check lúc login trong menu scene r
                if (acc.skipCheckAccount) {
                    acc.skipCheckAccount = false;
                } 
                else {
                    await unityCommunication.Handshake();
                    acc.jwtToken = unityCommunication.SmartFox.GetJwtForLogin(String.Empty);    
                }
                
            }

            // rest lại icon chờ.
            WaitingUiManager.Enable = true;
            ValidateUserAccount(acc);
            /*
             * Lưu ý: isProdChosen != isProdBuild
             * Khi build test thì isProdChosen sẽ thay đổi phụ thuộc theo server mà user chọn
             * Còn isProdBuild sẽ cố định theo bản build là test hay prod
            */
            var (isProdChosen, serverConfig) = GetServerConfig(acc);
            await ServiceUtils.InitializeGameServices(
                isProdChosen,
                isEnableLog,
                serverConfig,
                acc.network
            );
            await ServiceUtils.InitializeAccount(isProdChosen, isSimulated, acc);
            await WebGLBlockchainInitializer.InitBlockchainConfig(acc.network, isProdChosen);
            
            var profileManager = ServiceLocator.Instance.Resolve<IProfileManager>();
            var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();

            WebGLOnApplicationQuit.Init();
            userAccountManager.RememberAccount(acc);
            if (acc.loginType == LoginType.Guest) {
                userAccountManager.RememberUniqueGuestId(acc.userName, acc.id);
            }
            profileManager.UpdateLoginTime(acc.userName, DateTime.UtcNow);

            var analyticsDependency = new DefaultAnalyticsDependency(userAccountManager);
            analytics.SetDependencies(analyticsDependency);

            TrackConnect(analytics, storageManager, acc);
            Reloader.StartListening(acc.loginType);
            return acc;
        }

        private async UniTask<bool> LogUserIn(UserAccount acc, bool isForceLogin) {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();

            try {
                _analyticsLoginType = acc.loginType switch {
                    LoginType.UsernamePassword => Analytics.Modules.LoginType.Senspark,
                    LoginType.Guest => Analytics.Modules.LoginType.Guest,
                    _ => Analytics.Modules.LoginType.Unknown
                };
                
                UpdateLoadingBar(30,40,"Connecting");
                await serverManager.Connect(45);
                if (_cancellation.IsCancellationRequested) {
                    return false;
                }
                UpdateLoadingBar(40,50, "Login");
                
                var loginData = GetLoginData(acc);
                var res = await serverManager.Login(loginData, isForceLogin);

                _analyticsModuleLogin.TrackAction(ActionType.LoginSuccess, _analyticsLoginType);
                if (_cancellation.IsCancellationRequested) {
                    return false;
                }

                // Save user meta data
                storageManager.IdTelegram = res.IdTelegram;
                storageManager.NewUser = res.IsNewUser;
                storageManager.MiningTokenType = res.TokenType;
                storageManager.NickName = res.NickName;
                if (!string.IsNullOrWhiteSpace(res.SecondUserName)) {
                    storageManager.Username = res.SecondUserName;
                }
                if (res.IsNewUser) {
                    analytics.TrackConversion(ConversionType.NewUser);
                }

                // Phải đợi server trả về init user controller thành công thì client
                // mới được gọi extension request trong serverManager lên server
                await serverManager.WaitForUserInitialized();
                
                return true;
            } catch (Exception e) {
                _loadingQueue.Clear();
                if (e is not LoginException) {
                    _analyticsModuleLogin.TrackAction(ActionType.LoginFailed, _analyticsLoginType);
                    userAccountManager.EraseData();
                }
                throw;
            }
        }
        
        private void UpdateLoadingBar(int from, int to, string title, int step = 100) {
            from = from < _progress ? _progress : from;
            to = Mathf.Clamp(to, _progress, 100);
            if (from >= to) {
                return;
            }
            _loadingQueue.Enqueue((from, to, title, step));
            if (_isPlayAnimationLoading) {
                return;
            }
            _isPlayAnimationLoading = true;
            UniTask.Void(async () => {
                while (_loadingQueue.Count > 0) {
                    var (startProgress, endProgress, progressTitle, progressStep) = _loadingQueue.Peek();
                    _progress = startProgress;
                    while (_progress < endProgress) {
                        await UniTask.Delay(progressStep);
                        if (!_isPlayAnimationLoading) {
                            _loadingQueue.Clear();
                            break;
                        }
                        _progress += 1;
                        UpdateProgress(_progress, progressTitle);
                    }
                    if(_loadingQueue.Count > 0) 
                        _loadingQueue.Dequeue();
                }
                _isPlayAnimationLoading = false;
            });
        }

        private async UniTask<bool> LoadForTh() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            var launchPadManager = ServiceLocator.Instance.Resolve<ILaunchPadManager>();
            var informationManager = ServiceLocator.Instance.Resolve<IInformationManager>();
            var financeUserLoader = ServiceLocator.Instance.Resolve<IFinanceUserLoader>();
            var playerStoreManager = ServiceLocator.Instance.Resolve<IPlayerStorageManager>();

            try {
                _isPlayAnimationLoading = false;
                UpdateProgress(60, "Download Reward Data");
                await serverManager.General.GetChestReward();

                UpdateProgress(65, "Download Config Data");
                await launchPadManager.SyncRemoteData();

                UpdateProgress(70, "Download Config Data");
                await informationManager.SyncRemoteData();
                
                
                UpdateProgress(75, "Download TreasureHunt Data");
                await financeUserLoader.LoadAsync(UpdateProgress);
                
                UpdateProgress(98, "Download TreasureHunt Map");
                playerStoreManager.LoadMap(GameModeType.TreasureHuntV2);
                UpdateProgress(100, "Download TreasureHunt Map");

                return !_cancellation.IsCancellationRequested;
            } catch (Exception e) {
                throw new LoginException(LoginException.ErrorType.LoadThMode,
                    "Loading Problems, please try again after a few seconds.");
            }
        }

        private async UniTask InitBeforeComplete(ActionType actionType, int progressEnd) {
            var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            var logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            var configManager = ServiceLocator.Instance.Resolve<IPvPServerConfigManager>();

            // Telegram và solana ko cần config pvp
            // WebGL Airdrop không cần config pvp
            //DevHoang: Open for all platforms
            // if (!AppConfig.IsAirDrop()) {
            if (true) {
                var pingManager = new DefaultPingManager(logManager, configManager);
                await pingManager.Initialize();
                ServiceLocator.Instance.Provide(pingManager);
            }

            _isPlayAnimationLoading = false;

            var desc = progressEnd == 100 ? "Completed" : "Get Reward";
            UpdateProgress(progressEnd, desc);
            analytics.TrackConversionActiveUser();

            // Completed
            _analyticsModuleLogin.TrackAction(actionType, _analyticsLoginType);
        }

        public static ILoginData GetLoginData(UserAccount acc) {
            ILoginData loginData;
            // FI
            if (acc.isUserFi) {
                var userName = acc.loginType == LoginType.UsernamePassword ? acc.userName : acc.walletAddress;
                loginData = new LoginDataUserFi(userName, acc.jwtToken, acc.network);
            }

            // TR
            else if ((AppConfig.IsWebGL()||AppConfig.IsMobile()) && acc.loginType == LoginType.UsernamePassword) {
                loginData = new LoginDataUserTr(acc.userName, acc.jwtToken);
            }
            
            //Telegram
            else if (AppConfig.IsTon() && acc.jwtToken != string.Empty) {
                loginData = new LoginDataTelegram(acc.jwtToken, acc.userName,acc.thirdPartyAccessToken, acc.referralCode, acc.platform);
            }
            
            //Solana
            else if(AppConfig.IsSolana() && acc.jwtToken != string.Empty) {
                loginData = new LoginDataSolana(acc.jwtToken, acc.userName, acc.thirdPartyAccessToken, acc.platform);
            }

            // GUEST
            else {
                loginData = new LoginDataUserGuest(acc.userName, acc.jwtToken);
            }
            return loginData;
        }

        private void UpdateProgress(int progress, string details) {
            _onProgressUpdate?.Invoke(new GameReadyProgress(progress, details));
        }

        private static void TrackConnect(
            IAnalytics analytics,
            IStorageManager storageManager,
            UserAccount userAccount
        ) {
            // Track First Open
            analytics.TrackConversionRetention();
            if (storageManager.LastOpened == DateTime.MinValue) {
                storageManager.LastOpened = DateTime.UtcNow;
                analytics.TrackConversion(ConversionType.FirstOpen);
            }

            // Track Connect
            switch (userAccount.loginType) {
                case LoginType.Wallet: {
                    var t = userAccount.network == NetworkType.Binance
                        ? ConversionType.ConnectWalletBnb
                        : ConversionType.ConnectWalletPolygon;
                    analytics.TrackConversion(t);
                    break;
                }
                case LoginType.UsernamePassword:
                    if (userAccount.isUserFi) {
                        var t = userAccount.network == NetworkType.Binance
                            ? ConversionType.ConnectAccountBnb
                            : ConversionType.ConnectAccountPolygon;
                        analytics.TrackConversion(t);
                    } else {
                        analytics.TrackConversion(ConversionType.ConnectAccount);
                    }
                    break;
                case LoginType.Guest:
                    analytics.TrackConversion(ConversionType.ConnectGuest);
                    break;
                case LoginType.Apple:
                    analytics.TrackConversion(ConversionType.ConnectApple);
                    break;
                case LoginType.Telegram:
                    analytics.TrackConversion(ConversionType.ConnectTelegram);
                    break;
                case LoginType.Solana:
                    analytics.TrackConversion(ConversionType.ConnectSolana);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ValidateUserAccount(UserAccount acc) {
            // Ko cần validate id vì id ko dùng ở client
            //Assert.IsTrue(acc.id > 0);
            Assert.IsFalse(string.IsNullOrWhiteSpace(acc.userName));
            if (acc.loginType == LoginType.Telegram) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(acc.jwtToken));
            }
            if (acc.loginType == LoginType.Wallet) {
                Assert.IsTrue(acc.isUserFi);
            }
            if (acc.isUserFi) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(acc.walletAddress));
            }
            if (acc.loginType == LoginType.UsernamePassword) {
                Assert.IsFalse(string.IsNullOrWhiteSpace(acc.password));
            }
            if (acc.loginType == LoginType.Guest) {
                // validate nothing
            }
        }

        private static (bool, IServerConfig) GetServerConfig(UserAccount acc) {
            // Version này sẽ lấy dựa theo user chọn server nào
            var svInfo = acc.server;
            var chosenServer = svInfo.Address;
            var isProdAddress = ServerAddress.IsMainServerAddress(chosenServer);
            var buildConfig = new DefaultBuildConfig(isProdAddress);
            var serverConfig = new EditorServerConfig(buildConfig.Version, svInfo.Address, svInfo.Port, false,
                svInfo.IsEncrypted, buildConfig.Salt);
            return (isProdAddress, serverConfig);
        }
    }
}
