using App;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

namespace Communicate {
    public class NullUnityToReact: IUnityToReact {
        public UniTask Handshake(ILoginWith loginWith) {
            return UniTask.CompletedTask;
        }
        
        public UniTask<bool> RequestConnection() {
            return default;
        }
        
        public UniTask RequestLoginData() {
            return UniTask.CompletedTask;
        }

        public UniTask Handshake() {
            return UniTask.CompletedTask;
        }

        public UniTask<JwtData> RequestJwt(HandshakeType type) {
            return default;
        }
        
        public UserAccount GetLoginData() {
            return default;
        }

        public string GetChainId() {
            return default;
        }

        public UniTask<string> SendToReact(string cmd, string data) {
            return default;
        }

        public UniTask<string> SendToReact(string cmd, JObject data) {
            return default;
        }

        public UniTask<string> SendToReact(string cmd) {
            return default;
        }
        
        public UniTask<string> SendToReactNoEncrypted(string cmd, string data) {
            return default;
        }

        public UniTask<string> CallBlockChain(string cmd, string data) {
            return default;
        }

        public UniTask<string> CallBlockChain(string cmd) {
            return default;
        }

        public UniTask<string> CallBlockChain(string cmd, JObject data) {
            return default;
        }
    }
}