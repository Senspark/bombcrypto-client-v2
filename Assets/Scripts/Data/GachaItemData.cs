using Constant;

using Newtonsoft.Json;

namespace Data {
    public class GachaItemData {
        public string Description { get; }
        public string Name { get; }
        public GachaChestProductId ProductId { get; }

        [JsonConstructor]
        public GachaItemData(
            [JsonProperty("description")] string description,
            [JsonProperty("name")] string name,
            [JsonProperty("product_id")] int productId
        ) {
            Description = description;
            Name = name;
            ProductId = (GachaChestProductId) productId;
        }
    }
}