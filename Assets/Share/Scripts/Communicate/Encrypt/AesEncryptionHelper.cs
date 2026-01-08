namespace Communicate.Encrypt {
    public class AesEncryptionHelper {
        public AesEncryptionHelper(AesEncryption encryption, IObfuscate obfuscate) {
            _encryption = encryption;
            _obfuscate = obfuscate;
        }

        private readonly AesEncryption _encryption;
        private readonly IObfuscate _obfuscate;

        public string Encrypt(string plainText) {
            var newAes = new AesEncryption();
            newAes.GenerateKey();
            var iv = newAes.GetIv();
            newAes.SetKeyAndIv(_encryption.GetKey(), iv);

            var encrypted = newAes.Encrypt(plainText);
            var mergedBytes = ByteUtils.MergeByteArray(
                ByteUtils.Base64ToByteArray(iv), ByteUtils.Base64ToByteArray(encrypted));
            return _obfuscate.Obfuscate2(mergedBytes);
        }

        public string Decrypt(string encryptedData) {
            var bytes = _obfuscate.DeObfuscate2(encryptedData);
            var iv = ByteUtils.ByteArrayToBase64(bytes[.._encryption.IvSize]);
            var cipherText = ByteUtils.ByteArrayToBase64(bytes[_encryption.IvSize..]);

            var newAes = new AesEncryption();
            newAes.SetKeyAndIv(_encryption.GetKey(), iv);

            var decrypted = newAes.Decrypt(cipherText);
            return decrypted;
        }

        public string Decrypt(byte[] encryptedData) {
            var bytes = _obfuscate.DeObfuscateBytes(encryptedData);
            var iv = ByteUtils.ByteArrayToBase64(bytes[.._encryption.IvSize]);
            var cipherText = ByteUtils.ByteArrayToBase64(bytes[_encryption.IvSize..]);

            var newAes = new AesEncryption();
            newAes.SetKeyAndIv(_encryption.GetKey(), iv);

            var decrypted = newAes.Decrypt(cipherText);
            return decrypted;
        }
    }
}