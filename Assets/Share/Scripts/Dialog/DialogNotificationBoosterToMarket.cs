using System;

using App;

using Cysharp.Threading.Tasks;

using Game.UI;

using PvpMode.Manager;

using Scenes.MarketplaceScene.Scripts;

using Senspark;

using Services;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Share.Scripts.Dialog {
    public class DialogNotificationBoosterToMarket : Game.Dialog.Dialog {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private BLBoosterResource resource;

        private ISkinManager _skinManager;

        private static UniTask<DialogNotificationBoosterToMarket> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNotificationBoosterToMarket>();
        }

        protected override void Awake() {
            base.Awake();
            _skinManager = ServiceLocator.Instance.Resolve<ISkinManager>();
        }
        
        public static void ShowOn(Canvas canvas, BoosterType boosterType, Action callback = null) {
            Create().ContinueWith(dialog => {
                dialog.SetIconSprite(boosterType);
                dialog.OnDidHide(callback);
                dialog.Show(canvas);
            });
        }

        private void SetIconSprite(BoosterType boosterType) {
            icon.sprite = resource.GetSprite(boosterType);
        }
        
        public void OnButtonVisitMarketClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            MarketplaceScene.LoadScene(BLTabType.Booster);
        }

        private bool _isClicked;
        protected override void OnYesClick() {
            if (_isClicked) {
                return;
            }
            _isClicked = true;
            OnButtonVisitMarketClicked();
        }
    }
}