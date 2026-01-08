using Cysharp.Threading.Tasks;

namespace Game.Dialog.Connects {
    public interface IAccountFlowController {
        UniTask<bool> SyncGuest();
        UniTask<bool> LoginOldAccount();
        void Destroy();
    }
}