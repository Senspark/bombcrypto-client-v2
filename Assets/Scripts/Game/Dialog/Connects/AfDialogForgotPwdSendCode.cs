using System;

using App;

using Cysharp.Threading.Tasks;

using Senspark;

using Share.Scripts.Dialog;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.Connects {
    public class AfDialogForgotPwdSendCode : AfDialog<string> {
        [SerializeField]
        private TMP_InputField inpEmail;
        
        [SerializeField]
        private TextMeshProUGUI txtError;
        
        [SerializeField]
        private Button btnSendCode;

        [SerializeField]
        private Button btnClose;
        
        private IAuthManager _authManager;

        protected override void Awake() {
            base.Awake();
            btnSendCode.onClick.AddListener(OnBtnSendCodeClicked);
            btnClose.onClick.AddListener(OnBtnCloseClicked);
            
            _authManager = ServiceLocator.Instance.Resolve<IAuthManager>();
            ClearErrorTexts();
        }

        private void OnBtnSendCodeClicked() {
            PlayClickSound();
            ClearErrorTexts();

            var email = inpEmail.text.Trim();
            
            var result = ValidateEmail(email);
            if (!result) {
                return;
            }
            ShowWaiting();
            UniTask.Void(async () => {
                try {
                    await _authManager.ForgotPassword(email);
                    Resolve(email);
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

        private bool ValidateEmail(string str) {
            try {
                return CreateAccountHelper.CheckEmail(str);
            } catch (Exception e) {
                txtError.text = e.Message;
                return false;
            }
        }
        
        private void ClearErrorTexts() {
            txtError.text = null;
        }
    }
}