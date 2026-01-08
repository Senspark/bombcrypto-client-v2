using Senspark;

namespace Communicate.Encrypt {

    public interface IRsaHelper {
        void GenerateKeyPair();
        string ImportData(string data);
        string ExportData();
        string Encrypt(string data);
        string Decrypt(string encryptedData);
        void ImportPublicKeyBase64(string base64Key);
    }

    public class RsaHelper: IRsaHelper {
        private readonly RsaEncryption _encryption;
        private readonly RsaEncryptionHelper _helper;
        
        public RsaHelper(RsaEncryption encryption, RsaEncryptionHelper helper) {
            _encryption = encryption;
            _helper = helper;
        }

        public void GenerateKeyPair() {
            _encryption.GenerateKeyPair();
        }

        public string ImportData(string data) {
            return _helper.ImportData(data);
        }

        public string ExportData() {
            return _helper.ExportData();
        }

        public string Encrypt(string data) {
            return _encryption.Encrypt(data);
        }

        public string Decrypt(string encryptedData) {
            return _encryption.Decrypt(encryptedData);
        }

        public void ImportPublicKeyBase64(string base64Key) {
            _encryption.ImportPublicKeyBase64(base64Key);
        }
    }
    
    public interface IAesHelper {
        void SetKeyAndIv(string key, string iv);
        string GetIv();
        string GetKey();
        void GenerateKey();
        string Encrypt(string data);
        string Decrypt(string encryptedData);
        string Decrypt(byte[] encryptedData);
    }
    
    public class AesHelper: IAesHelper {
        private readonly AesEncryption _encryption;
        private readonly AesEncryptionHelper _helper;
        
        public AesHelper(AesEncryption encryption, AesEncryptionHelper helper) {
            _encryption = encryption;
            _helper = helper;
        }

        public void SetKeyAndIv(string key, string iv) {
            _encryption.SetKeyAndIv(key, iv);
        }

        public string GetIv() {
            return _encryption.GetIv();
        }

        public string GetKey() {
            return _encryption.GetKey();
        }

        public void GenerateKey() {
            _encryption.GenerateKey();
        }

        public string Encrypt(string data) {
            return _helper.Encrypt(data);
        }

        public string Decrypt(string encryptedData) {
            return _helper.Decrypt(encryptedData);
        }

        public string Decrypt(byte[] encryptedData) {
            return _helper.Decrypt(encryptedData);
        }
    }
    
    public class UnityEncryption {
        public UnityEncryption(
            IObfuscate obfuscate
        ) {
            var initRsa = new RsaEncryption();
            var baseAes = new AesEncryption();
            var aesHelper = new AesEncryptionHelper(baseAes, obfuscate);
            var rsaHelper = new RsaEncryptionHelper(initRsa, obfuscate);
            
            _rsa = new RsaHelper(initRsa, rsaHelper);
            _aes = new AesHelper(baseAes, aesHelper);
        }

        private readonly IRsaHelper _rsa;
        private readonly IAesHelper _aes;
        
        public IRsaHelper Rsa() {
            return _rsa;
        }
        public IAesHelper Aes() {
            return _aes;
        }
    }
}