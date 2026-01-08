using System.Threading.Tasks;
using App;
using Communicate;
using Cysharp.Threading.Tasks;
using Scenes.TreasureModeScene.Scripts.Service;
using Share.Scripts.Communicate.UnityReact;

namespace Share.Scripts.Communicate {
    public class NullMasterUnity : IMasterUnityCommunication {
        public NullMasterUnity() {
            var unityReact = new NullUnityReact();
            SmartFox = new NullUnitySmartFox();
            UnityToReact = unityReact.UnityToReact;
            ReactToUnity = unityReact.ReactToUnity;
        }
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
        
        public UniTask<bool> RequestConnection() {
            return default;
        }
        
        public UniTask RequestLoginData() {
            return UniTask.CompletedTask;
        }

        public UniTask Handshake(HandshakeType type) {
            return UniTask.CompletedTask;
        }

        public ISmartFoxSupportMethod SmartFox { get; }

        public IUnityReactSupportMethod UnityToReact { get; }

        public IReactToUnity ReactToUnity { get; }
        public IMobileRequest MobileRequest { get; }
        public IPublicJwtSession JwtSession { get; }

        public void ResetSession() {
            throw new System.NotImplementedException();
        }
    }
}