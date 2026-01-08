using System;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.Connects {
    public class AfDialogSyncAccWarning : AfDialog<bool> {
        [SerializeField]
        private Toggle tggUnderstand;

        [SerializeField]
        private Button btnContinue;

        [SerializeField]
        private Button btnClose;

        protected override void Awake() {
            base.Awake();
            tggUnderstand.onValueChanged.AddListener(OnToggleChanged);
            btnContinue.onClick.AddListener(OnBtnContinueClicked);
            btnClose.onClick.AddListener(OnBtnCloseClicked);
        }

        private void OnBtnContinueClicked() {
            PlayClickSound();
            Resolve(true);
        }

        private void OnToggleChanged(bool val) {
            PlayClickSound();
            btnContinue.interactable = val;
        }

        private void OnBtnCloseClicked() {
            PlayClickSound();
            Reject();
        }
        
        public async void UsedOnlyForAOTCodeGeneration() {
            await StartFlow<bool>(null);
            throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
        }
    }
}