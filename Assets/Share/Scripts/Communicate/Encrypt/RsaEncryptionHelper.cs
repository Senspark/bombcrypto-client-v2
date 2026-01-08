namespace Communicate.Encrypt {
    public class RsaEncryptionHelper {
        public RsaEncryptionHelper(RsaEncryption encryption, IObfuscate obfuscate) {
            _encryption = encryption;
            _obfuscate = obfuscate;
        }

        private readonly RsaEncryption _encryption;
        private readonly IObfuscate _obfuscate;

        public string ExportData() {
            var publicKey = _encryption.GetPublicKeyBase64();
            return _obfuscate.Obfuscate(publicKey);
        }
        
        public string ImportData(string data) {
            var unpack1 = _obfuscate.DeObfuscate(data);
            var decrypted = _encryption.Decrypt(unpack1);
            if (string.IsNullOrEmpty(decrypted)) {
                return null;
            }
            var unpack2 = _obfuscate.DeObfuscate(decrypted);
            return unpack2;
        }
    }
}