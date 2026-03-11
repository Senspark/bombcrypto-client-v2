using System;

using App;
using Communicate.Encrypt;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Utils;

namespace Communicate {
    public class EditorToReact : IUnityToReact {

        private const string LocalHost = "http://localhost:8120";
        private JwtExtraData _extraData;
        private readonly IObfuscate _obfuscate;
        private readonly IPublicJwtSession _jwtSession;
        private readonly RsaEncryption _initRsa = new();
        private readonly string _sampleWalletAddress;

        public EditorToReact(IPublicJwtSession jwtSession) {
            _jwtSession = jwtSession;
            _sampleWalletAddress = GetWallet();
            _obfuscate = new AppendBytesObfuscate(Secret.ApiAppendBytes);
        }

        public async UniTask<bool> RequestConnection() {
            return true;
        }
        
        public UniTask RequestLoginData() {
            return UniTask.CompletedTask;
        }
        
        public UniTask Handshake() {
            return UniTask.CompletedTask;
        }

        public async UniTask<JwtData> RequestJwt(HandshakeType type) {
            var userAccount = _jwtSession.Account;
            if (AppConfig.IsMobile()) {
                string url;
                JwtEditor result;

                if(userAccount.loginType is LoginType.UsernamePassword) {
                    url = $"{LocalHost}/mobile/check_proof";
                    var data1 = new {
                        userAccount.userName,
                        userAccount.password
                    };
                    result = await HttpClientHelper.SendPost<JwtEditor>(url, data1);
                } else {
                    url = $"{LocalHost}/mobile/check_proof_guest";
                    var data2 = new {
                        userAccount.userName
                    };
                    result = await HttpClientHelper.SendPost<JwtEditor>(url, data2);
                }
                
                var key = _obfuscate.DeObfuscate(result.ServerPublicKey);
                var extraData = JsonConvert.DeserializeObject<JwtExtraData>(result.extraData);
                _initRsa.GenerateKeyPair();
                _initRsa.ImportPublicKeyBase64(key);
                return new JwtData(result.EncryptedJwt, key, extraData);
            }
            
            //Login with username and password in editor
            if (AppConfig.IsWebGL()) {
                var isAccount = userAccount != null && userAccount.loginType is LoginType.UsernamePassword;
                if (isAccount) {
                    var pathAccount =
                        $"/web{GetEthNetwork()}/editor_get_jwt_account?username={userAccount.userName}&password={userAccount.password}";
                    var url = $"{LocalHost}{pathAccount}";
                    var response = await HttpClientHelper.SendGet<JwtEditor>(url);
                    if (response == null)
                        return null;

                    _initRsa.GenerateKeyPair();
                    _initRsa.ImportPublicKeyBase64(response.ServerPublicKey);
                    var extra = JsonConvert.DeserializeObject<ExtraDataAccount>(response.extraData);
                    _extraData = new JwtExtraData(extra.IsUserFi, extra.Address, userAccount.userName,
                        userAccount.password);
                    return new JwtData(response.EncryptedJwt, response.ServerPublicKey, _extraData);
                } else {
                    var pathWallet = $"/web{GetEthNetwork()}/editor_get_jwt?walletAddress={_sampleWalletAddress}";
                    var url = $"{LocalHost}{pathWallet}";
                    var response = await HttpClientHelper.SendGet<JwtEditor>(url);
                    if (response == null)
                        return null;
            
                    _initRsa.GenerateKeyPair();
                    _initRsa.ImportPublicKeyBase64(response.ServerPublicKey);
                    var chainId = BlockChainNetwork.GetEthChainId(AppConfig.EthNetwork, AppConfig.IsProduction);
                    _extraData = new JwtExtraData(true, WalletAddress: _sampleWalletAddress, ChainId: chainId);
                    return new JwtData(response.EncryptedJwt, response.ServerPublicKey, _extraData);
                }
            }
            
            //Login telegram in editor
            if (AppConfig.IsTon()) {
                var url = $"{LocalHost}/ton/editor_get_jwt?walletAddress=Editor{_sampleWalletAddress}";
                var response = await HttpClientHelper.SendGet<JwtEditor>(url);
                if (response == null)
                    return null;
                
            
                _initRsa.GenerateKeyPair();
                _initRsa.ImportPublicKeyBase64(response.ServerPublicKey);
                _extraData = new JwtExtraData(
                    true,
                    WalletAddress: _sampleWalletAddress,
                    WalletHex: AppConfig.TestWalletTonHex
                    );
                return new JwtData(response.EncryptedJwt, response.ServerPublicKey, _extraData);
            }
                
            //Login solana
            else {
                var url = $"{LocalHost}/sol/editor_get_jwt?walletAddress={_sampleWalletAddress}";
                var response = await HttpClientHelper.SendGet<JwtEditor>(url);
                if (response == null)
                    return null;
            
                _initRsa.GenerateKeyPair();
                _initRsa.ImportPublicKeyBase64(response.ServerPublicKey);
                var chainId = BlockChainNetwork.GetEthChainId(AppConfig.EthNetwork, AppConfig.IsProduction);
                _extraData = new JwtExtraData(true, WalletAddress: _sampleWalletAddress, ChainId: chainId);
                return new JwtData(response.EncryptedJwt, response.ServerPublicKey, _extraData);
            }
            
        }
        
