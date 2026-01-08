using Senspark;

using Newtonsoft.Json;

using Services;

namespace Data {
    public class GachaChestShopItemData {
        public string ProductDescription { get; }
        public int ProductId { get; }
        public string ProductName { get; }

        [JsonConstructor]
        public GachaChestShopItemData(
            [JsonProperty("name")] string name,
            [JsonProperty("product_id")] int productId
        ) {
            ProductDescription = ServiceLocator.Instance.Resolve<IProductItemManager>().GetDescription(productId);
            ProductId = productId;
            ProductName = name;
        }
        
        public GachaChestShopItemData( string name, int productId, string des) {
            ProductId = productId;
            ProductName = name;
            ProductDescription = des;
        }
    }
}