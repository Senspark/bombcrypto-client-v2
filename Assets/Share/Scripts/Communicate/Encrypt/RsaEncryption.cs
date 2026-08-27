using System;
using System.Text;

using App;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Communicate.Encrypt {
    /// <summary>
    /// Khoá công khai và khoá riêng được giữ TÁCH RỜI, không gộp thành AsymmetricCipherKeyPair.
    ///
    /// Lý do: bên gửi lệnh lên SmartFox chỉ cần khoá công khai của server để Encrypt — nó không hề
    /// có, và không cần, một khoá riêng. Bản cũ gộp cặp khoá nên buộc phải gọi GenerateKeyPair()
    /// (RSA 2048-bit, hàng giây trên WebGL máy yếu) chỉ để có chỗ nhét khoá riêng và để gán
    /// _maxLength, rồi vứt luôn khoá công khai vừa sinh.
    /// </summary>
    public class RsaEncryption {
        private readonly char _delimiter = AppConfig.EncryptionData?.rsaDelimiter ?? '*';
        private const int PaddingOverhead = 42; // PCKS1 padding = 11, OAEP padding = 42

        private AsymmetricKeyParameter _publicKey;
        private AsymmetricKeyParameter _privateKey;
        private int _maxLength;

        public void GenerateKeyPair(int keySize = 2048) {
            var keyGenerationParameters = new KeyGenerationParameters(new SecureRandom(), keySize);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var keyPair = keyPairGenerator.GenerateKeyPair();
            _publicKey = keyPair.Public;
            _privateKey = keyPair.Private;
            _maxLength = MaxLengthOf(_publicKey, keySize);
        }

        public string GetPublicKeyBase64() {
            AssertPublicKey();
            var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(_publicKey);
            return Convert.ToBase64String(publicKeyInfo.GetEncoded());
        }

        public void ImportPublicKeyBase64(string base64Key) {
            var keyBytes = Convert.FromBase64String(base64Key);
            _publicKey = PublicKeyFactory.CreateKey(keyBytes);
            _maxLength = MaxLengthOf(_publicKey, 0);
        }

        public string Encrypt(string data) {
            AssertPublicKey();

            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i += _maxLength) {
                var length = Math.Min(_maxLength, data.Length - i);
                var part = data.Substring(i, length);
                sb.Append(EncryptPart(part));
                sb.Append(_delimiter);
            }

            return sb.ToString();
        }

        public string Decrypt(string encryptedData) {
            AssertPrivateKey();

            var sb = new StringBuilder();
            var parts = encryptedData.Split(_delimiter);
            foreach (var part in parts) {
                if (string.IsNullOrEmpty(part)) {
                    continue;
                }

                sb.Append(DecryptPart(part));
            }

            return sb.ToString();
        }

        private string EncryptPart(string data) {
            if (data.Length > _maxLength)
                throw new Exception("Data is too long to encrypt");

            var encryptEngine = new OaepEncoding(new RsaEngine());
            encryptEngine.Init(true, _publicKey);

            var encryptedBytes = encryptEngine.ProcessBlock(Encoding.UTF8.GetBytes(data), 0, data.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptPart(string encryptedData) {
            if (string.IsNullOrEmpty(encryptedData))
                throw new Exception("Encrypted data is empty");

            var encryptedBytes = Convert.FromBase64String(encryptedData);

            var decryptEngine = new OaepEncoding(new RsaEngine());
            decryptEngine.Init(false, _privateKey);

            var decryptedBytes = decryptEngine.ProcessBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        /// <summary>
        /// Suy độ dài khối tối đa từ modulus của chính khoá, thay vì phải biết trước keySize.
        /// Bỏ qua bước này thì _maxLength = 0 và vòng lặp trong Encrypt sẽ chạy vô hạn.
        /// </summary>
        private static int MaxLengthOf(AsymmetricKeyParameter key, int fallbackKeySize) {
            var keySize = key is RsaKeyParameters rsa ? rsa.Modulus.BitLength : fallbackKeySize;
            if (keySize <= 0) {
                throw new Exception("Cannot determine RSA key size");
            }
            return keySize / 8 - PaddingOverhead;
        }

        private void AssertPublicKey() {
            if (_publicKey == null)
                throw new Exception("Public key is null");
        }

        private void AssertPrivateKey() {
            if (_privateKey == null)
                throw new Exception("Private key is null");
        }
    }
}
