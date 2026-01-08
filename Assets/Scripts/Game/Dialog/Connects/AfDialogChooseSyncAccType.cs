using System;

using Senspark;

using Share.Scripts.Dialog;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.Connects {
    public class AfDialogChooseSyncAccType : AfDialog<AccountFlowLoginType> {
        [SerializeField]
        private Button btnSyncWithSenspark;

        [SerializeField]
        private Button btnSyncWithFacebook;

        [SerializeField]
        private Button btnSyncWithGoogle;

        [SerializeField]
        private Button btnSyncWithApple;

        [SerializeField]
        private Button btnClose;

        protected override void Awake() {
            base.Awake();
            btnSyncWithSenspark.onClick.AddListener(OnSyncWithSenspark);
            btnSyncWithFacebook.onClick.AddListener(OnSyncWithFacebook);
            btnSyncWithGoogle.onClick.AddListener(OnSyncWithGoogle);
            btnSyncWithApple.onClick.AddListener(OnSyncWithApple);
            btnClose.onClick.AddListener(OnBtnCloseClicked);
//
//             var isIOs = false;
// #if UNITY_IOS
//             isIOs = true;
// #endif
//             btnSyncWithApple.gameObject.SetActive(isIOs);
        }

        private void OnSyncWithSenspark() {
            PlayClickSound();
            Resolve(AccountFlowLoginType.Senspark);
        }

        private void OnSyncWithFacebook() {
            DialogOK.ShowError(DialogCanvas, "Login failed");
        }

        private void OnSyncWithGoogle() {
            PlayClickSound();
            Resolve(AccountFlowLoginType.Google);
        }

        private void OnSyncWithApple() {
            PlayClickSound();
            Resolve(AccountFlowLoginType.Apple);
        }

        private void OnBtnCloseClicked() {
            PlayClickSound();
            Reject();
        }

        public async void UsedOnlyForAOTCodeGeneration() {
            await StartFlow<AccountFlowLoginType>(null);
            throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
        }
    }
}