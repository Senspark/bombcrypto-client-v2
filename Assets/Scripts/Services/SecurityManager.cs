using System;
using System.Threading.Tasks;
using Senspark;

namespace App {
    public class SecurityManager : ISecurityManager {
        public bool IsShieldEnabled { get; private set; }
        private IServerManager _serverManager;

        public SecurityManager() {
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
        }

        public Task<bool> Initialize() {
            // Load status initially
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public bool IsTokenLocked(int heroId) {
            // Currently shield is wallet-wide
            return IsShieldEnabled;
        }

        public async Task SetupPin(string pin) {
            // Send setup PIN request to server
            // var result = await _serverManager.Security.SetupPin(pin);
            await Task.Delay(100); // Mock
            IsShieldEnabled = true;
        }

        public async Task<bool> VerifyPinForTransfer(int tokenId, string pin) {
            // Verify PIN via server
            await Task.Delay(100); // Mock
            return true;
        }

        public async Task LockToken(int heroId) {
            await Task.Delay(100);
        }

        public async Task UnlockToken(int heroId) {
            await Task.Delay(100);
        }

        public async Task LockAllTokens() {
            await Task.Delay(100);
        }

        public async Task RequestEmergencyDisable() {
            await Task.Delay(100);
        }

        public async Task CancelEmergency() {
            await Task.Delay(100);
        }
    }
}
