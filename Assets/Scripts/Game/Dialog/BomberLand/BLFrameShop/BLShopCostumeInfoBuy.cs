using System;

using App;

using Data;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopCostumeInfoBuy : MonoBehaviour {
        [SerializeField]
        public Image icon;

        [SerializeField]
        public Text tQuantity;

        [SerializeField]
        public Text tDuration;
        
        private Action _onBuy;

        public int Price { get; private set; }
        public BlockRewardType RewardType { get; private set; }

        public async void SetData(BLGachaRes resource, CostumeData.PriceData priceData) {
            Price = priceData.Price;
            RewardType = RewardUtils.ConvertToBlockRewardType(priceData.RewardType);
            
            tQuantity.text = $"{Price}";
            tDuration.text = priceData.Duration > 0
                ? $"{TimeUtil.ConvertTimeToStringFull(priceData.Duration)}"
                : "Forever";
            var rewardType = priceData.RewardType;
            if (rewardType == "GEM") {
                rewardType = "GEM_LOCKED";
            }
            icon.sprite = await resource.GetSpriteByRewardType(rewardType);
        }

        public void UpdateTotalText(string text) {
            tQuantity.text = text;
        }

        public void SetOnBuy(Action onBuy) {
            _onBuy = onBuy;
        }

        public void OnBtBuyClick() {
            _onBuy?.Invoke();
        }
    }
}