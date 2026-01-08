using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Game.Dialog.AIO;

using Scenes.ConnectScene.Scripts;

using Services.Server.Exceptions;

using Share.Scripts.Dialog;

using UnityEngine;

namespace Game.Dialog {
    public class BLConnect : MonoBehaviour {
        [SerializeField]
        private GameObject facebook;

        [SerializeField]
        private GameObject metamask;

        [SerializeField]
        private GameObject senspark;
        
        [SerializeField]
        private GameObject apple;

        private UserAccount _newUserAccount;
        private UserAccount _currentUserAccount;

        private ILogManager _logManager;
        private IUserAccountManager _userAccountManager;
        private IAuthManager _authManager;
        private IServerManager _serverManager;

        private Canvas _canvas;
        private List<Action> _didHides;
        private Action _hide;
        private bool _isProduction;
        private LoginType _requestLoginType;

        private void Awake() {
            _newUserAccount = new UserAccount();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _authManager = ServiceLocator.Instance.Resolve<IAuthManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _currentUserAccount = _userAccountManager.GetRememberedAccount();
            _isProduction = ServerAddress.IsMainServerAddress(_currentUserAccount.server.Address);
            
            var isEditor = Application.isEditor;
            var platform = Application.platform;
            metamask.SetActive(false);
            senspark.SetActive(isEditor ||
                               platform
                                   is RuntimePlatform.Android
                                   or RuntimePlatform.IPhonePlayer
                                   or RuntimePlatform.WebGLPlayer);
            apple.SetActive(isEditor || platform == RuntimePlatform.IPhonePlayer);
        }

