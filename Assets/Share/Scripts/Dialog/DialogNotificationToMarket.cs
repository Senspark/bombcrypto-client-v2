using App;

using Cysharp.Threading.Tasks;

using Game.UI;

using Scenes.MarketplaceScene.Scripts;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Share.Scripts.Dialog {
    public class DialogNotificationToMarket : Game.Dialog.Dialog {
        
        private static UniTask<DialogNotificationToMarket> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNotificationToMarket>();
        }
        
        public static void ShowOn(Canvas canvas, BLTabType defaultTab) {
            Create().ContinueWith(dialog => {
                dialog.DefaultTab = defaultTab;
                dialog.Show(canvas);
            });
        }

        private BLTabType DefaultTab { get; set; } = BLTabType.Booster;

        public void OnButtonNoClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }
        private bool _isClicked;
        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = false;
            OnButtonVisitMarketClicked();
        }

        public void OnButtonVisitMarketClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            MarketplaceScene.LoadScene(DefaultTab);
        }
    }
}