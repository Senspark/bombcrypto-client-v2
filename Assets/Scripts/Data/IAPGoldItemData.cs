using Newtonsoft.Json;

namespace Data {
    public class IAPGoldItemData {
        public int ItemId { get; }
        public string ItemName { get; }
        public int Price { get; }
        public int Quantity { get; }

        [JsonConstructor]
        public IAPGoldItemData(
            [JsonProperty("item_id")] int itemId,
            [JsonProperty("name")] string itemName,
            [JsonProperty("gem_price")] int price,
            [JsonProperty("golds_receive")] int quantity
        ) {
            ItemId = itemId;
            ItemName = itemName;
            Price = price;
            Quantity = quantity;
        }
    }
}