using System.Threading.Tasks;

namespace App {
    public class MobileClaimManager : IClaimManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public Task<double> ClaimCoin() {
            return Task.FromResult<double>(0);
        }

        public Task<int> ClaimHero() {
            return Task.FromResult(0);
        }
    }
}