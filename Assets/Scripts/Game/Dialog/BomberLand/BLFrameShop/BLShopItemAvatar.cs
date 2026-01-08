using Data;
using Engine.Utils;
using Game.Dialog.BomberLand.BLGacha;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopItemAvatar : MonoBehaviour {
        [SerializeField]
        private ImageAnimation avatarTR;

        [SerializeField]
        private Text title;
        
        [SerializeField]
        private OverlayTexture effectPremium;

        [SerializeField]
        private GameObject tagNew;

        [SerializeField]
        private GameObject tagLimited;
        
        [SerializeField]
        private BLGachaRes resource;

        public async void SetData(ProductItemData product) {
            var sprites = await resource.GetAvatar(product.ItemId);
            avatarTR.StartAni(sprites);
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