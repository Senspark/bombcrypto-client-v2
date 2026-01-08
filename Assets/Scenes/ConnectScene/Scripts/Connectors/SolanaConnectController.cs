using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Analytics.Modules;

using App;

using CustomSmartFox;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

using Services.WebGL;

using Share.Scripts;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

using LoginType = App.LoginType;

namespace Scenes.ConnectScene.Scripts.Connectors {
    public class SolanaConnectController : IConnectController {

        private readonly IUserAccountManager _userAccountManager;
        private readonly ILogManager _logManager;
        private readonly IAuthManager _authManager;
        private readonly IExtResponseEncoder _encoder;
        private readonly IMasterUnityCommunication _unityCommunication;
        private readonly IAnalyticsModuleLogin _analytics;
        private readonly ITaskDelay _taskDelay;
        private readonly IWebGLBridgeUtils _webGLBridgeUtils;

        private readonly Stack<StateType> _stackStates;
        private readonly Canvas _canvasDialog;
        private readonly bool _isProduction;

        private UserAccount _userAccount;
        private TaskCompletionSource<UserAccount> _completion;

        public SolanaConnectController(
            IExtResponseEncoder encoder,
            IMasterUnityCommunication unityCommunication,
            IUserAccountManager userAccountManager,
            ILogManager logManager,
            IWebGLBridgeUtils webGLBridgeUtils,
            IAnalyticsModuleLogin analytics,
            ITaskDelay taskDelay,
            Canvas canvasDialog,
            bool isProduction
        ) {
            _isProduction = isProduction;
            _userAccountManager = userAccountManager;
            _encoder = encoder;
            _unityCommunication = unityCommunication;
            _logManager = logManager;
            _webGLBridgeUtils = webGLBridgeUtils;
            _analytics = analytics;
            _taskDelay = taskDelay;
            _canvasDialog = canvasDialog;
            _authManager = new DefaultAuthManager(logManager, new NullSignManager(), encoder, unityCommunication, isProduction);
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
                case StateType.LoginSolana:
                    ToLoginThirdParty(ThirdPartyLogin.Solana);
                    break;
                case StateType.ChooseNetworkAndServer:
                    ToChooseNetworkAndServer();
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
            _completion.SetResult(_userAccount);
        }



        private void ToWaitAniLogo() {
            UniTask.Void(async () => {
                await _taskDelay.Delay(2000);
                ApplyState(StateType.CheckAcceptTerms);
            });
        }

        private void ToCheckAcceptTerms() {
            // Designer đổi luồng, mặc định accept Terms
            _userAccountManager.SetTermsServiceAccepted(true);
            ApplyState(StateType.LoginSolana);
        }

        private void ToLoginThirdParty(ThirdPartyLogin type) {
            void Resolve(UserAccount acc) {
                LogUserAccount(acc);
                _userAccount = acc;
                ApplyState(StateType.Done);
            }
            var ctrl = new ConnectThirdPartyController(
                _encoder,
                _unityCommunication,
                _logManager,
                _webGLBridgeUtils,
                GetServerAddress(),
                type,
                Resolve,
                CancelState,
                _canvasDialog);
            ctrl.ToLoginThirdParty();
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
            }

            // Nếu đã chọn server rồi thì không chọn lại nữa
            // Nếu là user fi thì cho chọn network
            // Nếu ko còn gì nữa thì done luôn

            var mustChooseSvAddress = _userAccount.server != null ? null : GetServerAddress();
            //var isEditorOrWebGL = Application.isEditor || Application.platform == RuntimePlatform.WebGLPlayer;
            var mustChooseNetwork =
                /*isEditorOrWebGL &&*/ (_userAccount.isUserFi || _userAccount.loginType == LoginType.Wallet);

            if (mustChooseSvAddress == null && !mustChooseNetwork) {
                ApplyState(StateType.Done);
            } else {
                if (!mustChooseNetwork && mustChooseSvAddress.Count == 1) {
                    // Nếu không cần chọn Network và SvAddress thì auto login
                    Resolve(new UserAccount {
                        server = mustChooseSvAddress[0]
                    });
                } else {
                    _ = DialogChooseNetworkServer.Create().ContinueWith(dialog => {
                        dialog.InitServerAddresses(mustChooseSvAddress)
                            .InitNetwork(mustChooseNetwork)
                            .StartFlow(Resolve, CancelState)
                            .Show(_canvasDialog);
                    });
                }
            }
        }
        
        private List<ServerAddress.Info> GetServerAddress() {
            List<ServerAddress.Info> prodServerAddress;
            if (AppConfig.IsTournament()) {
                prodServerAddress = ServerAddress.TournamentProServerAddress;
            } else if (AppConfig.IsTon()) {
                prodServerAddress = ServerAddress.TelegramProdServerAddress;
            }
            else if(AppConfig.IsSolana()) {
                prodServerAddress = ServerAddress.SolanaProdServerAddress;
            }
            else {
                prodServerAddress = ServerAddress.ProdServerAddresses;
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

    }
}