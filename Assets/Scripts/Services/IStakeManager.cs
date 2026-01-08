using Senspark;

namespace App {
    [Service(nameof(IStakeManager))]
    public interface IStakeManager : IService {
        bool CanStake(double myStaked, double staking);
    }
}