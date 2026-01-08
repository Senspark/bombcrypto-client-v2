using System;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine.Networking;

namespace App {
    public class DefaultClaimManager : IClaimManager {
        private string Host => _isProduction 
            ? "https://claim.bombcrypto.io/" 
            : "";

        private string ClaimCoinHost => $"{Host}claim";

        private string ClaimHeroHost => $"{Host}bhero/claim";

        private string ClaimWithSignHost => $"{Host}claim-with-sign";

        private readonly bool _isProduction;
        private readonly IAccountManager _accountManager;
        private readonly IStorageManager _storeManager;
        private readonly IBlockchainBridge _blockchainBridge;
        private readonly bool _enableClaim;

        public DefaultClaimManager(
            bool production,
            bool enableClaim,
            IAccountManager accountManager,
            IStorageManager storeManager,
            IBlockchainBridge blockchainBridge) {
            _isProduction = production;
            _accountManager = accountManager;
            _storeManager = storeManager;
            _enableClaim = enableClaim;
            _blockchainBridge = blockchainBridge;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task<int> ClaimHero() {
            if (!_enableClaim) {
                return 0;
            }
            var data = await GetRequest(ClaimHeroHost);
            var result = (int) data["amount"];
            return result;
        }

        private async Task<JObject> GetRequest(string host) {
            var account = _accountManager.Account;
            var url = $"{host}/{account}";
            var request = UnityWebRequest.Get(url);
            request.timeout = 30;
            await request.SendWebRequest();
            return ParseData(request);
        }

        private static async Task<JObject> ClaimWithSign(string host, string address, string signature) {
            using var request = new UnityWebRequest(host, UnityWebRequest.kHttpVerbPOST);
            var body = JObject.FromObject(new { address, signature }).ToString(Formatting.None);
            var bodyRaw = Encoding.UTF8.GetBytes(body);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 30;
            await request.SendWebRequest();
            return ParseData(request);
        }
        
        private static JObject ParseData(UnityWebRequest request) {
            if (request.result != UnityWebRequest.Result.Success) {
                throw new Exception(request.error);
            }
            var response = request.downloadHandler.text;
            JObject token;
            try {
                token = JObject.Parse(response);
            } catch (Exception) {
                throw new Exception("Invalid response");
            }
            var message = (string) token["message"];
            if (message != null) {
                throw new Exception(message);
            }
            var result = (JObject) token["data"];
            if (result == null) {
                throw new Exception("Null data");
            }
            return result;
        }
    }
}