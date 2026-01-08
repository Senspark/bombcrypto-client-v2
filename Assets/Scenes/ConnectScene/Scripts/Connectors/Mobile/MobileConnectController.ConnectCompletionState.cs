using Cysharp.Threading.Tasks;

namespace Scenes.ConnectScene.Scripts.Connectors.Mobile {
    public partial class MobileConnectController {
        private class ConnectCompletionState : IState {
            public UniTask<IState> GetNextState() {
                return UniTask.FromResult<IState>(this);
            }
        }
    }
}