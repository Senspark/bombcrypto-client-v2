using Newtonsoft.Json;

namespace Data {
    public class InventorySellingItemData {
        public int Id { get; }
        public int ItemId { get; }
        public PriceData Price { get; }
        public int Quantity { get; }
        public float UnitPrice { get; }
        public int ExpirationAfter { get; }

        [JsonConstructor]
        public InventorySellingItemData(
            [JsonProperty("quantity")] int quantity,
            [JsonProperty("item_id")] int itemId,
            [JsonProperty("reward_type")] int rewardType,
            [JsonProperty("id")] int id,
            [JsonProperty("price")] float unitPrice,
            [JsonProperty("expiration_after")] int expirationAfter
        ) {
            Quantity = quantity;
            ItemId = itemId;
            Price = new PriceData(
                rewardType,
                unitPrice
            );
            Id = id;
            UnitPrice = unitPrice;
            ExpirationAfter = expirationAfter;
        }
    }
}