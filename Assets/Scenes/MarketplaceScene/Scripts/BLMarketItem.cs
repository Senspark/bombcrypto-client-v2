using System;
using TMPro;
using UnityEngine;

namespace Game.Dialog {
    public class BLMarketItem: BaseBLItem {
        [SerializeField]
        private TextMeshProUGUI itemPriceTxt;

        private const string GEM_ICON = "<sprite index=0>";
        
        public override void SetInfo<T>(int index, T itemData, Action<int> callback) {
            Index = index;
            OnClickCallback = callback;
            
            switch (itemData) {
                case UIHeroData hero:
                    avatar.ChangeAvatar(itemData);
                    itemId = hero.HeroData.DataBase.ItemId;
                    itemPriceTxt.text = $"{GEM_ICON}--";
                    break;
                case ItemData item:
                    avatar.ChangeAvatar(item);
                    itemId = item.ItemId;
                    itemPriceTxt.text = $"{GEM_ICON}--";
                    break;
            }
        }
        public override void UpdateMinPrice(float minPrice) {
            itemPriceTxt.text = $"{GEM_ICON}{minPrice}";
        }
        
    }
}