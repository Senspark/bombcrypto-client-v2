using Animation;

using Constant;

using Data;

using Engine.Entities;

using Game.Dialog.BomberLand.BLGacha;

using Senspark;

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
            icon.sprite = await GetIconSprite(resource, product);
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

        // Hero icon đã decouple sang IHeroSpriteLoader (path-load). Dict cũ trong ResourceChestSo trỏ tới
        // sprite hero đã bị gộp/xoá → trắng. Hero → LoadPortrait; item khác (bomb/trail/avatar/emoji) giữ dict.
        private static async Cysharp.Threading.Tasks.UniTask<Sprite> GetIconSprite(
            BLGachaRes resource, ProductItemData product) {
            if (product.ItemType == (int) InventoryItemType.Hero) {
                var playerType = UIHeroData.ConvertFromHeroId(product.ItemId);
                if (HeroSpriteCatalog.Has(playerType)) {
                    var loader = ServiceLocator.Instance.Resolve<IHeroSpriteLoader>();
                    return await loader.LoadPortrait(playerType, PlayerColor.HeroTr);
                }
            }
            return await resource.GetSpriteByItemId(product.ItemId);
        }
    }
}