using Senspark;

namespace App {
    [Service(nameof(IResetNeftManager))]
    public interface IResetNeftManager {
        bool CanResetNerf(BlockRewardType token, float resetFee);
    }
}