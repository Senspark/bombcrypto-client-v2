using System.Collections.Generic;

using App;

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

        // Native coin only: gem packs have no in-game list currency, the store owns the fiat price. An
        // empty list means the pack cannot be bought in-game on this network.
        public List<ITokenPrice> Prices { get; }

        [JsonConstructor]
        public IAPGemItemData(
            [JsonProperty("product_id")] string productId,
            [JsonProperty("name")] string itemName,
            [JsonProperty("items")] JArray gemReceive,
            [JsonProperty("items_bonus")] JArray gemsBonus,
            [JsonProperty("bonus_type")] int bonusType,
            [JsonProperty("prices")] JArray prices
        ) {
            ProductId = productId;
            ItemName = itemName;
            GemReceive = gemReceive[0].Value<int>("quantity");
            ItemPrice = "";
            GemsBonus = gemsBonus[0].Value<int>("quantity");
            BonusType = bonusType;
            Prices = ParsePrices(prices);
        }

        private static List<ITokenPrice> ParsePrices(JArray prices) {
            var result = new List<ITokenPrice>();
            if (prices == null) {
                return result;
            }
            foreach (var price in prices) {
                result.Add(new TokenPrice(price.Value<int>("reward_type"), price.Value<double>("price")));
            }
            return result;
        }
    }
}
