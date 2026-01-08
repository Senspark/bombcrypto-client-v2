using System;

using App;

using Cysharp.Threading.Tasks;

using Game.Dialog;
using Game.Dialog.BomberLand.BLFrameShop;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    public class DialogConfirmBuyRock : Dialog {
        
        [SerializeField]
        private Image rockImage;

        [SerializeField]
        private Text rockText;

        private ISoundManager _soundManager;
        private Action _onYesBtnClicked;
        private Action _onNoBtnClicked;

        public static UniTask<DialogConfirmBuyRock> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogConfirmBuyRock>();
        }

        private void Start() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
        }

        public DialogConfirmBuyRock SetInfo(BLShopResource shop, IRockPackConfig data, Action onYesBtnClicked, Action onNoBtnClicked) {
            rockImage.sprite = shop.GetImageRock(data.PackageName);
            rockText.text = $"+{data.RockAmount}";
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