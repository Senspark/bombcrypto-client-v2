using App;
using Communicate;
using Cysharp.Threading.Tasks;
using Scenes.TreasureModeScene.Scripts.Service;
using Senspark;
using UnityEngine;

namespace Share.Scripts.Communicate.UnityReact {
    /// <summary>
    /// React to unity: Unity lắng nghe các event đc chủ động gọi từ phía react
    /// Unity to react: Unity chủ động gọi các method của react
    /// </summary>
    public interface IUnityReactCommunication {
        IReactToUnity ReactToUnity { get; }
        IUnityToReact UnityToReact { get; }
        UniTask<bool> RequestConnection();
        UniTask RequestLoginData();
        UniTask Handshake();
    }
    
    public class UnityReactCommunication: IUnityReactCommunication {
        public UnityReactCommunication(ILogManager logManager, IMobileRequest mobileRequest, IPublicJwtSession jwtSession) {
            ReactToUnity = new ReactToUnity(logManager);
            UnityToReact = AppConfig.IsEditor && !AppConfig.IsMobile()
                ? new EditorToReact(jwtSession)
                : new UnityToReact(logManager, mobileRequest, jwtSession);
        }

        public IReactToUnity ReactToUnity { get; }
        public IUnityToReact UnityToReact { get; }
        public async UniTask<bool> RequestConnection() {
            return await UnityToReact.RequestConnection();
        }
        
        public UniTask RequestLoginData() {
            return UnityToReact.RequestLoginData();
        }
        
        public UniTask Handshake() {
            return UnityToReact.Handshake();
        }
    }
    
    
}