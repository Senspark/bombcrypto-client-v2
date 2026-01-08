using System;

using App;

using Data;

using Game.UI.Custom;

using StickWar.Manager;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopGemInfo : MonoBehaviour {
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
        public AdsVideoButton btGetFreeGem;

        [SerializeField]
        public Text tFreeGem;

        [SerializeField]
        private CustomContentSizeFitter priceLayout;

        private Action _onBuy;
        private Action _onGetFreeGem;

        private TimeTick _timeTick = null;
        private DateTime _claimTime;

        public void SetData(BLShopResource shopResource, IAPGemItemData d) {
            icon.sprite = shopResource.GetImageIpaGem(d.ProductId);
            title.text = d.ItemName;
            tGold.text = d.ItemPrice;
            tQuantity.text = d.GemsBonus > 0 ? $"{d.GemReceive} <color=#DDF192ff>+{d.GemsBonus}</color>" : $"+{d.GemReceive}";
            btBuy.gameObject.SetActive(true);
            btGetFreeGem.gameObject.SetActive(false);
            _timeTick = null;
            priceLayout.AutoLayoutHorizontal();
        }

        public void SetData(FreeRewardConfig freeRewardConfig) {
            icon.sprite = spriteFree;
            title.text = "Free gems";
            tGold.text = "";
            btBuy.gameObject.SetActive(false);
            btGetFreeGem.gameObject.SetActive(true);
            _timeTick = null;
            if (freeRewardConfig == null) {
                tQuantity.text = "--";
                tFreeGem.text = "Watch Ads";
                btGetFreeGem.Interactable = false;
                return;
            }
            tQuantity.text = $"+{freeRewardConfig.QuantityPerView}";
            if (freeRewardConfig.NextTime <= 0) {
                btGetFreeGem.Interactable = true;
                tFreeGem.text = "Watch Ads";
            } else {
                btGetFreeGem.Interactable = false;
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
                btGetFreeGem.Interactable = false;
                tFreeGem.text = $"{TimeUtil.ConvertTimeToString(duration)}";
            } else {
                btGetFreeGem.Interactable = true;
                tFreeGem.text = "Watch Ads";
            }
        }

        public void SetCallbacks(Action onBuy) {
            _onBuy = onBuy;
        }

        public void SetOnGetFreeGem(Action onGetFreeGem) {
            _onGetFreeGem = onGetFreeGem;
        }

        public void OnBtBuyClick() {
            _onBuy?.Invoke();
        }

        public void OnBtGetFreeGem() {
            _onGetFreeGem?.Invoke();
        }
    }
}