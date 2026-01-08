using Newtonsoft.Json;

namespace Data {
    public class ConfigUpgradeCrystalData {
        [JsonProperty("source_item_id")]
        public int SourceItemID;

        [JsonProperty("gold_fee")]
        public int GoldFee;

        [JsonProperty("target_item_id")]
        public int TargetItemID;

        [JsonProperty("gem_fee")]
        public int GemFee;
    }
}