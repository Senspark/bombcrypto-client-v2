using App;
using Communicate;
using Communicate.Encrypt;
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
        public UnityReactCommunication(UnityEncryption encryption, ILogManager logManager, IMobileRequest mobileRequest, IPublicJwtSession jwtSession) {
            ReactToUnity = new ReactToUnity(encryption, logManager);
            UnityToReact = Application.isEditor && !AppConfig.IsMobile()
                ? new EditorToReact(GetWallet(), jwtSession)
                : new UnityToReact(encryption, logManager, mobileRequest, jwtSession);
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
        
        private string GetWallet() {
            if (AppConfig.IsSolana()) {
                return AppConfig.NewTestWallet;
            }
            if (AppConfig.IsWebGL()) {
                return AppConfig.GetTestWalletInfo().jwt;
            }
            if (AppConfig.IsTon()) {
                return AppConfig.WalletTon;
            }
            return AppConfig.NewTestWallet;
        }
    }
    
    
}