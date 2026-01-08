using Cysharp.Threading.Tasks;

namespace Game.Dialog.Connects {
    public interface IAccountFlowState {
        UniTask<StateReturnValue<T>> StartFlow<T>(AccountFlowData data);
    }

    public interface IAccountFlowStateFactory {
        UniTask<StateReturnValue<T>> StartFlow<T>();
    }
}