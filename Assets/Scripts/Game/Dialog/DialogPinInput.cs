using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Manager;
using Senspark;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogPinInput : Dialog {
        [SerializeField] private InputField pinInput;
        [SerializeField] private Button btnConfirm;
        [SerializeField] private Button btnCancel;
        [SerializeField] private Text txtError;
        [SerializeField] private Text txtAttempts;

        private ISoundManager _soundManager;
        private ISecurityManager _securityManager;
        
        private TaskCompletionSource<string> _tcs;

        public static DialogPinInput Create() {
            var prefab = Resources.Load<DialogPinInput>("Prefabs/Dialog/DialogPinInput");
            return Instantiate(prefab);
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _securityManager = ServiceLocator.Instance.Resolve<ISecurityManager>();

            btnConfirm.onClick.AddListener(OnConfirmClicked);
            btnCancel.onClick.AddListener(OnCancelClicked);
            txtError.gameObject.SetActive(false);
            txtAttempts.gameObject.SetActive(false);
        }

        public async Task<string> RequestPinAsync() {
            _tcs = new TaskCompletionSource<string>();
            return await _tcs.Task;
        }

        private void OnConfirmClicked() {
            _soundManager.PlaySound(Audio.Tap);
            
            var pin = pinInput.text;
            if (pin.Length != 4) {
                ShowError("PIN must be 4 digits.");
                return;
            }

            _tcs.TrySetResult(pin);
            Hide();
        }

        private void OnCancelClicked() {
            _soundManager.PlaySound(Audio.Tap);
            _tcs.TrySetCanceled();
            Hide();
        }

        private void ShowError(string message) {
            txtError.text = message;
            txtError.gameObject.SetActive(true);
        }
    }
}
