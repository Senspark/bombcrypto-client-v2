using Newtonsoft.Json;

namespace Data {
    public class FreeRewardConfig {
        public string RewardType { get; }
        public int QuantityPerView { get; }
        public long NextTime { get; }

        [JsonConstructor]
        public FreeRewardConfig(
            [JsonProperty("reward_type")] string rewardType,
            [JsonProperty("quantity_per_view")] int quantityPerView,
            [JsonProperty("next_time")] long nextTime
        ) {
            RewardType = rewardType;
            QuantityPerView = quantityPerView;
            NextTime = nextTime;
        }
    }
}