using App;
using Cysharp.Threading.Tasks;
using Scenes.TreasureModeScene.Scripts.Service;
using Share.Scripts.Communicate.UnityReact;

namespace Communicate {
    public class NullUnityReact : IUnityReactCommunication {
        public IReactToUnity ReactToUnity { get; } = new NullReactToUnity();
        public IUnityToReact UnityToReact { get; } = new NullUnityToReact();

        public UniTask<bool> RequestConnection() {
            return default;
        }
        
        public UniTask RequestLoginData() {
            return UniTask.CompletedTask;
        }
        
        public UniTask Handshake() {
            return UniTask.CompletedTask;
        }
    }
}