        private static string GetEthNetwork() {
            var network = AppConfig.EthNetwork;
            return network switch {
                EthNetwork.Bsc => "/bsc",
                EthNetwork.Polygon => "/pol",
                EthNetwork.Ronin => "/ron",
                EthNetwork.Base => "/bas",
                _ => throw new ArgumentOutOfRangeException(nameof(GetEthNetwork), network, null)
            };
        }

        public UniTask<string> SendToReact(string cmd, string data) {
            return UniTask.FromResult(string.Empty);
        }

        public UniTask<string> SendToReact(string cmd, JObject data) {
            return UniTask.FromResult(string.Empty);
        }

        public UniTask<string> SendToReact(string cmd) {
            return UniTask.FromResult(string.Empty);
        }
        
        public UniTask<string> SendToReactNoEncrypted(string cmd, string data) {
            return UniTask.FromResult(string.Empty);
        }

        public UniTask<string> CallBlockChain(string cmd, string data) {
            return UniTask.FromResult(string.Empty);
        }

        public UniTask<string> CallBlockChain(string cmd) {
            return UniTask.FromResult(string.Empty);
        }

        public UniTask<string> CallBlockChain(string cmd, JObject data) {
            return UniTask.FromResult(string.Empty);
        }
        
        public UserAccount GetLoginData() {
            return new UserAccount();
        }

        public string GetChainId() {
            if (_extraData == null)
                return BlockChainNetwork.GetEthChainId(EthNetwork.Bsc, false); //bsc testnet

            return _extraData.ChainId;
        }
        
        private string GetWallet() {
            if (AppConfig.IsSolana()) {
                return AppConfig.TestWalletSolana;
            }
            if (AppConfig.IsWebGL()) {
                return AppConfig.EditorAccount;
            }
            if (AppConfig.IsTon()) {
                return AppConfig.WalletTon;
            }
            return AppConfig.EditorAccount;
        }

        private record JwtEditor(
            [JsonProperty("auth")]
            string EncryptedJwt,
            [JsonProperty("key")]
            string ServerPublicKey,
            [JsonProperty("extraData")]
            string extraData
        );
        private class ExtraDataWallet {
            [JsonProperty("version")]
            public string Version { get; set; }
        }
        private class ExtraDataAccount {
            [JsonProperty("version")]
            public string Version { get; set; }
            [JsonProperty("isUserFi")]
            public bool IsUserFi { get; set; }

            [JsonProperty("address")]
            public string Address = null;
        }
        
    }
}