using Cysharp.Threading.Tasks;

namespace Scenes.ConnectScene.Scripts.Connectors.Mobile {
    public partial class MobileConnectController {
        private interface IState {
            UniTask<IState> GetNextState();
        }
    }
}