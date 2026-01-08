using System;

using Data;

using Game.UI.Custom;

using Scenes.MarketplaceScene.Scripts;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLInventoryItemOnSell : BaseBLItem {
        [SerializeField]
        private Text nameText;

        [SerializeField]
        private TMP_Text durationText;
        
        [SerializeField]
        private CustomContentSizeFitter priceLayout;
        
        
        public override void SetInfo<T>(int index, T itemData, Action<int> callback) {
            Index = index;
            OnClickCallback = callback;
            avatar.ChangeAvatar(itemData);
            durationText.gameObject.SetActive(false);

            switch (itemData) {
                case UIHeroData hero:
                    valueText.text = $"x{hero.Quantity}";
                    break;
                case ItemData item:
                    avatar.ChangeAvatar(item);
                    valueText.text = $"x{item.Quantity}";
                    nameText.text = App.Utils.FormatBcoinValue(item.UnitPrice);
                    if(item.ExpirationAfter > 0) {
                        durationText.gameObject.SetActive(true);
                        durationText.text = $"Duration: <color=#33ff00>{MarketUtils.ExpirationToDays(item.ExpirationAfter)} days</color>";
                    }
                    break;
                case InventoryChestData chest:
                    avatar.ChangeAvatar(chest);
                    nameText.text = $"{chest.ChestName}";
                    break;
            }
            priceLayout.AutoLayoutHorizontal();
        }
    }
}