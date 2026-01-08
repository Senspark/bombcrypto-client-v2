using Newtonsoft.Json;

namespace Data {
    public class MarketplaceProductData {
        public int ProductId { get; }
        public int ProductType { get; }
        public PriceData Price { get; }

        [JsonConstructor]
        public MarketplaceProductData([JsonProperty("id")] int id, [JsonProperty("item_id")] int itemId,
            [JsonProperty("price")] float price, [JsonProperty("reward_type")] int rewardType) {
            ProductId = id;
            ProductType = itemId;
            Price = new PriceData(rewardType, price);
        }
    }
}