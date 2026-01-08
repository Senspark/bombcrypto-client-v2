using System.Collections.Generic;
using Constant;
using Services;

namespace Data {
    public class GachaChestItemData {
        public string Description { get; }
        public GachaChestProductId ProductId { get; }
        public int Value { get; }
        public ProductItemKind ItemKind { get; }

        public GachaChestItemData(Dictionary<string, int> data, IProductItemManager productItemManager) {
            var productId = data["item_id"];
            Description = productItemManager.GetDescription(productId);
            ProductId = (GachaChestProductId) productId;
            Value = data["quantity"];
            ItemKind = productItemManager.GetItem(productId).ItemKind;
        }

        public GachaChestItemData(int itemId, int quantity, IProductItemManager productItemManager) {
            Description = productItemManager.GetDescription(itemId);
            ProductId = (GachaChestProductId) itemId;
            Value = quantity;
            ItemKind = productItemManager.GetItem(itemId).ItemKind;
        }
    }
}