using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;

using Senspark;

using Share.Scripts.PrefabsManager;

using TMPro;

using UnityEngine;

namespace Scenes.FarmingScene.Scripts {
    public class DialogConfirmBurnOrFusion : Dialog {
        [SerializeField]
        private TMP_Text descriptionTxt;
        
        private ISoundManager _soundManager;
        private Action _onYesBtnClicked;
        private Action _onNoBtnClicked;

        public static UniTask<DialogConfirmBurnOrFusion> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConfirmBurnOrFusion>();

        }

        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public DialogConfirmBurnOrFusion SetInfo(int heroFakeCount, Action onYesBtnClicked,
            Action onNoBtnClicked) {
            descriptionTxt.text = $"There are <color=#FFD12F>{heroFakeCount} SHero being staked</color> in\n this Exchange. <color=red>All your stake\n amount will be burn</color> along side\n with the Hero(es) \n\n This action <color=red>cannot be undone</color>\n after burn";
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