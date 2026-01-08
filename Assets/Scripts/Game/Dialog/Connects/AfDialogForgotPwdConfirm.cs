using System;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Services.DeepLink;

using Share.Scripts.Dialog;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.Connects {
    public class AfDialogForgotPwdConfirm : AfDialog<string> {
        [SerializeField]
        private TMP_InputField inpCode;

        [SerializeField]
        private TextMeshProUGUI txtError;
        
        [SerializeField]
        private TextMeshProUGUI txtResendCode;

        [SerializeField]
        private Button btnConfirm;
        
        [SerializeField]
        private Button btnResendCode;
        
        [SerializeField]
        private Button btnClose;
        
        [SerializeField]
        private int resendCodeInterval = 30;

        private IDeepLinkListener _deepLinkListener;
        private IAuthManager _authManager;
        
        private ObserverHandle _handle;
        private DateTime _lastResendCodeTime = DateTime.Now;

        protected override void Awake() {
            base.Awake();
            btnConfirm.onClick.AddListener(OnBtnConfirmClicked);
            btnResendCode.onClick.AddListener(OnBtnResendCodeClicked);
            btnClose.onClick.AddListener(OnBtnCloseClicked);

            _authManager = ServiceLocator.Instance.Resolve<IAuthManager>();
            _deepLinkListener = ServiceLocator.Instance.Resolve<IDeepLinkListener>();
            
            _handle = new ObserverHandle();
            _handle.AddObserver(_deepLinkListener, new DeepLinkObserver {
                OnDeepLinkReceived = OnDeepLinkReceived
            });
            
            InvokeRepeating(nameof(OnUpdateTime), 0, 1);
            inpCode.characterLimit = CreateAccountHelper.ForgotPwdTokenLength;
            ClearErrorTexts();
            IgnoreOutsideClick = true;
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _handle.Dispose();
        }

        private void OnDeepLinkReceived() {
            var data = _deepLinkListener.GetDeepLinkData();
            if (data.TryGetValue(DeepLinkKey.Token, out var token)) {
                if (!string.IsNullOrWhiteSpace(token) && token.Length == CreateAccountHelper.ForgotPwdTokenLength) {
                    inpCode.text = token;
                }
            }
        }

        private void OnUpdateTime() {
            var timePassed = DateTime.Now - _lastResendCodeTime;
            var secondsPassed = resendCodeInterval - timePassed.TotalSeconds;
            var canResend = secondsPassed <= 0;
            var str = canResend ? "Resend code" : $"Wait {secondsPassed:0}s To Resend";
            txtResendCode.text = str;
            txtResendCode.color = Color.white;
            btnResendCode.interactable = canResend;
        }

        private void OnBtnConfirmClicked() {
            PlayClickSound();
            ClearErrorTexts();

            var token = inpCode.text.Trim();
            var result = ValidateToken(token);
            if (!result) {
                return;
            }
            Resolve(token);
        }
        
        private void OnBtnResendCodeClicked() {
            PlayClickSound();
            ClearErrorTexts();

            var email = Data.PendingForgotPasswordEmail;
            var result = ValidateEmail(email);
            if (!result) {
                Reject();
                return;
            }
            ShowWaiting();
            UniTask.Void(async () => {
                try {
                    await _authManager.ForgotPassword(email);
                    txtResendCode.text = "Resend successfully";
                    txtResendCode.color = Color.green;
                    _lastResendCodeTime = DateTime.Now;
                } catch (Exception e) {
                    DialogOK.ShowError(Data.CurrentCanvas, e.Message);
                } finally {
                    HideWaiting();
                }
            });
        }
        
        private void OnBtnCloseClicked() {
            PlayClickSound();
            Reject();            
        }
        
        private void ClearErrorTexts() {
            txtError.text = null;
        }

        private bool ValidateToken(string str) {
            try {
                return CreateAccountHelper.CheckForgotPasswordToken(str);
            } catch (Exception e) {
                txtError.text = e.Message;
                return false;
            }
        }
        
        private bool ValidateEmail(string str) {
            try {
                return CreateAccountHelper.CheckEmail(str);
            } catch (Exception e) {
                return false;
            }
        }
    }
}