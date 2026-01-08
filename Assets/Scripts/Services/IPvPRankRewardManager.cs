using Data;

namespace Services {
    public interface IPvPRankRewardManager {
        RewardData[] GetRewards(int rank);
    }
}