using Newtonsoft.Json;

namespace Data {
    public class Fee {
        public class Item {
            [JsonProperty("quantity")]
            public int Quantity;

            [JsonProperty("item_id")]
            public int ItemID;
        }

        [JsonProperty("gold_fee")]
        public int GoldFee;

        [JsonProperty("index")]
        public int Index;

        [JsonProperty("items")]
        public Item[] Items;

        [JsonProperty("gem_fee")]
        public int GemFee;
    }

    public class ConfigUpgradeHeroData {
        [JsonProperty("fee")]
        public Fee[] Fees;

        [JsonProperty("upgrade_type")]
        public string UpgradeType;
    }
}