using System;
using System.Threading.Tasks;
using Analytics;
using Analytics.Modules;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using Data;
using Exceptions;
using Game.Dialog;
using Game.UI;
using Scenes.ConnectScene.Scripts.Connectors;
using Scenes.TutorialScene.Scripts;
using Senspark;
using Services;
using Services.RemoteConfig;
using Share.Scripts.Dialog;
using Share.Scripts.Services;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using LoginType = App.LoginType;

namespace Scenes.ConnectScene.Scripts {
    public class ConnectScene : MonoBehaviour {
        [SerializeField]
        private Text appVersionLbl;

        [SerializeField]
        protected Canvas canvasDialog;

        [SerializeField]
        private MainLoadingBar mainLoadingBar;

        [SerializeField]
        private Button cancelBtn;
        
        [SerializeField]
        private Animator logoAnimator;

        private IAnalyticsModuleLogin _analyticsModuleLogin;
        private IGameReadyController _gameReadyController;
        private NewcomerGiftData[] _newcomerGift;
        private bool _remoteReady;
        
        private void Awake() {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;
            
            //DevHoang: Disable WebGLInput capture all keyboard input
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = false;
#endif   
            mainLoadingBar.gameObject.SetActive(false);
            cancelBtn.gameObject.SetActive(false);
            cancelBtn.onClick.AddListener(Cancel);

            UniTask.Void(async () => {
                try {
                    _remoteReady = false;
                    var isProdBuild = AppConfig.IsProduction;
                    var isEnableLog = AppConfig.EnableLog;
                    await ServiceUtils.InitializeRemoteConfig(AppConfig.IsProduction, AppConfig.EnableLog);
                    await ServiceUtils.InitializeAnalytics(isProdBuild, isEnableLog);
                    var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
                    analytics.TrackScene(SceneType.ChooseLoginScene);
                    _remoteReady = true;
                } catch (Exception e) {
                    Debug.LogError(e);
                    // Debug.LogError(e.Message);
                }
            });
        }

        private void Start() {
            SetVersion();
            MainMenuScene.Scripts.MainMenuScene.IsDataLoaded = false;
            if (AppConfig.IsMobile() && RequestTutorial()) { 
                UniTask.Void(async () => {
                    //delay 3 giây remote config Initialize 
                    await UniTask.Delay(2000);
                    // Đảm bảo mọi base service đã init xong
                    await UniTask.WaitUntil(() => _remoteReady);
                    
                    var remoteConfig = ServiceLocator.Instance.Resolve<IRemoteConfig>();
                    var isEnableTutorial = RemoteConfigHelper.IsEnableTutorial(remoteConfig);
                    if (isEnableTutorial) {
                        LoadingToTutorial();
                    } else {
                        Loading(false, true);
                    }
                });
            } else {
                Loading();
                InitDebugTools();
            }
        }

        private bool RequestTutorial() {
            var userAccountManager = new DefaultUserAccountManager(new DefaultLogManager(true));
            var hasAccount = userAccountManager.HasAccount();
            var haveGuest = userAccountManager.HasAccGuest();
            if (!haveGuest && !hasAccount) {
                return BLLevelScenePvpTutorial.IsRequestTutorial;
            }
            return false;
        }

        private async Task TutorialGetReward(bool isShowDialogReward) {
            var analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            var storageManager = ServiceLocator.Instance.Resolve<IStorageManager>();
            analytics.TrackScene(SceneType.RewardTutorial);
            var serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _newcomerGift = await serverRequester.GetNewcomerGift();
            analytics.TrackClaimRewardTutorial();
            // Check ưu tiên equip Hero
            var heroes = await ServiceLocator.Instance.Resolve<ITRHeroManager>().GetHeroesAsync("HERO");
            foreach (var hero in heroes) {
                if (hero.ItemId != (int) GachaChestProductId.Ninja) {
                    continue;
                }
                storageManager.SelectedHeroKey = hero.InstanceId;
                await serverRequester.ActiveTRHero(hero.InstanceId);
                break;
            }
        }

        public static async Task LoadForMain() {
            var serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            await serverManager.General.GetChestReward();

            //DevHoang: Open for all platforms
            // if (AppConfig.IsBscOrPolygon()) {
            if (true) {
                var earlyConfigManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
                await serverManager.General.GetConfig();
                await earlyConfigManager.InitializeAsync();
            }
        }
        
