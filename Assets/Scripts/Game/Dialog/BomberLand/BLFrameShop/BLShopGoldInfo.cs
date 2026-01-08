using System;

using App;

using Data;

using Game.UI.Custom;

using StickWar.Manager;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopGoldInfo : MonoBehaviour {
        [SerializeField]
        public Image icon;

        [SerializeField]
        public Text title;

        [SerializeField]
        public Text tGold;

        [SerializeField]
        public Text tQuantity;

        [SerializeField]
        public Sprite spriteFree;

        [SerializeField]
        public Button btBuy;

        [SerializeField]
        public AdsVideoButton btGetFreeGold;

        [SerializeField]
        public Text tFreeGold;

        [SerializeField]
        private CustomContentSizeFitter priceLayout;

        private Action _onBuy;
        private Action _onGetFreeGold;
        private TimeTick _timeTick = null;
        private DateTime _claimTime;

        public void SetData(BLShopResource shopResource, IAPGoldItemData d) {
            btGetFreeGold.gameObject.SetActive(false);
            btBuy.gameObject.SetActive(true);
            icon.sprite = shopResource.GetImageIpaGold(d.ItemId);
            title.text = d.ItemName;
            tGold.text = $"{d.Price}";
            tQuantity.text = $"+{d.Quantity}";
            priceLayout.AutoLayoutHorizontal();
        }

        public void SetData(FreeRewardConfig freeRewardConfig) {
            btGetFreeGold.gameObject.SetActive(true);
            btBuy.gameObject.SetActive(false);
            icon.sprite = spriteFree;
            title.text = "Free golds";
            if (freeRewardConfig == null) {
                tQuantity.text = "--";
                btGetFreeGold.Interactable = false;
                return;
            }
            tQuantity.text = $"+{freeRewardConfig.QuantityPerView}";
            if (freeRewardConfig.NextTime <= 0) {
                btGetFreeGold.Interactable = true;
                tFreeGold.text = "Watch Ads";
            } else {
                btGetFreeGold.Interactable = false;
                _claimTime = DateTime.UnixEpoch + TimeSpan.FromMilliseconds(freeRewardConfig.NextTime);
                UpdateGui();
                _timeTick = new TimeTick(1, UpdateGui);
            }
            priceLayout.AutoLayoutHorizontal();
        }

        protected void LateUpdate() {
            _timeTick?.Update(Time.deltaTime);
        }

        private void UpdateGui() {
            var duration = _claimTime - DateTime.UtcNow;
            if (duration.Ticks > 0) {
                btGetFreeGold.Interactable = false;
                tFreeGold.text = $"{TimeUtil.ConvertTimeToString(duration)}";
            } else {
                btGetFreeGold.Interactable = true;
                tFreeGold.text = "Watch Ads";
            }
        }

        public void SetOnGetFreeGold(Action onGetFreeGold) {
            _onGetFreeGold = onGetFreeGold;
        }

        public void SetOnBuy(Action onBuy) {
            _onBuy = onBuy;
        }

        public void OnBtBuyClick() {
            _onBuy?.Invoke();
        }

        public void OnBtGetFreeGold() {
            _onGetFreeGold?.Invoke();
        }
    }
}