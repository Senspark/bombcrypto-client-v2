using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;

namespace Data {
    public class PvPRankRewardData {
        public readonly int MaxRank;
        public readonly int MinRank;
        public readonly RewardData[] Rewards;

        public PvPRankRewardData(int maxRank, int minRank, RewardData[] rewards) {
            MaxRank = maxRank;
            MinRank = minRank;
            Rewards = rewards;
        }

        [JsonConstructor]
        public PvPRankRewardData(
            [JsonProperty("rank_max")] int maxRank,
            [JsonProperty("rank_min")] int minRank,
            [JsonProperty("reward")] Dictionary<string, int> rewards
        ) : this(
            maxRank,
            minRank,
            rewards.Select(it => new RewardData(it.Value, 0)).ToArray()
        ) {
        }
    }
}