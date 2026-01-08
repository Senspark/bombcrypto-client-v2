using System;

using Data;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

using Image = UnityEngine.UI.Image;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopItemChest : MonoBehaviour {
        
        [SerializeField]
        public ChestShopType chestShopType;
        
        [SerializeField]
        public Image icon;
        
        [SerializeField]
        public Text title;
        
        [SerializeField]
        public Text tGold;

        public void SetData(BLShopResource shopResource, GachaChestShopData d) {
            chestShopType = d.ChestType;
            var r = shopResource.GetChestShop(chestShopType);
            icon.sprite = r.sprite;
            title.text = r.name;
        }
    }
}