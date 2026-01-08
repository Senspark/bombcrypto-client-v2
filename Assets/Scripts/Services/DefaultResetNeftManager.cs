using System.Threading.Tasks;

namespace App {
    public class DefaultResetNeftManager : IResetNeftManager {
        private IChestRewardManager _chestRewardManager;
        
        public DefaultResetNeftManager(IChestRewardManager chestRewardManager) {
            _chestRewardManager = chestRewardManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public bool CanResetNerf(BlockRewardType token, float resetFee) {
            var cond1 = resetFee > 0;
            var cond2 = false;
            if (token == BlockRewardType.BCoin) {
                cond2 = _chestRewardManager.GetBcoinRewardAndDeposit() >= resetFee;
            }
            else if (token == BlockRewardType.Senspark) {
                cond2 = _chestRewardManager.GetSenRewardAndDeposit() >= resetFee;
            }
            return cond1 && cond2;
        }
    }
}