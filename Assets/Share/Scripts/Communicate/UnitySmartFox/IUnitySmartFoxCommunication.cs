using Cysharp.Threading.Tasks;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

namespace Share.Scripts {
    public interface IUnitySmartFoxCommunication : ISmartFoxSupportMethod {
        UniTask Handshake(HandshakeType type = HandshakeType.Login);
    }

    public interface ISmartFoxSupportMethod {
        public string Decrypt(string message);
        public string Encrypt(string message);
        string Decrypt(byte[] data);
        string GetJwtForLogin(string extraData = null);
    }
}