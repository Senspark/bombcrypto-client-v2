using System.Threading.Tasks;

using Senspark;

namespace Services {
    [Service(nameof(IOfflineRewardManager))]
    public interface IOfflineRewardManager : IService {
        bool HadRewards { get; }
        long GetLastLogout();
        (int ItemId, int Quantity)[] GetRewards();
        Task InitializeAsync();
        void ResetRewards();
    }
}