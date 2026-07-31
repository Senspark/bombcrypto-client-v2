using Data;

using Game.Dialog.BomberLand.BLFrameShop;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.FarmingScene.Scripts {
    // Slot content of DialogGemShop, the gem counterpart of BLShopItemRock. The price is not shown here:
    // it lives on the detail panel, same as the rock grid.
    public class GemShopItem : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text title;

        [SerializeField]
        private Text tGem;

        [SerializeField]
        private GameObject bonusGroup;

        [SerializeField]
        private Text bonusTxt;

        public void SetData(BLShopResource shopResource, IAPGemItemData d) {
            if (icon) icon.sprite = shopResource.GetImageIpaGem(d.ProductId);
            if (title) title.text = d.ItemName;
            // Same string the Adventure gem shop renders, so the two shops read alike.
            if (tGem) {
                tGem.text = d.GemsBonus > 0
                    ? $"{d.GemReceive} <color=#DDF192ff>+{d.GemsBonus}</color>"
                    : $"+{d.GemReceive}";
            }

            var hasBonus = d.BonusType == (int)IAPGemItemData.EBonusType.FirstTimePurchase && d.GemsBonus > 0;
            if (bonusGroup) bonusGroup.SetActive(hasBonus);
            if (bonusTxt && hasBonus) {
                bonusTxt.text = $"+{(int)(100.0f * d.GemsBonus / d.GemReceive)}%";
            }
        }
    }
}