        private void ApplyState(State state) {
            switch (state) {
                case State.ChooseServerAndNetwork:
                    StateShowNetworkAndServer();
                    break;
                case State.Login:
                    StateLogin();
                    break;
                case State.LinkAccount:
                    StateLinkProfile();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        public void Initialize(Dialog dialog) {
            _canvas = dialog.DialogCanvas;
            _didHides = new List<Action>();
            _hide = () => dialog.Hide();
            dialog.OnDidHide(OnDidHide);
        }

        public void OnButtonSensparkClicked() {
            _requestLoginType = LoginType.UsernamePassword;
            _didHides.Add(() => ApplyState(State.ChooseServerAndNetwork));
            _hide();
        }

        public void OnButtonFacebookClicked() {
#if UNITY_ANDROID || UNITY_IOS
            _didHides.Add(() => ApplyState(State.ChooseServerAndNetwork));
            _hide();
#endif
        }
        
        public void OnButtonAppleClicked() {
#if UNITY_IOS
            _requestLoginType = LoginType.Apple;
            _didHides.Add(() => ApplyState(State.ChooseServerAndNetwork));
            _hide();
#endif      
        }

        public void OnButtonMetamaskClicked() {
#if UNITY_WEBGL
            _requestLoginType = LoginType.Wallet;
            _didHides.Add(() => ApplyState(State.ChooseServerAndNetwork));
            _hide();
#endif
        }

        private void OnDidHide() {
            foreach (var didHide in _didHides) {
                didHide();
            }
        }

        private void StateLogin() {
            switch (_requestLoginType) {
                case LoginType.Wallet:
                    StateLoginMetamask();
                    break;
                case LoginType.UsernamePassword:
                    StateLoginSenspark(() => ApplyState(State.LinkAccount));
                    break;
                case LoginType.Telegram:
                    StateLoginTelegram(() => ApplyState(State.LinkAccount));
                    break;
                case LoginType.Apple:
                    StateLoginApple(() => ApplyState(State.LinkAccount));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private async void StateLoginSenspark(Action onCompleted) {
            var dialog = await DialogLogInSenspark.Create();
            dialog.Init(new DialogLogInSenspark.OptionsData {
                Production = _isProduction,
                Shadow = 153f,
                ShowRemember = false,
                Transform = new DialogLogInSenspark.RectTransformData {
                    AnchorMax = new Vector2(.5f, .5f),
                    AnchorMin = new Vector2(.5f, .5f),
                    Pivot = new Vector2(.5f, .5f),
                    Position = new Vector2(0f, 0f),
                }
            }).StartFlow(
                res => {
                    _newUserAccount.userName = res.userName;
                    _newUserAccount.id = res.id;
                    _newUserAccount.password = res.password;
                    _newUserAccount.jwtToken = res.jwtToken;
                    _newUserAccount.walletAddress = res.walletAddress;
                    _newUserAccount.isUserFi = res.isUserFi;
                    _newUserAccount.rememberMe = res.rememberMe;
                    _newUserAccount.loginType = LoginType.UsernamePassword;
                    _newUserAccount.hasPasscode = res.hasPasscode;
                    _newUserAccount.rememberMe = true;
                    onCompleted();
                },
                () => { }
            ).Show(_canvas);
        }
        
        private void StateLoginTelegram(Action onCompleted) {
            UniTask.Void(async () => {
                try {
                    var res = await _authManager.GetUserLoginDataByThirdParty(ThirdPartyLogin.Telegram);
                    _newUserAccount.userName = res.UsernameOrWallet;
                    _newUserAccount.thirdPartyAccessToken = res.ThirdPartyAccessToken;
                    _newUserAccount.id = res.UserId;
                    _newUserAccount.jwtToken = res.JwtToken;
                    _newUserAccount.hasPasscode = res.HasPasscode;
                    _newUserAccount.loginType = LoginType.Telegram;
                    _newUserAccount.rememberMe = true;
                    onCompleted();
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(_canvas, e.Message);    
                    } else {
                        DialogOK.ShowError(_canvas, e.Message);
                    }
                }
            });
        }
        
        private void StateLoginApple(Action onCompleted) {
            UniTask.Void(async () => {
                try {
                    var res = await _authManager.GetUserLoginDataByThirdParty(ThirdPartyLogin.Apple);
                    _newUserAccount.userName = res.UsernameOrWallet;
                    _newUserAccount.id = res.UserId;
                    _newUserAccount.jwtToken = res.JwtToken;
                    _newUserAccount.hasPasscode = res.HasPasscode;
                    _newUserAccount.loginType = LoginType.Apple;
                    _newUserAccount.rememberMe = true;
                    onCompleted();
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(_canvas, e.Message);    
                    } else {
                        DialogOK.ShowError(_canvas, e.Message);
                    }
                }
            });
        }

        private async void StateLoginMetamask() {
            //Ko dùng dialog connect nữa
            return;
            IBlockchainConfig networkConfig;
            switch (_newUserAccount.network)
            {
                case NetworkType.Binance:
                    networkConfig = new BinanceBlockchainConfig(_isProduction);
                    break;
                case NetworkType.Polygon:
                    networkConfig = new PolygonBlockchainConfig(_isProduction);
                    break;
                case NetworkType.Ton:
                    networkConfig = new TonBlockchainConfig(_isProduction);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var dialog = await DialogConnectWallet.Create();
            dialog.Init(true, _isProduction, networkConfig.NetworkId)
                .StartFlow(acc => {
                    _newUserAccount.userName = acc.userName;
                    _newUserAccount.id = acc.id;
                    _newUserAccount.jwtToken = acc.jwtToken;
                    _newUserAccount.walletAddress = acc.walletAddress;
                    _newUserAccount.isUserFi = acc.isUserFi;
                    _newUserAccount.hasPasscode = acc.hasPasscode;
                    _newUserAccount.loginType = LoginType.Wallet;
                    _newUserAccount.rememberMe = true;
                }, () => { })
                .Show(_canvas);
        }

        private void StateShowNetworkAndServer() {
            _newUserAccount.server = _currentUserAccount.server;
            ApplyState(State.Login);
        }

        private void StateLinkProfile() {
            UniTask.Void(async () => {
                try {
                    // FIXME: wrong wallet account
                    Debug.LogError("Not implemented yet");
                    await _serverManager.General.LinkUser(_newUserAccount);
                    
                    // Save new account
                    _userAccountManager.EraseData();
                    _userAccountManager.EraseGuest();
                    _userAccountManager.RememberAccount(_newUserAccount);
                    App.Utils.KickToConnectScene();
                    
                } catch (Exception e) {
                    _logManager.Log(e.Message);
                    if (e is ErrorCodeException) {
                        DialogError.ShowError(_canvas, e.Message);    
                    } else {
                        DialogOK.ShowError(_canvas, e.Message);
                    }
                }
            });
        }

        private enum State {
            ChooseServerAndNetwork,
            Login,
            LinkAccount
        }
    }
}