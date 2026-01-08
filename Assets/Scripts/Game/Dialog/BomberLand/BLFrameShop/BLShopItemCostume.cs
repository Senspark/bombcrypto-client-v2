using Data;

using Game.Dialog.BomberLand.BLGacha;

using UnityEngine;
using UnityEngine.UI;

using Image = UnityEngine.UI.Image;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopItemCostume : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text title;
        
        [SerializeField]
        private OverlayTexture effectPremium;

        [SerializeField]
        private GameObject tagNew;

        [SerializeField]
        private GameObject tagLimited;

        public async void SetData(BLGachaRes resource, ProductItemData product, CostumeData costume) {
            icon.sprite = await resource.GetSpriteByItemId(product.ItemId);
            title.text = product.Name;
            // effectPremium.enabled = product.ItemKind == ProductItemKind.Premium;
            effectPremium.enabled = false;
            title.color = product.ItemKind == ProductItemKind.Premium
                ? effectPremium.m_OverlayColor : Color.white;
            
            switch (product.TagShop)
            {
                case ProductTagShop.New:
                    tagNew.SetActive(true);
                    tagLimited.SetActive(false);
                    break;
                case ProductTagShop.Limited:
                    tagNew.SetActive(false);
                    tagLimited.SetActive(true);
                    break;
                case ProductTagShop.Unknown:
                default:
                    tagNew.SetActive(false);
                    tagLimited.SetActive(false);
                    break;
            }
        }
    }
}