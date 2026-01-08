using System;

using App;

using BLPvpMode.UI;

using Communicate;

using CustomSmartFox;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Manager;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Dialog;
using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.ConnectScene.Scripts {
    public class DialogLogInSenspark : Dialog {
        public struct OptionsData {
            public bool Production;
            public bool ShowRemember;
            public float Shadow;
            public RectTransformData Transform;
        }

        public struct RectTransformData {
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector3 Position;
        }

        [SerializeField]
        private InputField userNameTxt;
        
        [SerializeField]
        private InputField passwordTxt;

        [SerializeField]
        private RectTransform panel;

        [SerializeField]
        private GameObject remember;

        [SerializeField]
        private Toggle rememberToggle;

        [SerializeField]
        private Button buttonLogin;
        
        public const string PREF_SENSPARK_ACC = "PREF_SENSPARK_ACC"; 
        public const string PREF_SENSPARK_PWD = "PREF_SENSPARK_PWD"; 
        
        private ISoundManager _soundManager;
        private ILogManager _logManager;
        private IAuthManager _authManager;
        private IMasterUnityCommunication _unityCommunication;
        private Action<UserAccount> _resolve;
        private Action _reject;
        private bool _processing;
        private bool _isProduction;
        
        public static UniTask<DialogLogInSenspark> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogLogInSenspark>();
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            IgnoreOutsideClick = true;
#if UNITY_EDITOR
            userNameTxt.text = AppConfig.TestUserName;
            passwordTxt.text = AppConfig.TestPassword;
#endif
            if (PlayerPrefs.HasKey(PREF_SENSPARK_ACC)) {
                userNameTxt.text = PlayerPrefs.GetString(PREF_SENSPARK_ACC);
            }
            if (PlayerPrefs.HasKey(PREF_SENSPARK_PWD)) {
                passwordTxt.text = PlayerPrefs.GetString(PREF_SENSPARK_PWD);
            }
        }

        public DialogLogInSenspark Init(OptionsData data) {
            BackgroundAlpha = data.Shadow;
            panel.localPosition = data.Transform.Position;
            panel.anchorMin = data.Transform.AnchorMin;
            panel.anchorMax = data.Transform.AnchorMax;
            panel.pivot = data.Transform.Pivot;
            remember.SetActive(data.ShowRemember);
            return Init(data.Production);
        }

        public DialogLogInSenspark Init(bool isProduction) {
            _isProduction = isProduction;
            var signManager = new NullSignManager();
            _authManager = new DefaultAuthManager(_logManager, signManager, new NullEncoder(_logManager), _unityCommunication, isProduction);
            return this;
        }

        public DialogLogInSenspark StartFlow(Action<UserAccount> resolve, Action reject) {
            _resolve = resolve;
            _reject = reject;
            OnDidHide(() => _reject?.Invoke());
            return this;
        }

        private void Update() {
            if (Input.GetKeyDown(KeyBoardInputDefine.Login) ||
                Input.GetKeyDown(KeyBoardInputDefine.LoginKeyPad) ||
                Input.GetButtonDown(ControllerInputDefine.Login) ||
                Input.GetButtonDown(ControllerInputDefine.LoginKeyPad)) {
                OnLoginBtnClicked();
            }
        }

        public void OnLoginBtnClicked() {
            if (!buttonLogin.interactable) {
                return;
            }
            buttonLogin.interactable = false;
            _soundManager.PlaySound(Audio.Tap);
            Process(userNameTxt.text, passwordTxt.text);
        }

        public void OnCreateAccountBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var url = _isProduction
                ? "https://dapp.bombcrypto.io/account/register"
                : "";
            WebGLUtils.OpenUrl(url, true);
        }

        public void OnForgotPasswordBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            var url = _isProduction
                ? "https://dapp.bombcrypto.io/account/login"
                : "";
            WebGLUtils.OpenUrl(url, true);
        }

        public void OnBackBtnClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _reject?.Invoke();
            Clear();
            Hide();
        }

        private void Clear() {
            _resolve = null;
            _reject = null;
        }
        
        private void OnCompleted(UserAccount acc) {
            acc.rememberMe = rememberToggle.isOn;
            ConsiderSaveAccount(acc);
            _resolve?.Invoke(acc);
            Clear();
            Hide();
        }

        private static void ConsiderSaveAccount(UserAccount acc) {
            if (acc.rememberMe) {
                PlayerPrefs.SetString(PREF_SENSPARK_ACC, acc.userName);
                PlayerPrefs.SetString(PREF_SENSPARK_PWD, acc.password);
            } else {
                PlayerPrefs.DeleteKey(PREF_SENSPARK_ACC);
                PlayerPrefs.DeleteKey(PREF_SENSPARK_PWD);
            }
            PlayerPrefs.Save();
        }

        private async void Process(string username, string password) {
            if (_processing) {
                return;
            }
            if (_authManager == null) {
                buttonLogin.interactable = true;
                DialogOK.ShowError(DialogCanvas, "Auth service not started");
                return;
            }
            var errorMsg = App.Utils.CheckUsernameAndPassword(username, password); 
            if (!string.IsNullOrWhiteSpace(errorMsg)) {
                buttonLogin.interactable = true;
                DialogOK.ShowError(DialogCanvas, errorMsg);
                return;
            }
            _processing = true;
            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            
            // tạo account tạm để lưu user và pass, và lấy về jwt, sau khi login sẽ có account đầy đủ thông tin
            var acc = new UserAccount {
                userName = username,
                password = password,
                loginType = LoginType.UsernamePassword
            };
            try {
                _unityCommunication.JwtSession.SetAccount(acc);
                await _unityCommunication.Handshake();

                acc.jwtToken = _unityCommunication.SmartFox.GetJwtForLogin();
                acc.walletAddress = _unityCommunication.JwtSession.ExtraData.WalletAddress;
                acc.isUserFi = _unityCommunication.JwtSession.ExtraData.IsUserFi;
                OnCompleted(acc);
            } catch (Exception e) {
                _logManager.Log($"Error when login account {e.Message}");
                DialogOK.ShowError(DialogCanvas, "Username or password is incorrect");
            } finally {
                _processing = false;
                waiting.End();
                buttonLogin.interactable = true;
            }
            
        }
    }
}