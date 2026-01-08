using App;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Share.Scripts.Communicate.UnityReact {
    public interface IUnityToReact : IUnityToReactLocalMethod, IUnityReactSupportMethod {}

    public record JwtData(
        string Jwt,
        string ServerPublicKey,
        JwtExtraData ExtraData = null
    );

    public record JwtExtraData(
        [JsonProperty("isUserFi")]
        bool IsUserFi = false,
        [JsonProperty("address")]
        string WalletAddress = null,
        [JsonProperty("userName")]
        string Username = null,
        [JsonProperty("password")]
        string Password = null,
        [JsonProperty("chainId")]
        string ChainId = null,
        [JsonProperty("walletHex")]
        string WalletHex = null,
        [JsonProperty("uid")]
        int Uid = 0
        );
    
    public interface IUnityToReactLocalMethod {
        UniTask<bool> RequestConnection();
        UniTask RequestLoginData();
        UniTask Handshake();
        UniTask<JwtData> RequestJwt(HandshakeType type);
    }
    
    public interface IUnityReactSupportMethod {
        UserAccount GetLoginData();
        string GetChainId();
        UniTask<string> SendToReact(string cmd, string data);
        UniTask<string> SendToReact(string cmd, JObject data);
        UniTask<string> SendToReact(string cmd);
        UniTask<string> SendToReactNoEncrypted(string cmd, string data);
        UniTask<string> CallBlockChain(string cmd, string data);
        UniTask<string> CallBlockChain(string cmd);
        UniTask<string> CallBlockChain(string cmd, JObject data);
    }
    
}