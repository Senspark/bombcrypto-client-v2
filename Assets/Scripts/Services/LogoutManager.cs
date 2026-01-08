using System.Threading.Tasks;

namespace Services {
    public class LogoutManager : ILogoutManager {
        private readonly IInventoryManager _inventoryManager;

        public LogoutManager(IInventoryManager inventoryManager) {
            _inventoryManager = inventoryManager;
        }
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void Logout() {
            _inventoryManager.Clear();
        }
    }
}