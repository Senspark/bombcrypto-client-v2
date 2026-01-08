using System;
using App;
using Communicate.Encrypt;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Senspark;
using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;
using Share.Scripts.Communicate.UnitySmartFox.LoginType;
using UnityEngine;

namespace Communicate {
    public class UnityToReact : IUnityToReact {
        public UnityToReact(
            UnityEncryption encryption,
            ILogManager logger,
            IMobileRequest mobileRequest,
            IPublicJwtSession jwtSession
        ) {
            _encryption = encryption;
            _logger = logger;
            _mobileRequest = mobileRequest;
            _jwtSession = jwtSession;
            _reactSender = NewJavascriptProcessor.Instance;
        }

        private readonly ILogManager _logger;
        private readonly IJavascriptProcessor _reactSender;
        private readonly UnityEncryption _encryption;
        private readonly IMobileRequest _mobileRequest;
        private readonly IPublicJwtSession _jwtSession;
        private ILoginWith _loginWith;
        private readonly string _tag = $"[{nameof(UnityToReact)}]";
        
        private string _chainId;
        private UserAccount _userAccount;

        private bool _initialized;

        public async UniTask<bool> RequestConnection() {
            try {
                _logger.Log($"{_tag} RequestConnection start");
                _encryption.Rsa().GenerateKeyPair();
                var result = await _reactSender.CallReact(ReactCommand.GET_CONNECTION, _encryption.Rsa().ExportData());
                bool.TryParse(result, out var connection);
                _logger.Log($"{_tag} RequestConnection done, result: {connection}");
                return connection;
            }
            catch (Exception ex) {
                _logger.Log($"{_tag} RequestConnection error: {ex.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
        }
        
        public async UniTask RequestLoginData() {
            try {
                _logger.Log($"{_tag} RequestLoginData start");
                _encryption.Rsa().GenerateKeyPair();
                var json = await _reactSender.CallReact(ReactCommand.GET_LOGIN_DATA, _encryption.Rsa().ExportData());
                var resultAcc = JsonUtility.FromJson<LoginAccount>(json);
                _userAccount = new UserAccount() {
                    userName = resultAcc.userName,
                    password = resultAcc.password,
                    network = GetSupportedNetwork(resultAcc.network),
                    loginType = LoginType.UsernamePassword
                };
                _logger.Log($"{_tag} RequestLoginData done");
            }
            catch (Exception ex) {
                _logger.Log($"{_tag} RequestConnection error: {ex.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
        }
        
        private NetworkType GetSupportedNetwork(string network) {
            //DevHoang: This must match SupportedNetwork in template
            if (network == "bsc") {
                return NetworkType.Binance;
            }
            if (network == "polygon") {
                return NetworkType.Polygon;
            }
            if (network == "ronin") {
                return NetworkType.Ronin;
            }
            if (network == "base") {
                return NetworkType.Base;
            }
            if (network == "vic") {
                return NetworkType.Viction;
            }
            return NetworkType.Binance;
        }
        
        public async UniTask Handshake() {
            try {
                _logger.Log($"{_tag} Handshake start");
                
                _loginWith = new DefaultLoginWith(_logger, _mobileRequest, _jwtSession);

                //Mobile ko có react template nên bỏ qua bước này
                if (!AppConfig.IsMobile()) {
                    _encryption.Rsa().GenerateKeyPair();
                    var received = await _reactSender.CallReact(ReactCommand.INIT, _encryption.Rsa().ExportData());
                    var aesKey = _encryption.Rsa().ImportData(received);
                    if (string.IsNullOrEmpty(aesKey)) {
                        throw new Exception("Handshake failed");
                    }
                    _encryption.Aes().SetKeyAndIv(aesKey, _encryption.Aes().GetIv());
                }
                
                _initialized = true;
                _logger.Log($"{_tag} Handshake done");    
            }
            catch (Exception ex) {
                _logger.Log($"{_tag} Handshake error: {ex.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
            
        }

        public async UniTask<JwtData> RequestJwt(HandshakeType type) {
            if (!_initialized) {
                throw new Exception("Not initialized");
            }

            if (_loginWith == null) {
                throw new Exception("Login type is not set");
            }

            var data = await _loginWith.GetJwtData(this, type);
            _chainId = data.ExtraData.ChainId;
            return data;
        }

        public async UniTask<string> SendToReact(string cmd, string data) {
            try {
                if (!_initialized) {
                    throw new Exception("Not initialized");
                }

                _logger.Log($"{_tag} SendToReact {cmd} - {data}");
                var encrypted = _encryption.Aes().Encrypt(data);
                var response = await _reactSender.CallReact(cmd, encrypted);
                if (response == null) {
                    _logger.Log($"{_tag} SendToReact: response is null");
                    return string.Empty;
                }
                var decrypted = _encryption.Aes().Decrypt(response);
                return decrypted;
            } catch (Exception e) {
                _logger.Log($"{_tag} SendToReact error: {e.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
            
        }
        
        public async UniTask<string> SendToReactNoEncrypted(string cmd, string data) {
            try {
                _logger.Log($"{_tag} SendToReactNoEncrypted {cmd} - {data}");
                var response = await _reactSender.CallReact(cmd, data);
                if (response == null) {
                    _logger.Log($"{_tag} SendToReactNoEncrypted: response is null");
                    return string.Empty;
                }
                return response;
            } catch (Exception e) {
                _logger.Log($"{_tag} SendToReactNoEncrypted error: {e.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
            
        }

        public UniTask<string> SendToReact(string cmd, JObject data) {
            return SendToReact(cmd, data.ToString());
        }

        public UniTask<string> SendToReact(string cmd) {
            return SendToReact(cmd, string.Empty);
        }

        public UniTask<string> CallBlockChain(string cmd, string data) {
            var dataJson = new JObject {
                ["name"] = cmd,
                ["param"] = data
            };
            return SendToReact(ReactCommand.CALL_BLOCK_CHAIN_METHOD, dataJson.ToString());
        }

        public UniTask<string> CallBlockChain(string cmd) {
            return CallBlockChain(cmd, string.Empty);
        }

        public UniTask<string> CallBlockChain(string cmd, JObject data) {
            return CallBlockChain(cmd, data.ToString());
        }
        
        public UserAccount GetLoginData() {
            return _userAccount;
        }

        public string GetChainId() {
            if (!AppConfig.IsWebGL()) {
                throw new Exception("This feature is only available on WebGL or tournament");
            }
            if (string.IsNullOrEmpty(_chainId)) {
                throw new Exception("ChainId is not available");
            }
            return _chainId;
        }

        
    }
}