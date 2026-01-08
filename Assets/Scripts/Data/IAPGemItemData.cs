using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data {
    public class IAPGemItemData {
        public enum EBonusType {
            Non = 0, FirstTimePurchase = 1
        }

        public string ProductId { get; }
        public string ItemName { get; }
        public int GemReceive { get; }
        public string ItemPrice { get; set; }
        public int GemsBonus { get; }
        public int BonusType { get; }

        [JsonConstructor]
        public IAPGemItemData(
            [JsonProperty("product_id")] string productId,
            [JsonProperty("name")] string itemName,
            [JsonProperty("items")] JArray gemReceive,
            [JsonProperty("items_bonus")] JArray gemsBonus,
            [JsonProperty("bonus_type")] int bonusType
        ) {
            ProductId = productId;
            ItemName = itemName;
            GemReceive = gemReceive[0].Value<int>("quantity");
            ItemPrice = "";
            GemsBonus = gemsBonus[0].Value<int>("quantity");
            BonusType = bonusType;
        }
    }
}