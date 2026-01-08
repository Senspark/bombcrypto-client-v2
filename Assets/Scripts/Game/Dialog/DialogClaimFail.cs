using System;

using App;

using Senspark;

using UnityEngine;

namespace Game.Dialog {
    public class DialogClaimFail : Dialog {
        [SerializeField]
        private GameObject successfulPurchase;

        [SerializeField]
        private GameObject claimHeroesFail;

        [SerializeField]
        private GameObject claimBcoinFail;
        
        [SerializeField]
        private GameObject claimDepositFail;
        
        [SerializeField]
        private GameObject claimSensparkFail;

        private DialogType _dialogType;

        public static DialogClaimFail Create(DialogType type) {
            var prefab = Resources.Load<DialogClaimFail>("Prefabs/Dialog/DialogClaimFail");
            var ins = Instantiate(prefab);
            ins.SetDialogType(type);
            return ins;
        }

        public void OnOkBtnClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

        public void OnReportLinkClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            var url = _dialogType switch {
                DialogType.ClaimBcoinFail => "https://report.bombcrypto.io/",
                DialogType.ClaimDepositFail => "https://report.bombcrypto.io/bcoin-deposited",
                DialogType.ClaimSensparkFail => "https://report.bombcrypto.io/senspark",
                _ => string.Empty
            };
            if(!string.IsNullOrWhiteSpace(url)) {
                Application.OpenURL(url);
            }
        }

        private void SetDialogType(DialogType type) {
            _dialogType = type;
            successfulPurchase.gameObject.SetActive(false);
            claimHeroesFail.gameObject.SetActive(false);
            claimBcoinFail.gameObject.SetActive(false);
            claimDepositFail.gameObject.SetActive(false);
            claimSensparkFail.gameObject.SetActive(false);

            switch (type) {
                case DialogType.SuccessfulPurchase:
                    successfulPurchase.gameObject.SetActive(true);
                    break;
                case DialogType.ClaimHeroesFail:
                    claimHeroesFail.gameObject.SetActive(true);
                    break;
                case DialogType.ClaimBcoinFail:
                    claimBcoinFail.gameObject.SetActive(true);
                    break;
                case DialogType.ClaimDepositFail:
                    claimDepositFail.gameObject.SetActive(true);
                    break;
                case DialogType.ClaimSensparkFail:
                    claimSensparkFail.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        public enum DialogType {
            SuccessfulPurchase,
            ClaimHeroesFail,
            ClaimBcoinFail,
            ClaimDepositFail,
            ClaimSensparkFail
        }
    }
}