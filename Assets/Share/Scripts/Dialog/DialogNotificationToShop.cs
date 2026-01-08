using System;

using App;

using Cysharp.Threading.Tasks;

using Scenes.ShopScene.Scripts;

using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;

namespace Share.Scripts.Dialog {
    public class DialogNotificationToShop : Game.Dialog.Dialog {
        public enum Reason {
            NotEnoughGem = 1,
            NotEnoughGold = 2,
            DoNotHaveEquipment = 3, 
            DoNotHaveHeroSoul = 4
        }
        private bool _isClicked;

        
        private static UniTask<DialogNotificationToShop> Create() {
            return ServiceLocator.Instance.Resolve<IPrefabLoaderManager>().Instantiate<DialogNotificationToShop>();
        }

        public static void ShowOn(Canvas canvas, Reason reason, Action callback = null, Action onHide = null) {
            Create().ContinueWith(dialog => {
                dialog.SetReason(reason);
                dialog.OnDidHide(onHide);
                dialog.Show(canvas);
                if (callback != null) {
                    dialog.SetOnButtonVisitMarketClicked(callback);
                }
            });
        }

        [SerializeField]
        public Text description;

        private TypeMenuLeftShop TypeMenuPrefer { get; set; } = TypeMenuLeftShop.Costume;
        private Action _onButtonVisitMarketClicked;

        private void SetReason(Reason reason) {
            switch (reason) {
                case Reason.NotEnoughGem:
                    description.text = "Visit the shop to purchase gem";
                    TypeMenuPrefer = TypeMenuLeftShop.Gems;
                    break;
                case Reason.NotEnoughGold:
                    description.text = "Visit the shop to purchase gold";
                    TypeMenuPrefer = TypeMenuLeftShop.Gold;
                    break;
                case Reason.DoNotHaveEquipment:
                    description.text = "Visit the shop to purchase costumes";
                    TypeMenuPrefer = TypeMenuLeftShop.Costume;
                    break;
                case Reason.DoNotHaveHeroSoul:
                    description.text = "Visit the shop to purchase heroes";
                    TypeMenuPrefer = TypeMenuLeftShop.Hero;
                    break;
                default:
                    break;
            }
        }

        public void SetOnButtonVisitMarketClicked(Action onButtonVisitMarketClicked) {
            _onButtonVisitMarketClicked = onButtonVisitMarketClicked;
        }

        public void OnButtonNoClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            Hide();
        }

        protected override void OnYesClick() {
            if(_isClicked)
                return;
            _isClicked = true;
            OnButtonVisitMarketClicked();
        }
        public void OnButtonVisitMarketClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_onButtonVisitMarketClicked != null) {
                _onButtonVisitMarketClicked.Invoke();
                Hide();
                return;
            }
            ShopScene.LoadScene(TypeMenuPrefer);
        }
    }
}