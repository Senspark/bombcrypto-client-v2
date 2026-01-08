using App;
using Communicate;
using Cysharp.Threading.Tasks;
using Scenes.TreasureModeScene.Scripts.Service;
using Senspark;
using Share.Scripts.Communicate.UnityReact;

namespace Share.Scripts.Communicate {
    [Service(nameof(IMasterUnityCommunication))]
    public interface IMasterUnityCommunication : IService {
        UniTask<bool> RequestConnection();
        UniTask RequestLoginData();
        UniTask Handshake(HandshakeType type = HandshakeType.Login);
        ISmartFoxSupportMethod SmartFox { get; }
        IUnityReactSupportMethod UnityToReact { get; }
        IReactToUnity ReactToUnity { get; }
        IMobileRequest MobileRequest { get; }
        IPublicJwtSession JwtSession { get; }
        void ResetSession();
    }

    public enum HandshakeType {
        Login,
        Reconnect,
    }
}