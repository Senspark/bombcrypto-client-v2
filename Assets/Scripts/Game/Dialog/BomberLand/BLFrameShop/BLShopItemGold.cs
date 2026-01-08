using System;

using Data;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

using Image = UnityEngine.UI.Image;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopItemGold : MonoBehaviour {
        
        [SerializeField]
        public Image icon;
        
        [SerializeField]
        public Text title;
        
        [SerializeField]
        public Text tGold;
        
        [SerializeField]
        public Text tPrice;
        
        [SerializeField]
        public GameObject blockPrice;
        
        [SerializeField]
        public GameObject blockFree;

        public void SetData(BLShopResource shopResource, IAPGoldItemData d) {
            blockPrice.SetActive(true);
            blockFree.SetActive(false);
            icon.sprite = shopResource.GetImageIpaGold(d.ItemId);
            title.text = d.ItemName;
            tGold.text = $"+{d.Quantity}";
            tPrice.text = $"{d.Price}";
        }
        
        public void SetData(FreeRewardConfig freeRewardConfig) {
            blockPrice.SetActive(false);
            blockFree.SetActive(true);
            title.text = "Free golds";
            if (freeRewardConfig == null) {
                tGold.text = "";
                return;
            }
            tGold.text = $"+{freeRewardConfig.QuantityPerView}";
        }
    }
}