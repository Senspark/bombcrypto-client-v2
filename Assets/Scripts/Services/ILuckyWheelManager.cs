using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(ILuckyWheelManager))]
    public interface ILuckyWheelManager : IService {
        LuckyWheelRewardData[] GetRewards();
        Task InitializeAsync();
    }
}