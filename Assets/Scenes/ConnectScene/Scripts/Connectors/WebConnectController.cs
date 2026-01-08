using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Analytics.Modules;
using App;
using CustomSmartFox;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Senspark;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Dialog;
using Share.Scripts.Utils;
using UnityEngine;
using LoginType = App.LoginType;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public class WebConnectController : IConnectController {
        /* Flow:
         * 1. Nếu chưa lưu account:
         *      1a. Hiện Dialog Choose Login Method
         *          - Nếu chọn Senspark -> Hiện Dialog Login Sen -> 2a.
         *          - Nếu chọn Guest & Facebook -> Do nothing
         *          - Nếu chọn Wallet -> 2a.
         *      2a. Hiện Dialog Choose Network Server
         *          - Nếu có Wallet -> Hiện Dialog Connect Wallet -> Login
         *          - Nếu không -> Login
         * 2. Nếu có lưu account:
         *  - Hiện Dialog Remember Login.
         *  - Nếu chọn Edit hoặc Close -> 1.
         */

        private readonly IMasterUnityCommunication _unityCommunication;
        private readonly IUserAccountManager _userAccountManager;
        private readonly ILogManager _logManager;
        private readonly IAuthManager _authManager;
        private readonly IAnalyticsModuleLogin _analytics;
        private readonly ITaskDelay _taskDelay;

        private readonly Stack<StateType> _stackStates;
        private readonly Canvas _canvasDialog;
        private readonly bool _isProduction;

        private UserAccount _userAccount;
        private TaskCompletionSource<UserAccount> _completion;

        public WebConnectController(
            IMasterUnityCommunication unityCommunication,
            IUserAccountManager userAccountManager,
            ILogManager logManager,
            IAnalyticsModuleLogin analytics,
            ITaskDelay taskDelay,
            Canvas canvasDialog,
            bool isProduction
        ) {
            _unityCommunication = unityCommunication;
            _isProduction = isProduction;
            _userAccountManager = userAccountManager;
            _logManager = logManager;
            _analytics = analytics;
            _taskDelay = taskDelay;
            _canvasDialog = canvasDialog;
            _authManager = new DefaultAuthManager(logManager, new NullSignManager(), new NullEncoder(logManager),
                unityCommunication, isProduction);
            _stackStates = new Stack<StateType>();
        }

        public async Task<UserAccount> StartFlow() {
            _completion = new TaskCompletionSource<UserAccount>();
            ApplyState(0);
            var result = await _completion.Task;
            return result;
        }

        private void ApplyState(StateType state) {
            switch (state) {
                case StateType.Done:
                    Completed();
                    return;
                case StateType.Error:
                    ResetAllState();
                    return;
                case StateType.Cancel:
                    CancelState();
                    return;
            }
            PushToStack(state);
            switch (state) {
                case StateType.WaitAniLogo:
                    ToWaitAniLogo();
                    break;
                case StateType.CheckAcceptTerms:
                    ToCheckAcceptTerms();
                    break;
                case StateType.CheckConnection:
                    ToCheckConnection();
                    break;
                case StateType.CheckRemember:
                    ToCheckRememberAccount();
                    break;
                case StateType.ChooseLoginMethod:
                    ToChooseLoginMethod();
                    break;
                case StateType.LoginGuest:
                    ToLoginGuest();
                    break;
                case StateType.LoginSenspark:
                    ToLoginSenspark();
                    break;
                case StateType.LoginWallet:
                    ToLoginWallet();
                    break;
                case StateType.ChooseNetworkAndServer:
                    ToChooseNetworkAndServer();
                    break;
                case StateType.ChooseWalletType:
                    ToChooseWalletType();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void ResetAllState() {
            _stackStates.Clear();
            ApplyState(0);
        }

        private void CancelState() {
            _stackStates.Pop();
            ApplyState(_stackStates.Count > 0 ? _stackStates.Peek() : 0);
        }

        private void PushToStack(StateType stateType) {
            if (_stackStates.Count == 0 || _stackStates.Peek() != stateType) {
                _stackStates.Push(stateType);
            }
        }

        private void Completed() {
            _logManager.Log("Login account completed");
            var lt = GetAnalyticsLoginType(_userAccount);
            _analytics.TrackAction(ActionType.ChooseLogin, lt);
            ;
            _completion.SetResult(_userAccount);
            
            //DevHoang: Enable WebGLInput capture all keyboard input
#if UNITY_WEBGL && !UNITY_EDITOR
            WebGLInput.captureAllKeyboardInput = true;
#endif
        }

        private void ToWaitAniLogo() {
            UniTask.Void(async () => {
                await _taskDelay.Delay(2000);
                ApplyState(StateType.CheckAcceptTerms);
            });
        }

        private void ToCheckAcceptTerms() {
            _userAccountManager.SetTermsServiceAccepted(true);
            ApplyState(StateType.CheckConnection);
        }
        
        private void ToCheckConnection() {
            UniTask.Void(async () => {
                var connection = await _unityCommunication.RequestConnection();
                ApplyState(connection ? StateType.LoginWallet : StateType.LoginSenspark);
            });
        }

        private void ToCheckRememberAccount() {
            _userAccount = _userAccountManager.GetRememberedAccount();

            void Resolve(UserAccount acc) {
                _logManager.Log("Resolve");
                _userAccount = acc;
                LogUserAccount(acc);
                var lt = GetAnalyticsLoginType(_userAccount);
                _analytics.TrackAction(ActionType.AutoLogin, lt);
                ApplyState(StateType.Done);
            }

            void Reject() {
                _logManager.Log("Reject");
                ApplyState(StateType.ChooseLoginMethod);
            }

            var controller =
                new ConnectContinueLoginController(_logManager, _authManager, _userAccount, Resolve, Reject);
            controller.ToCheckRememberAccount();
        }

        private void ToChooseLoginMethod() {
            void Resolve(ConnectMethodType type) {
                var state = type switch {
                    ConnectMethodType.Facebook => StateType.LoginFacebook,
                    ConnectMethodType.Apple => StateType.LoginApple,
                    ConnectMethodType.Telegram => StateType.LoginTelegram,
                    ConnectMethodType.Guest => StateType.LoginGuest,
                    ConnectMethodType.Senspark => StateType.LoginSenspark,
                    ConnectMethodType.Wallet => StateType.LoginWallet,
                    ConnectMethodType.Solana => StateType.LoginSolana,
                    _ => StateType.Cancel
                };
                ApplyState(state);
            }
            _ = DialogChooseLoginMethod.Create().ContinueWith(dialog => {
                dialog.StartFlow(Resolve, CancelState).Show(_canvasDialog);
                _analytics.TrackAction(ActionType.ShowDialogLogin, Analytics.Modules.LoginType.Unknown);
            });
        }

        private void ToLoginWallet() {
            _userAccount = new UserAccount {
                loginType = LoginType.Wallet,
                isUserFi = true
            };
            ToChooseNetworkAndServer();
        }
        
        private void ToLoginGuest() {
            void Resolve(UserAccount acc) {
                LogUserAccount(acc);
                _userAccount = acc;
                _userAccountManager.RememberUniqueGuestId(acc.userName, acc.id);
                ApplyState(StateType.Done);
            }
            var ctrl = new ConnectGuestController(
                _unityCommunication,
                _logManager,
                _userAccountManager,
                _analytics,
                GetServerAddress(),
                Resolve,
                CancelState,
                _canvasDialog);
            ctrl.ToLoginGuest();
        }

        private void ToLoginSenspark() {
            _userAccount = new UserAccount();
            if (_isProduction) {
                ToLoginSenspark_KnownServer();
                return;
            }

            void Resolve(UserAccount acc) {
                _userAccount.server = acc.server;
                ToLoginSenspark_KnownServer();
            }

            _ = DialogChooseNetworkServer.Create().ContinueWith(dialog => {
                dialog.InitServerAddresses(ServerAddress.TestServerAddresses)
                    .InitNetwork(false)
                    .StartFlow(Resolve, CancelState)
                    .Show(_canvasDialog);
            });
        }

        private void ToLoginSenspark_KnownServer() {
            void Resolve(UserAccount acc) {
                _userAccount.userName = acc.userName;
                _userAccount.id = acc.id;
                _userAccount.password = acc.password;
                _userAccount.jwtToken = acc.jwtToken;
                _userAccount.walletAddress = acc.walletAddress;
                _userAccount.isUserFi = acc.isUserFi;
                _userAccount.rememberMe = acc.rememberMe;
                _userAccount.loginType = LoginType.UsernamePassword;
                _userAccount.hasPasscode = acc.hasPasscode;
                
                _userAccount.network = acc.network;
                if (acc.server != null) {
                    _userAccount.server = acc.server;
                }
                if (_isProduction) {
                    var serverList = GetServerAddress();
                    _userAccount.server = serverList[0];
                }
                
                LogUserAccount(_userAccount);
                ApplyState(StateType.Done);
            }
            
            UniTask.Void(async () => {
                try {
                    var acc = new UserAccount();
                    Debug.Log($"!@#DevHoang unity login acc start");
                    _unityCommunication.UnityToReact.SendToReactNoEncrypted(ReactCommand.ENABLE_VIDEO_THUMBNAIL,
                        true.ToString());
                    await _unityCommunication.RequestLoginData();
                    acc = _unityCommunication.UnityToReact.GetLoginData();
                    Debug.Log($"!@#DevHoang unity login acc request login data {acc.network} - {acc.walletAddress} - {acc.userName} - {acc.password}");
                    
                    _unityCommunication.JwtSession.SetAccount(acc);
                    await _unityCommunication.Handshake();
                    Debug.Log($"!@#DevHoang unity login acc handshake");

                    acc.jwtToken = _unityCommunication.SmartFox.GetJwtForLogin();
                    acc.walletAddress = _unityCommunication.JwtSession.ExtraData.WalletAddress;
                    acc.isUserFi = _unityCommunication.JwtSession.ExtraData.IsUserFi;
                    Debug.Log($"!@#DevHoang unity login acc done");
                    Resolve(acc);
                } catch (Exception e) {
                    _logManager.Log($"Error when login account {e.Message}");
                    DialogOK.ShowError(_canvasDialog, "Username or password is incorrect", App.Utils.KickToConnectScene);
                } finally {
                    _unityCommunication.UnityToReact.SendToReactNoEncrypted(ReactCommand.ENABLE_VIDEO_THUMBNAIL, false.ToString());
                }
            });
        }

        private void ToChooseNetworkAndServer() {
            void Resolve(UserAccount acc) {
                _userAccount.network = acc.network;
                if (acc.server != null) {
                    _userAccount.server = acc.server;
                }
                LogUserAccount(_userAccount);
                var mustChooseWallet = _userAccount.loginType == LoginType.Wallet;
                ApplyState(mustChooseWallet ? StateType.ChooseWalletType : StateType.Done);
                
                //DevHoang: Disable WebGLInput capture all keyboard input (only for testing)
#if UNITY_WEBGL && !UNITY_EDITOR
                WebGLInput.captureAllKeyboardInput = false;
#endif
            }

            // Nếu đã chọn server rồi thì không chọn lại nữa
            // Nếu là user fi thì cho chọn network
            // Nếu ko còn gì nữa thì done luôn

            var mustChooseSvAddress = _userAccount.server != null ? null : GetServerAddress();
            var mustChooseNetwork =
                (_userAccount.isUserFi && _userAccount.loginType == LoginType.UsernamePassword);

            if (mustChooseSvAddress == null && !mustChooseNetwork) {
                ApplyState(StateType.Done);
            } else {
                if (!mustChooseNetwork && mustChooseSvAddress.Count == 1) {
                    // Nếu không cần chọn Network và SvAddress thì auto login
                    Resolve(new UserAccount {
                        server = mustChooseSvAddress[0]
                    });
                } else {
                    //DevHoang: Enable WebGLInput capture all keyboard input (only for testing)
#if UNITY_WEBGL && !UNITY_EDITOR
                    WebGLInput.captureAllKeyboardInput = true;
#endif
                    
                    _ = DialogChooseNetworkServer.Create().ContinueWith(dialog => {
                        dialog.InitServerAddresses(mustChooseSvAddress)
                            .InitNetwork(mustChooseNetwork)
                            .StartFlow(Resolve, CancelState)
                            .Show(_canvasDialog);
                    });
                }
            }
        }

        private async void ToChooseWalletType() {
            void Resolve(UserAccount acc) {
                if (acc.isUserFi) {
                    _userAccount.userName = acc.userName;
                    _userAccount.id = acc.id;
                    _userAccount.jwtToken = acc.jwtToken;
                    _userAccount.walletAddress = acc.walletAddress;
                    _userAccount.isUserFi = acc.isUserFi;
                }
                _userAccount.walletType = acc.walletType;
                LogUserAccount(_userAccount);
                ApplyState(StateType.Done);
            }
            try {
                //Các data cần thiết của 1 yser wallet cần đc set trước khi handshake
                var acc = new UserAccount {
                    id = 1,
                    isUserFi = true,
                    loginType = LoginType.Wallet
                };
                _unityCommunication.JwtSession.SetAccount(acc);
                await _unityCommunication.Handshake();
                var userName = _unityCommunication.JwtSession.ExtraData.WalletAddress;
                var chainId = _unityCommunication.UnityToReact.GetChainId();
                var jwt = _unityCommunication.SmartFox.GetJwtForLogin(userName);

                _userAccount.network = BlockChainNetwork.GetNetworkType(chainId);
                //DevHoang: Add new airdrop
#if UNITY_EDITOR
                if (AppConfig.IsRonin()) {
                    _userAccount.network = NetworkType.Ronin;
                }
                if (AppConfig.IsBase()) {
                    _userAccount.network = NetworkType.Base;
                }
                if (AppConfig.IsViction()) {
                    _userAccount.network = NetworkType.Viction;
                }
#else
                AppConfig.SetWebGLNetwork(_userAccount.network);
#endif
                
                //Update các data còn lại có đc sau khi handshake
                acc.userName = userName;
                acc.jwtToken = jwt;
                acc.walletAddress = userName;
                
                Resolve(acc);
            }
            catch (Exception e) {
                _logManager.Log($"Error login wallet: {e.Message}");
                DialogOK.ShowErrorAndKickToConnectScene(_canvasDialog, "Something went wrong\nPlease try again");
            }
        }

        private List<ServerAddress.Info> GetServerAddress() {
            List<ServerAddress.Info> prodServerAddress;
            if (AppConfig.IsTournament()) {
                prodServerAddress = ServerAddress.TournamentProServerAddress;
            } else {
                if (AppConfig.IsWebAirdrop()) {
                    prodServerAddress = ServerAddress.WebAirdropProdServerAddress;
                } else {
                    prodServerAddress = ServerAddress.ProdServerAddresses;
                }
            }
            return _isProduction
                ? prodServerAddress
                : ServerAddress.TestServerAddresses;
        }

        private void LogUserAccount(UserAccount acc) {
            if (acc == null) {
                _logManager.Log($"UserAccount: null");
                return;
            }
            _logManager.Log($"UserAccount: {JsonConvert.SerializeObject(acc)}");
        }

        private Analytics.Modules.LoginType GetAnalyticsLoginType(UserAccount usr) {
            var lt = usr.loginType switch {
                LoginType.UsernamePassword => Analytics.Modules.LoginType.Senspark,
                LoginType.Guest => Analytics.Modules.LoginType.Guest,
                LoginType.Apple => Analytics.Modules.LoginType.Apple,
                _ => Analytics.Modules.LoginType.Unknown
            };
            return lt;
        }

        private enum StateType {
            WaitAniLogo,
            CheckAcceptTerms,
            CheckConnection,
            CheckRemember,
            ChooseLoginMethod,
            LoginFacebook,
            LoginApple,
            LoginGuest,
            LoginSenspark,
            LoginTelegram,
            LoginSolana,
            LoginWallet,
            ChooseNetworkAndServer,
            ChooseWalletType,
            Done,
            Error,
            Cancel,
        }

        public enum ConnectMethodType {
            Guest,
            Facebook,
            Apple,
            Senspark,
            Wallet,
            Telegram,
            Solana
        }
    }
}