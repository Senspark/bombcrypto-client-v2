using System;

using Cysharp.Threading.Tasks;

using UnityEngine.Assertions;

namespace Game.Dialog.Connects {
    public class AfStateFactory : IAccountFlowStateFactory {
        private readonly Func<IAccountFlowState> _guiFactory;
        private readonly AccountFlowData _data;

        public AfStateFactory(
            Func<IAccountFlowState> guiFactory,
            AccountFlowData data
        ) {
            _guiFactory = guiFactory;
            _data = data;
        }

        public UniTask<StateReturnValue<T>> StartFlow<T>() {
            var dialog = _guiFactory() as Dialog;
            Assert.IsNotNull(dialog);
            var ins = (IAccountFlowState)dialog;
            return ins.StartFlow<T>(_data);
        }
    }
}