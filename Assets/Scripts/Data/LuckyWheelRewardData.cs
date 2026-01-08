using Newtonsoft.Json;

namespace Data {
    public class LuckyWheelRewardData {
        [JsonProperty("reward_code")]
        public string RewardCode;
        
        // [JsonProperty("block_reward_type")]
        // public string BlockRewardType;
        
        [JsonProperty("quantity")]
        public int Quantity;
        
        [JsonProperty("item_id")]
        public int ItemId;

        [JsonProperty("item_type")]
        public string ItemType;
        
    }
}