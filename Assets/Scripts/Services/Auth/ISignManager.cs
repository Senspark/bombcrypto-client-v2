using System.Threading.Tasks;

namespace App {
    public interface ISignManager {
        Task<string> Sign(string message, string address);
        Task<string> ConnectAccount();
        Task<bool> IsValidChainId(int chainId);
        Task<string> GetSignature(string account);
    }
}