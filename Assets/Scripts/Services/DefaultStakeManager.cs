using System.Threading.Tasks;

namespace App {
    public class DefaultStakeManager : IStakeManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public bool CanStake(double myStaked, double staking) {
            if (myStaked > 0) {
                return staking > 0;
            }
            return staking >= 200f;
        }
    }
}