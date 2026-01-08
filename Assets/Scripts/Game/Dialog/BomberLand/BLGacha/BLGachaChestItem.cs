using Constant;

using Data;

using UnityEngine;

namespace Game.Dialog.BomberLand.BLGacha {
    public class BLGachaChestItem : MonoBehaviour {
        public delegate void ClickAction(BLGachaChestItem item, GachaChestProductId productId);

        private ClickAction _clickAction;
        private GachaChestItemData _data;

        public void Initialize(ClickAction clickAction, GachaChestItemData data) {
            _clickAction = clickAction;
            _data = data;
        }

        public void OnPointerClicked() {
            _clickAction(this, _data.ProductId);
        }
    }
}