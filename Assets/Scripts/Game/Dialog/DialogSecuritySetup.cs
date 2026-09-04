using System;
using App;
using Cysharp.Threading.Tasks;
using Game.Manager;
using Senspark;
using Share.Scripts.Dialog;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class DialogSecuritySetup : Dialog {
        [SerializeField] private InputField pinInput;
        [SerializeField] private InputField pinConfirmInput;
        [SerializeField] private Button btnActivate;
        [SerializeField] private Text txtError;

        private ISoundManager _soundManager;
        private ISecurityManager _securityManager;

        public static DialogSecuritySetup Create() {
            var prefab = Resources.Load<DialogSecuritySetup>("Prefabs/Dialog/DialogSecuritySetup");
            return Instantiate(prefab);
        }

        protected override void Awake() {
            base.Awake();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _securityManager = ServiceLocator.Instance.Resolve<ISecurityManager>();

            btnActivate.onClick.AddListener(OnActivateClicked);
            txtError.gameObject.SetActive(false);
        }

        private void OnActivateClicked() {
            _soundManager.PlaySound(Audio.Tap);
            
            var pin = pinInput.text;
            var confirm = pinConfirmInput.text;

            if (pin.Length != 4) {
                ShowError("PIN must be 4 digits.");
                return;
            }

            if (pin != confirm) {
                ShowError("PINs do not match.");
                return;
            }

            var waiting = new WaitingUiManager(DialogCanvas);
            waiting.Begin();
            UniTask.Void(async () => {
                try {
                    await _securityManager.SetupPin(pin);
                    DialogOK.ShowSuccess(DialogCanvas, "Shield Activated!");
                    Hide();
                } catch (Exception e) {
                    ShowError(e.Message);
                } finally {
                    waiting.End();
                }
            });
        }

        private void ShowError(string message) {
            txtError.text = message;
            txtError.gameObject.SetActive(true);
        }
    }
}
