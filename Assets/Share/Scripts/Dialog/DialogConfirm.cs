using System;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using Share.Scripts.PrefabsManager;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Share.Scripts.Dialog {
    public class DialogConfirm : Game.Dialog.Dialog {
        [SerializeField]
        private Text descriptionTxt;

        [SerializeField]
        private TextMeshProUGUI buttonYesTxt;

        [SerializeField]
        private TextMeshProUGUI buttonNoTxt;

        private ISoundManager _soundManager;
        private Action _onYesBtnClicked;
        private Action _onNoBtnClicked;

        public static async UniTask<DialogConfirm> Create() {
            return await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConfirm>();
        }

        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public DialogConfirm SetInfo(string desc, string yesText, string noText, Action onYesBtnClicked,
            Action onNoBtnClicked) {
            descriptionTxt.text = desc;
            if (!string.IsNullOrWhiteSpace(yesText)) {
                buttonYesTxt.text = yesText;
            }
            if (!string.IsNullOrWhiteSpace(noText)) {
                buttonNoTxt.text = noText;
            }
            _onYesBtnClicked = onYesBtnClicked;
            _onNoBtnClicked = onNoBtnClicked;
            return this;
        }

        protected override void OnYesClick() {
            OnYesBtnClicked();
        }

        protected override void OnNoClick() {
            OnNoBtnClicked();
        }

        public void OnYesBtnClicked() {
            if (IsHiding()) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            _onYesBtnClicked?.Invoke();
            Hide();
        }

        public void OnNoBtnClicked() {
            if (IsHiding()) {
                return;
            }
            _soundManager.PlaySound(Audio.Tap);
            _onNoBtnClicked?.Invoke();
            Hide();
        }
    }
}