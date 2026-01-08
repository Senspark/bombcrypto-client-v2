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
    public class AfDialogCreateSensparkAcc : AfDialog<UserAccount> {
        [SerializeField]
        private TMP_InputField inpUserName;

        [SerializeField]
        private TextMeshProUGUI txtUserNameError;

        [SerializeField]
        private TMP_InputField inpPassword;

        [SerializeField]
        private TextMeshProUGUI txtPasswordError;

        [SerializeField]
        private TMP_InputField inpPassword2;

        [SerializeField]
        private TextMeshProUGUI txtPassword2Error;

        [SerializeField]
        private TMP_InputField inpEmail;

        [SerializeField]
        private TextMeshProUGUI txtEmailError;

        [SerializeField]
        private Button btnCreate;

        [SerializeField]
        private Button btnClose;
        
        private IServerManager _serverManager;


        protected override void Awake() {
            base.Awake();
            btnCreate.onClick.AddListener(OnBtnCreateClicked);
            btnClose.onClick.AddListener(OnBtnCloseClicked);

            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            ClearErrorTexts();
            inpUserName.characterLimit = CreateAccountHelper.UsrMaxLength;
            inpPassword.characterLimit = CreateAccountHelper.PwdMaxLength;
            inpPassword2.characterLimit = CreateAccountHelper.PwdMaxLength;
        }

        private void ClearErrorTexts() {
            txtUserNameError.text = null;
            txtPasswordError.text = null;
            txtPassword2Error.text = null;
            txtEmailError.text = null;
        }

        private async void OnBtnCreateClicked() {
            PlayClickSound();
            ClearErrorTexts();
            var usrName = inpUserName.text.Trim();
            var pwd1 = inpPassword.text.Trim();
            var pwd2 = inpPassword2.text.Trim();
            var email = inpEmail.text.Trim();

            var result = CheckUserName(usrName) &&
                         CheckPassword1(pwd1) &&
                         CheckPassword2(pwd1, pwd2) &&
                         CheckEmail(email);
            if (!result) {
                return;
            }
            ShowWaiting();

            try {
                // var uid = await _unityCommunication.MobileRequest.RegisterAccountSenspark(usrName, pwd1, email); 
                // var res = await _authManager.Register(usrName, pwd1, email);
                var usr = new UserAccount {
                    id = 1,
                    userName = usrName,
                    password = pwd1,
                    //jwtToken = res.,
                    // thirdPartyAccessToken = res.ThirdPartyAccessToken,
                    server = Data.CurrentServer,
                    hasPasscode = false,
                    loginType = LoginType.UsernamePassword,
                    isUserFi = false,
                    rememberMe = true,
                    email = email
                };
                
                var resultLink = await LinkAccount(usr);
                if (resultLink) {
                    Resolve(usr);
                } else {
                    DialogOK.ShowError(Data.CurrentCanvas, "Username or email is already exist");
                }
            } catch (Exception e) {
                DialogOK.ShowError(Data.CurrentCanvas, "Username or email is already exist");
            } finally {
                HideWaiting();
            }
        }

        private void OnBtnCloseClicked() {
            PlayClickSound();
            Reject();
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

        private bool CheckPassword2(string pwd1, string pwd2) {
            try {
                return CreateAccountHelper.CheckPassword2(pwd1, pwd2);
            } catch (Exception e) {
                txtPassword2Error.text = e.Message;
                return false;
            }
        }

        private bool CheckEmail(string str) {
            try {
                return CreateAccountHelper.CheckEmail(str);
            } catch (Exception e) {
                txtEmailError.text = e.Message;
                return false;
            }
        }
        
        private async UniTask<bool> LinkAccount(UserAccount newUserAccount) {
            try {
                var result = await _serverManager.General.LinkUser(newUserAccount);
                return result;

            } catch (Exception e) {
                return false;
                // ignore
                // FIXME: nếu tạo acc mới rồi mà link lỗi thì cũng ko biết phải thế nào
            }
        }
    }
}