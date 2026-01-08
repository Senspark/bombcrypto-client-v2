using System;

using Data;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopItemSubscription : MonoBehaviour {
        [SerializeField]
        private Text title;
        
        [SerializeField]
        public Image icon;

        [SerializeField]
        public Text priceText;
        
        [SerializeField]
        private Button selectPlanButton;

        [SerializeField]
        private Text currentPlan;

        [SerializeField]
        private Button unsubscribeButton;

        private Action _subscribeCallback;
        private Action _unSubscribeCallback;

        public void SetData(BLShopResource shopResource, IAPSubscriptionItemData data) {
            icon.sprite = shopResource.GetImageIpaProduct(data.ProductId);
            title.text = data.ItemName;
            priceText.text = $"{data.ItemPrice}/Month";
            
            var notSubscribed = data.UserPackage is not {State: SubscriptionState.Active};
            selectPlanButton.gameObject.SetActive(notSubscribed);
            currentPlan.gameObject.SetActive(!notSubscribed);
            unsubscribeButton.gameObject.SetActive(!notSubscribed);
        }

        public void SetCallbacks(Action onSubscribe, Action onUnsubscribe) {
            _subscribeCallback = onSubscribe;
            _unSubscribeCallback = onUnsubscribe;
        }
        
        public void OnSelectPlanButtonClicked() {
            _subscribeCallback.Invoke();
        }

        public void OnUnsubscriptionButtonClicked() {
            _unSubscribeCallback.Invoke();
        }
    }
}