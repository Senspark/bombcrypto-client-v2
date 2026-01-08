using Data;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopItemGem : MonoBehaviour {
        [SerializeField]
        public Image icon;

        [SerializeField]
        public Text title;

        [SerializeField]
        public Text tGemReceiveGold;

        [SerializeField]
        public Text tPrice;

        [SerializeField]
        public GameObject objPrice;

        [SerializeField]
        public GameObject objBonus;

        [SerializeField]
        public Text titleBonus;

        public void SetData(BLShopResource shopResource, IAPGemItemData d) {
            objPrice.SetActive(true);
            icon.sprite = shopResource.GetImageIpaGem(d.ProductId);
            title.text = d.ItemName;
            tPrice.text = d.ItemPrice;

            if (d.BonusType == (int)IAPGemItemData.EBonusType.FirstTimePurchase && d.GemsBonus > 0) {
                objBonus.SetActive(true);
                var percent = (int)(100.0f * d.GemsBonus / d.GemReceive);
                titleBonus.text = $"+{percent}%";
            } else {
                objBonus.SetActive(false);
                
            }
            tGemReceiveGold.text = d.GemsBonus > 0 ? $"{d.GemReceive} <color=#DDF192ff>+{d.GemsBonus}</color>" : $"+{d.GemReceive}";
        }

        public void SetData(FreeRewardConfig freeRewardConfig) {
            objPrice.SetActive(true);
            title.text = "Free gems";
            tPrice.text = "Free";
            if (freeRewardConfig == null) {
                tGemReceiveGold.text = "";
                return;
            }
            tGemReceiveGold.text = $"+{freeRewardConfig.QuantityPerView}";
            objBonus.SetActive(false);
        }
    }
}