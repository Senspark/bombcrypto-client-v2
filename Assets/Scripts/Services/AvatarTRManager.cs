using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Senspark;

namespace Services {
    public class AvatarTRManager : IAvatarTRManager {
        private int _curAvatarId = -1;

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async UniTask<bool> Init() {
            var inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _curAvatarId = await inventoryManager.GetCurrentAvatarTR();
            var initStatus = await Initialize();
            return initStatus;
        }

        public void SetCurrentAvatarId(int avatarId) {
            _curAvatarId = avatarId;
        }

        public int GetCurrentAvatarId() {
            return _curAvatarId;
        }
    }
}