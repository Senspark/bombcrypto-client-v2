using System;

using Constant;

using Data;

namespace Services {
    public class PvPRankRewardManager : IPvPRankRewardManager {
        private readonly PvPRankRewardData[] _data;

        private readonly RewardData[] _null = {
            new(0, (int) RewardType.Gem)
        };

        public PvPRankRewardManager(PvPRankRewardData[] data) {
            _data = data;
        }

        public RewardData[] GetRewards(int rank) {
            foreach (var data in _data) {
                if (rank >= data.MinRank && rank <= data.MaxRank) {
                    return data.Rewards;
                }
            }
            return _null;
        }
    }
}