        private void LoadingToTutorial() {
            //DevHoang: Enable WebGLInput capture all keyboard input
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = true;
#endif
            
            UniTask.Void(async () => {
                try {
                    // Tạm hard code, chờ 1.0s diễn xong animation của Logo
                    await WebGLTaskDelay.Instance.Delay(1000);
                    mainLoadingBar.gameObject.SetActive(true);
                    mainLoadingBar.UpdateProgress(10, "Loading");
                    var isProdBuild = AppConfig.IsProduction;
                    var isEnableLog = AppConfig.EnableLog;
                    await ServiceUtils.InitializeBaseServices(isProdBuild, isEnableLog);
                    mainLoadingBar.UpdateProgress(80, "Loading");
                    await WebGLTaskDelay.Instance.Delay(100);
                    mainLoadingBar.UpdateProgress(90, "Loading");
                    await WebGLTaskDelay.Instance.Delay(200);
                    mainLoadingBar.UpdateProgress(100, "Completed");
                    const string sceneName = "TutorialScene";
                    await SceneLoader.LoadSceneAsync(sceneName);
                } catch (Exception e) {
                    if (e is TaskCanceledException te) {
                        App.Utils.KickToConnectScene();
                        return;
                    }
                    if (e is ServerMaintenanceException se) {
                        DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, "Server is under maintenance");
                        return;
                    }
                    if (e is LoginException le) {
                        if (le.Error == LoginException.ErrorType.WrongVersion) {
                            const string msg =
                                "Your current version is outdated and needs to be updated to run the game";
                            DialogOK.ShowErrorAsync(canvasDialog, msg, new DialogOK.Optional { OnWillHide = QuitApp })
                                .Forget();
                            return;
                        }
                    }
                    Debug.LogException(e);
                    DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, e.Message);
                }
            });
        }

        private void Loading(bool reload = false, bool bypassTutorial = false, bool isForceLogin = false) {
            UniTask.Void(async () => {
                try {
                    async UniTask OpenTreasureHuntMode() {
                        await LevelScene.OpenTreasureHuntWithoutLoad();
                    }
                    
                    const string sceneName = "MainMenuScene";
                    void GoToMainMenu() {
                        SceneLoader.LoadSceneAsync(sceneName).Forget();
                    }

                    void OnMainMenuLoaded(GameObject obj) {
                        var mainMenu = obj.GetComponent<MainMenuScene.Scripts.MainMenuScene>();
                        mainMenu.NewcomerGift = _newcomerGift;
                        MainMenuScene.Scripts.MainMenuScene.IsDataLoaded = false;
                    }
                    void GoToMainMenuWithReward() {
                        SceneLoader.LoadSceneAsync(sceneName, OnMainMenuLoaded).Forget();
                    }
                    
                    if (reload) {
                        logoAnimator.Play("Intro_logo_bomb2", -1, 0);
                    } else {
                        if (!bypassTutorial) {
                            // Tạm hard code, chờ 1.0s diễn xong animation của Logo
                            await WebGLTaskDelay.Instance.Delay(1000);
                        }
                    }
                    _gameReadyController = new GameReadyController(canvasDialog, OnProgressUpdate);
                    var progressEnd = bypassTutorial ? 98 : 100;
                    await _gameReadyController.Start(progressEnd, reload, isForceLogin);

                    var userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
                    var user = userAccountManager.GetRememberedAccount();
                    var loginType = user?.loginType ?? LoginType.Guest;
                    var canOpenThMode = (!reload && loginType == LoginType.Wallet && !AppConfig.IsTournament()) || AppConfig.IsTon() ||
                                        AppConfig.IsSolana();
                    if (canOpenThMode) {
                        await LoadForMain();
                        mainLoadingBar.MoveDown(() => { OpenTreasureHuntMode().Forget(); });
                    } else {
                        if (bypassTutorial) {
                            await LoadForMain();
                            await TutorialGetReward(true);
                            mainLoadingBar.UpdateProgress(100, "Completed");
                            mainLoadingBar.MoveDown(GoToMainMenuWithReward);
                        } else {
                            mainLoadingBar.MoveDown(GoToMainMenu);
                        }
                    }
                    if (AppConfig.IsTon()) {
                        ServiceLocator.Instance.Resolve<IServerManager>().InitKickUserListener();
                    }
                } catch (Exception e) {
                    if (e is TaskCanceledException or OperationCanceledException) {
                        App.Utils.KickToConnectScene();
                        return;
                    }
                    if (e is TonOldDataException) {
                        DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, e.Message);
                        return;
                    }
                    if (e is IncorrectPassword) {
                        DialogOK.ShowError(canvasDialog, e.Message);
                        return;
                    }
                    if (e is ServerMaintenanceException se) {
                        DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, "Server is under maintenance");
                        return;
                    }
                    if(e is BanException be) {
                        var dialogBan = await DialogBan.Create();
                        dialogBan.Show(canvasDialog, be.BanCode, be.ExpireAt);
                        return;
                    }
                    if (e is LoginException le) {
                        if (le.Error == LoginException.ErrorType.WrongVersion) {
                            const string msg =
                                "Your current version is outdated and needs to be updated to run the game";
                            DialogOK.ShowErrorAsync(canvasDialog, msg, new DialogOK.Optional { OnWillHide = QuitApp })
                                .Forget();
                            return;
                        }
                        if (le.Error == LoginException.ErrorType.LoadThMode) {
                            var dialog = await DialogError.ShowErrorDialog(canvasDialog, le.Message);
                             dialog.OnDidHide(()=> {
                                 // Reset Loading Bar for reLoading...
                                 _gameReadyController.ResetProgress();
                                 mainLoadingBar.gameObject.SetActive(false);
                                 cancelBtn.gameObject.SetActive(false);
                                 Loading(true);
                             });
                            return;
                        }
                        if (le.Error == LoginException.ErrorType.AlreadyLogin) {
                            // Ton và Sol đươc phép force login
                            if(AppConfig.IsTon() || AppConfig.IsSolana()) {
                                var dialog = await DialogAlreadyLogin.Create();
                                dialog.SetContinueCallback(() => {
                                    // Reset Loading Bar for reLoading...
                                    _gameReadyController.ResetProgress();
                                    mainLoadingBar.gameObject.SetActive(false);
                                    cancelBtn.gameObject.SetActive(false);
                                    Loading(true, false, true);
                                });
                                dialog.Show(canvasDialog);
                                return;
                            }
                            // Nếu không phải Ton hay Sol thì sẽ kick
                            var userName = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount()?.userName;
                            userName = userName == null ? "Your account" : UserAccount.TryRemoveSuffixInUserName(userName);
                            var m =
                                $"{userName} is already logged-in on another device";
                            DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, m);
                            return;

                        }
                        if (le.Error == LoginException.ErrorType.KickByOtherDevice) {
                            var dialog = await DialogError.ShowError(canvasDialog, 
                                "Your account is currently logged in on another device.\n Try again after a few minutes"
                                ,App.Utils.Logout);
        
                            return;
                        }
                    }
                    if (e is NoInternetException) {
                        var dialog = await AfDialogCheckConnection.Create();
                        dialog.Show(canvasDialog);
                        await dialog.WaitForHide();
                        App.Utils.KickToConnectScene();
                        return;
                    }
                    Debug.LogException(e);
                    var message = e.Message;
                    if (message.Contains("Connection error")) {
                        message = "Connection error \n Please check your internet connection and try again";
                    }
                    if (message == "kick") {
                        var userName = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount()?.userName;
                        userName = userName == null ? "Your account" : UserAccount.TryRemoveSuffixInUserName(userName);
                        message = $"{userName} is already logged-in on another device";
                    }
                    DialogOK.ShowErrorAndKickToConnectScene(canvasDialog, message);
                }
            });
        }

        private void Cancel() {
            _gameReadyController.Cancel();
        }

        private void SetVersion() {
            var buildTime = Resources.Load<TextAsset>("configs/BuildTimeStamp");
            var isProd = AppConfig.IsProduction;
            var prodTxt = isProd ? string.Empty : " (test)";
            appVersionLbl.text = $"{AppConfig.GetVersion(isProd)}{prodTxt}{Environment.NewLine}{buildTime.text}";
        }

        private static void QuitApp() {
            App.Utils.GoToStore();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnProgressUpdate(GameReadyProgress data) {
            if (data.Progress > 3 && data.Progress <= 5) {
                mainLoadingBar.gameObject.SetActive(true);
            }
            mainLoadingBar.UpdateProgress(data.Progress, data.Details);
        }

        private void InitDebugTools() {
            var obj = new GameObject("Debug Tools");
            obj.AddComponent<EditorDisconnectButton>();
        }
    }
}