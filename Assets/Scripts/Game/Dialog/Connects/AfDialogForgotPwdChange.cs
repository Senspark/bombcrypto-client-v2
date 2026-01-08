using System;
using System.Collections.Generic;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Services.DeepLink;

using Share.Scripts.Dialog;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using Utils;

using Toggle = UnityEngine.UI.Toggle;

namespace Game.Dialog.Connects {
    public class AfDialogForgotPwdChange : AfDialog<bool> {
        [SerializeField]
        private TMP_InputField inpPwd1;

        [SerializeField]
        private TextMeshProUGUI txtErrorPwd1;
        
        [SerializeField]
        private TMP_InputField inpPwd2;

        [SerializeField]
        private TextMeshProUGUI txtErrorPwd2;
        
        [SerializeField]
        private TMP_InputField inpToken;

        [SerializeField]
        private TextMeshProUGUI txtErrorToken;
        
        [SerializeField]
        private Button btnChangePwd;
        
        [SerializeField]
        private Button btnClose;
        
        [SerializeField]
        private List<Toggle> tggRevealPassword;
        
        private IAuthManager _authManager;
        private IDeepLinkListener _deepLinkListener;
        
        private ObserverHandle _handle;
        private List<ContentSizeFitter> _contentSizeFitters;

        protected override void Awake() {
            base.Awake();
            
            _contentSizeFitters = new List<ContentSizeFitter>(GetComponentsInChildren<ContentSizeFitter>());
            btnChangePwd.onClick.AddListener(OnBtnChangePwdClicked);
            btnClose.onClick.AddListener(OnBtnCloseClicked);
            tggRevealPassword.ForEach(e=>e.onValueChanged.AddListener(OnRevealPasswordChanged));

            _authManager = ServiceLocator.Instance.Resolve<IAuthManager>();
            _deepLinkListener = ServiceLocator.Instance.Resolve<IDeepLinkListener>();
            _handle = new ObserverHandle();
            _handle.AddObserver(_deepLinkListener, new DeepLinkObserver {
                OnDeepLinkReceived = OnDeepLinkReceived
            });
            
            SetTokenInput(null, null, false);
            inpToken.characterLimit = CreateAccountHelper.ForgotPwdTokenLength;
            ClearErrorTexts();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _handle.Dispose();
        }

        private void OnDeepLinkReceived() {
            var data = _deepLinkListener.GetDeepLinkData();
            if (data.TryGetValue(DeepLinkKey.Token, out var token)) {
                if (!string.IsNullOrWhiteSpace(token) && token.Length == CreateAccountHelper.ForgotPwdTokenLength) {
                    inpToken.text = token;
                }
            }
        }

        private void OnBtnChangePwdClicked() {
            PlayClickSound();
            ClearErrorTexts();
            
            var pwd1 = inpPwd1.text.Trim();
            var pwd2 = inpPwd2.text.Trim();
            var token = inpToken.text.Trim();
            if (string.IsNullOrWhiteSpace(token)) {
                token = Data.ForgotPasswordToken;
            }

            var result = ValidatePassword1(pwd1) && ValidatePassword2(pwd1, pwd2) && ValidateToken(token);
            if (!result) {
                return;
            }
            ShowWaiting();
            UniTask.Void(async () => {
                try {
                    await _authManager.ResetPassword(token, pwd1);
                    await DialogOK.ShowInfoAsync(Data.CurrentCanvas, "Password changed successfully",
                        new DialogOK.Optional {
                            WaitUntilHidden = true
                        });
                    Resolve(true);
                } catch (Exception e) {
                    SetTokenInput(Data.ForgotPasswordToken, e.Message, true);
                } finally {
                    HideWaiting();
                }
            });
        }
        
        private void OnBtnCloseClicked() {
            PlayClickSound();
            Reject();
        }
        
        private void OnRevealPasswordChanged(bool val) {
            inpPwd1.contentType = inpPwd2.contentType =
                val ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
            inpPwd1.ForceLabelUpdate();
            inpPwd2.ForceLabelUpdate();
        }
        
        private bool ValidatePassword1(string str) {
            try {
                return CreateAccountHelper.CheckPassword(str);
            } catch (Exception e) {
                txtErrorPwd1.text = e.Message;
                return false;
            }
        }
        
        private bool ValidatePassword2(string pwd1, string pwd2) {
            try {
                return CreateAccountHelper.CheckPassword2(pwd1, pwd2);
            } catch (Exception e) {
                txtErrorPwd2.text = e.Message;
                return false;
            }
        }
        
        private bool ValidateToken(string str) {
            try {
                return CreateAccountHelper.CheckForgotPasswordToken(str);
            } catch (Exception e) {
                SetTokenInput(Data.ForgotPasswordToken, e.Message, true);
                return false;
            }
        }

        private void ClearErrorTexts() {
            txtErrorPwd1.text = null;
            txtErrorPwd2.text = null;
            txtErrorToken.text = null;
        }

        private void SetTokenInput(string token, string errorMsg, bool visible) {
            inpToken.text = token;
            txtErrorToken.text = errorMsg;
            inpToken.gameObject.SetActive(visible);
            txtErrorToken.gameObject.SetActive(visible);
            _contentSizeFitters.ForEach(e => {
                e.RebuildLayout();
            });
        }
        
        public async void UsedOnlyForAOTCodeGeneration() {
            await StartFlow<bool>(null);
            throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
        }
    }
}