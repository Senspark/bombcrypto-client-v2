using Constant;

namespace Data {
    public enum ProductItemKind {
        Unknown,
        Mvp,
        Normal,
        Premium,
    }

    public enum ProductTagShop {
        Unknown,
        New,
        Limited
    }

    public class ProductItemData {
        public string[] Abilities;
        public string Name;
        public string Description;
        public int ItemId;
        public int ItemType;
        public ProductItemKind ItemKind;
        public string ItemKindStr;
        public ProductTagShop TagShop;

        public static string GetTrackNameItemType(InventoryItemType type) {
            return type switch {
                InventoryItemType.BombSkin => "bombSkin",
                InventoryItemType.Trail => "trail",
                InventoryItemType.Avatar => "wings",
                InventoryItemType.Hero => "hero",
                InventoryItemType.Booster => "booster",
                InventoryItemType.Misc => "misc",
                InventoryItemType.Reward => "reward",
                InventoryItemType.Fire => "fire",
                InventoryItemType.MysteryBox => "mysterybox",
                InventoryItemType.Altar => "altar",
                InventoryItemType.Emoji => "emoji",
                _ => null // not support
            };
        }

        public static string GetTrackNameProductItem(ProductItemData productItemData) {
            return
                $"{GetTrackNameItemType((InventoryItemType) productItemData.ItemType)}_{productItemData.Name.Replace(" ", "").ToLower()}";
        }
    }
}