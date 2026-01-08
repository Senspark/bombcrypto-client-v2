using Newtonsoft.Json;

namespace Data {
    public class PriceData {
        public int Type { get; }
        public float Value { get; }

        public PriceData(int type, float value) {
            Type = type;
            Value = value;
        }
    }

    public class PriceShopData {
        [JsonProperty("duration")]
        public long Duration { get; set; }
        [JsonProperty("package")]
        public string Package { get; set; }
        [JsonProperty("reward_type")]
        public string RewardType { get; set; }
        [JsonProperty("price")]
        public int Price { get; set; }
    }
}