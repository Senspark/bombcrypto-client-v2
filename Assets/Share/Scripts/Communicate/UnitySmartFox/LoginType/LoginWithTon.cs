using System;

using App;

using Castle.Core.Internal;

using Communicate;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

using Share.Scripts.Communicate.UnityReact;

namespace Share.Scripts.Communicate.UnitySmartFox.LoginType {
    public class LoginWithTon : ILoginWith {
        private readonly ILogManager _logger;

        public LoginWithTon(ILogManager logger) {
            _logger = logger;
        }

        public async UniTask<JwtData> GetJwtData(IUnityReactSupportMethod unityReact,  HandshakeType type) {
            var data = await unityReact.SendToReact(ReactCommand.GET_JWT_WALLET, string.Empty);
            _logger.Log($"Received JWT for wallet: {data}");

            var jsonObj = JsonConvert.DeserializeObject<CmdDataGetJwt>(data);
            if (jsonObj == null) {
                throw new Exception("Invalid data");
            }
            //Ko có public key do client Ton cũ trước ko có refresh token nên ko gọi api lấy jwt mới đc nên ko có public key => cần connect ví lại
            if (jsonObj.ServerPublicKey.IsNullOrEmpty()) {
                throw new TonOldDataException("Your data is outdated.\n Please connect your wallet again.");
            }
            var extraData = new JwtExtraData(
                true,
                WalletAddress: jsonObj.WalletAddress,
                WalletHex: jsonObj.WalletHex
            );
            return new JwtData(jsonObj.EncryptedJwt, jsonObj.ServerPublicKey, extraData);
        }

        private record CmdDataGetJwt(
            [JsonProperty("walletAddress")]
            string WalletAddress,
            [JsonProperty("encryptedJwt")]
            string EncryptedJwt,
            [JsonProperty("serverPublicKey")]
            string ServerPublicKey,
            [JsonProperty("walletHex")]
            string WalletHex
        );
    }
}