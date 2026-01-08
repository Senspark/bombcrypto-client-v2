using System.Threading.Tasks;

namespace App {
    public class BomberLandAccountManager : IAccountManager {
        public string Account { get; }
        
        public BomberLandAccountManager(UserAccount acc) {
            Account = acc.walletAddress ?? acc.userName;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}