using System;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.Communicate;
using Share.Scripts.Dialog;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.Connects {
    public class AfDialogLoginSensparkAcc : AfDialog<LoginResult> {
        [SerializeField]
        private TMP_InputField inpUserName;

        [SerializeField]
        private TextMeshProUGUI txtUserNameError;

        [SerializeField]
        private TMP_InputField inpPassword;

        [SerializeField]
        private TextMeshProUGUI txtPasswordError;
        
        [SerializeField]
        private Button btnLogin;

        [SerializeField]
        private Toggle tggRevealPassword;
        
        [SerializeField]
        private Button btnForgotPassword;

        [SerializeField]
        private Button btnClose;
        
        private IAuthManager _authManager;
        private IUserAccountManager _userAccountManager;
        private IMasterUnityCommunication _unityCommunication;

        protected override void Awake() {
            base.Awake();
            btnLogin.onClick.AddListener(OnBtnLoginClicked);
            btnForgotPassword.onClick.AddListener(OnBtnForgotPasswordClicked);
            btnClose.onClick.AddListener(OnBtnCloseClicked);
            tggRevealPassword.onValueChanged.AddListener(OnRevealPasswordChanged);
            
            _authManager = ServiceLocator.Instance.Resolve<IAuthManager>();
            _userAccountManager = ServiceLocator.Instance.Resolve<IUserAccountManager>();
            _unityCommunication = ServiceLocator.Instance.Resolve<IMasterUnityCommunication>();
            
            ClearErrorTexts();
            inpUserName.characterLimit = CreateAccountHelper.UsrMaxLength;
            inpPassword.characterLimit = CreateAccountHelper.PwdMaxLength;
        }
        
        private void ClearErrorTexts() {
            txtUserNameError.text = null;
            txtPasswordError.text = null;
        }

        private void OnBtnLoginClicked() {
            PlayClickSound();
            ClearErrorTexts();
            var usrName = inpUserName.text.Trim();
            var pwd = inpPassword.text.Trim();
            
            var result = CheckUserName(usrName) && CheckPassword1(pwd);
            if (!result) {
                return;
            }
            ShowWaiting();
            UniTask.Void(async () => {
                try {
                    //Tạo account chứa các thông tin cơ bản cho biết đây là user mobile account sau đó gọi api để lấy thông tin đầy đủ
                    var acc = new UserAccount {
                        userName = usrName,
                        password = pwd,
                        server = Data.CurrentServer,
                        loginType = LoginType.UsernamePassword,
                    };
                    _unityCommunication.ResetSession();
                    _unityCommunication.JwtSession.SetAccount(acc);
                    await _unityCommunication.Handshake();
                    var extraData = _unityCommunication.JwtSession.ExtraData;
                    var jwt = _unityCommunication.SmartFox.GetJwtForLogin(string.Empty);
                    // var res = await _authManager.GetUserLoginDataByPassword(usrName, pwd);
                    acc.jwtToken = jwt;
                    acc.id = extraData.Uid;
                    acc.isUserFi = extraData.IsUserFi;
                    acc.walletAddress = extraData.IsUserFi ? extraData.WalletAddress : null;
                    acc.rememberMe = true;
                    acc.skipCheckAccount = true;
                    _unityCommunication.JwtSession.SetAccount(acc);
                    
                    Resolve(new LoginResult(false, acc));
                } catch (Exception e) {
                    DialogOK.ShowError(Data.CurrentCanvas, "Wrong username or password");
                } finally {
                    HideWaiting();
                }
            });
        }
        
        private void OnBtnCloseClicked() {
            PlayClickSound();
            Reject();
        }

        private void OnBtnForgotPasswordClicked() {
            PlayClickSound();
            Resolve(new LoginResult(true, null));
        }
        
        private void OnRevealPasswordChanged(bool val) {
            inpPassword.contentType = val ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            inpPassword.ForceLabelUpdate();
        }
        
        private bool CheckUserName(string str) {
            try {
                return CreateAccountHelper.CheckUserName(str);
            } catch (Exception e) {
                txtUserNameError.text = e.Message;
                return false;
            }
        }
        
        private bool CheckPassword1(string str) {
            try {
                return CreateAccountHelper.CheckPassword(str);
            } catch (Exception e) {
                txtPasswordError.text = e.Message;
                return false;
            }
        }
    }
}