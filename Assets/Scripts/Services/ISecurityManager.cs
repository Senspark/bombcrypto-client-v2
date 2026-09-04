using System.Threading.Tasks;
using Senspark;

namespace App {
    [Service(nameof(ISecurityManager))]
    public interface ISecurityManager : IService {
        bool IsShieldEnabled { get; }
        bool IsTokenLocked(int heroId);
        Task SetupPin(string pin);
        Task<bool> VerifyPinForTransfer(int tokenId, string pin);
        Task LockToken(int heroId);
        Task UnlockToken(int heroId);
        Task LockAllTokens();
        Task RequestEmergencyDisable();
        Task CancelEmergency();
    }
}
