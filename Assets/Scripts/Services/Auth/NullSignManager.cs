using System.Threading.Tasks;

namespace App {
    public class NullSignManager : ISignManager {
        public Task<string> Sign(string message, string address) {
            throw new System.NotImplementedException();
        }

        public Task<string> ConnectAccount() {
            throw new System.NotImplementedException();
        }

        public Task<bool> IsValidChainId(int chainId) {
            throw new System.NotImplementedException();
        }

        public Task<string> GetSignature(string account) {
            throw new System.NotImplementedException();
        }
    }
}