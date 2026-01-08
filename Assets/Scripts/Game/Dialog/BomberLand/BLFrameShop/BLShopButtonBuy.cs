using App;

using Data;

using Game.Dialog.BomberLand.BLGacha;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopButtonBuy : MonoBehaviour {
        [SerializeField]
        private TextMeshProUGUI openText;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI priceText;

        [SerializeField]
        private BLGachaRes resources;

        private GachaChestPrice _price;
        private int _quantity;
        private System.Action<GachaChestPrice, int> _buttonClickedCallback;

        public async void SetInfo(GachaChestPrice price, bool hideOpen, System.Action<GachaChestPrice, int> callback) {
            _price = price;
            _buttonClickedCallback = callback;
            _quantity = price.Quantity;

            openText.gameObject.SetActive(!hideOpen);
            openText.text = $"Open x{price.Quantity}";
            priceText.text = $"{price.Price}";

            var rewardType = price.RewardType;
            if (rewardType == BlockRewardType.Gem) {
                rewardType = BlockRewardType.LockedGem;
            }
            icon.sprite = await resources.GetSpriteByRewardType(rewardType);
        }

        public void SetQuantiy(int quantity) {
            _quantity = quantity;
            priceText.text = $"{_price.Price * quantity}";
        }

        public void OnButtonClicked() {
            _buttonClickedCallback?.Invoke(_price, _quantity);
        }
    }
}