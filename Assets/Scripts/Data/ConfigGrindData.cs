using Newtonsoft.Json;

namespace Data {
    public class ConfigGrindData {
        [JsonProperty("reward_type")]
        public string RewardType;

        [JsonProperty("price")]
        public int Price;

        [JsonProperty("item_kind")]
        public string ItemKind;
    }
}