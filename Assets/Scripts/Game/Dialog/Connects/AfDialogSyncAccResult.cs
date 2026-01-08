using System;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.Connects {
    public class AfDialogSyncAccResult : AfDialog<bool> {
        [SerializeField]
        private Button btnOk;

        protected override void Awake() {
            base.Awake();
            IgnoreOutsideClick = true;
            btnOk.onClick.AddListener(OnBtnOkClicked);
        }

        private void OnBtnOkClicked() {
            PlayClickSound();
            Resolve(true);
        }
        
        public async void UsedOnlyForAOTCodeGeneration() {
            await StartFlow<bool>(null);
            throw new InvalidOperationException("This method is used for AOT code generation only. Do not call it at runtime.");
        }
    }
}