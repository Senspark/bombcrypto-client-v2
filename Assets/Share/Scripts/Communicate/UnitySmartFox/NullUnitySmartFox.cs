using System;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

using Share.Scripts;
using Communicate.Encrypt;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using RsaEncryption = Communicate.Encrypt.RsaEncryption;

namespace Communicate {
    public class NullUnitySmartFox : IUnitySmartFoxCommunication {

        public JwtExtraData GetExtraData() {
            return new JwtExtraData();
        }

        public string Decrypt(string message) {
            return string.Empty;
        }

        public string Encrypt(string message) {
            return string.Empty;
        }

        public string Decrypt(byte[] data) {
            return string.Empty;
        }   

        public string GetJwtForLogin(string extraData) {
            return string.Empty;
        }

        public UniTask Handshake(HandshakeType type) {
            return UniTask.CompletedTask;
        }

        public void RefreshJwt(string jwt, string publicKey) {
        }
    }
}