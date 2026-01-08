using System;
using System.Threading.Tasks;

using App;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

using Share.Scripts;

using Communicate.Encrypt;

using Share.Scripts.Communicate;
using Share.Scripts.Communicate.UnityReact;

using UnityEngine;

using RsaEncryption = Communicate.Encrypt.RsaEncryption;

namespace Communicate {
    public class UnitySmartFoxCommunication : IUnitySmartFoxCommunication {
        public UnitySmartFoxCommunication(
            UnityEncryption encryption,
            IUnityReactCommunication unityToReact,
            IJwtSession jwtSession,
            IObfuscate obfuscate,
            ILogManager logger
        ) {
            _unityToReactCommunicate = unityToReact;
            _encryption = encryption;
            _obfuscate = obfuscate;
            _jwtSession = jwtSession;
            _logger = logger;
        }

        private readonly ILogManager _logger;
        private readonly IUnityReactCommunication _unityToReactCommunicate;
        private readonly UnityEncryption _encryption;
        private readonly IObfuscate _obfuscate;
        private readonly IJwtSession _jwtSession;
        private readonly string _tag = $"[{nameof(UnitySmartFoxCommunication)}]";
        

        public async UniTask Handshake(HandshakeType type) {
            _logger.Log($"{_tag} Start request jwt from react");

            var data = await _unityToReactCommunicate.UnityToReact.RequestJwt(type);

            _encryption.Rsa().GenerateKeyPair();
            _encryption.Rsa().ImportPublicKeyBase64(data.ServerPublicKey);
            _jwtSession.SetRawJwt(data.Jwt);
            _jwtSession.SetPublicKey(data.ServerPublicKey);
            _jwtSession.SetExtraData(data.ExtraData);
        }

        public string Encrypt(string message) {
            try {
                return _encryption.Aes().Encrypt(message);
            } catch (Exception e) {
                _logger.Log($"{_tag} EncryptDataForSmartFox error: {e.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
        }

        public JwtExtraData GetExtraData() {
            if (_jwtSession.ExtraData == null) {
                _logger.Log($"{_tag} GetExtraData error: extraData is null");
                return new JwtExtraData();
            }
            return _jwtSession.ExtraData;
        }

        public string Decrypt(string message) {
            try {
                return _encryption.Aes().Decrypt(message);
            } catch (Exception e) {
                _logger.Log($"{_tag} DecryptDataFromSmartFox error: {e.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
        }

        public string Decrypt(byte[] data) {
            try {
                return _encryption.Aes().Decrypt(data);
            } catch (Exception e) {
                _logger.Log($"{_tag} DecryptDataFromSmartFox error: {e.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
        }

        public string GetJwtForLogin(string extraData) {
            try {
                _encryption.Aes().GenerateKey();
                var aesKey = _encryption.Aes().GetKey();
                var corruptedAesKey = _obfuscate.Obfuscate(aesKey);
                var sendData =
                    JsonConvert.SerializeObject(new CmdDataLoginSfs(corruptedAesKey, _jwtSession.RawJwt, extraData ?? string.Empty));
                var jwtLogin = _encryption.Rsa().Encrypt(sendData);
                _jwtSession.SetLoginJwt(jwtLogin);
                return jwtLogin;
            } catch (Exception e) {
                _logger.Log($"{_tag} EncryptDataForLoginSmartFox error: {e.Message}");
                throw new Exception("Something went wrong\nPlease try again");
            }
        }
        

        private record CmdDataLoginSfs(
            [property: JsonProperty("aesKey")]
            string AesKey,
            [property: JsonProperty("encryptedJwt")]
            string EncryptedJwt,
            [property: JsonProperty("extraData")]
            string ExtraData
        );
    }
    
    public interface IPublicJwtSession {
        string RawJwt { get;}
        string LoginJwt { get;}
        string PublicKey { get;}
        JwtExtraData ExtraData { get;}
        UserAccount Account { get;}
        void SetAccount(UserAccount account);
    }
    public interface IJwtSession : IPublicJwtSession {
        void SetRawJwt(string rawJwt);
        void SetLoginJwt(string loginJwt);
        void SetPublicKey(string publicKey);
        void SetExtraData(JwtExtraData extraData);
    }
    
    public class JwtSession: IJwtSession {

        public string RawJwt { get; private set; }
        public string LoginJwt { get; private set; }
        public string PublicKey { get; private  set; }
        public JwtExtraData ExtraData { get; private set; }
        public UserAccount Account { get; private set; }

        public void SetAccount(UserAccount account) {
            Account = account;
        }

        public void Reset() {
            RawJwt = string.Empty;
            LoginJwt = string.Empty;
            PublicKey = string.Empty;
            ExtraData = null;
            Account = null;
        }

        public void SetRawJwt(string rawJwt) {
            RawJwt = rawJwt;
        }

        public void SetLoginJwt(string loginJwt) {
            LoginJwt = loginJwt;
        }

        public void SetPublicKey(string publicKey) {
            PublicKey = publicKey;
        }

        public void SetExtraData(JwtExtraData extraData) {
            ExtraData = extraData;
        }
    }
}