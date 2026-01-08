using System;
using System.Text;

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Communicate.Encrypt {
    public class RsaEncryption {
        private const char Delimiter = '@';
        private const int PaddingOverhead = 42; // PCKS1 padding = 11, OAEP padding = 42

        private AsymmetricCipherKeyPair _keyPair;
        private int _maxLength;

        public void GenerateKeyPair(int keySize = 2048) {
            var keyGenerationParameters = new KeyGenerationParameters(new SecureRandom(), keySize);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            _keyPair = keyPairGenerator.GenerateKeyPair();
            _maxLength = keySize / 8 - PaddingOverhead;
        }

        public string GetPrivateKeyBase64() {
            AssertKeyPair();
            var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(_keyPair.Private);
            return Convert.ToBase64String(privateKeyInfo.GetEncoded());
        }

        public void ImportPrivateKeyBase64(string base64Key) {
            AssertKeyPair();
            var keyBytes = Convert.FromBase64String(base64Key);
            var privateKey = PrivateKeyFactory.CreateKey(keyBytes);
            _keyPair = new AsymmetricCipherKeyPair(_keyPair.Public, privateKey);
        }

        public string GetPublicKeyBase64() {
            AssertKeyPair();
            var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(_keyPair.Public);
            return Convert.ToBase64String(publicKeyInfo.GetEncoded());
        }

        public void ImportPublicKeyBase64(string base64Key) {
            AssertKeyPair();
            var keyBytes = Convert.FromBase64String(base64Key);
            var publicKey = PublicKeyFactory.CreateKey(keyBytes);
            _keyPair = new AsymmetricCipherKeyPair(publicKey, _keyPair.Private);
        }

        public string Encrypt(string data) {
            AssertKeyPair();

            var sb = new StringBuilder();
            for (var i = 0; i < data.Length; i += _maxLength) {
                var length = Math.Min(_maxLength, data.Length - i);
                var part = data.Substring(i, length);
                sb.Append(EncryptPart(part));
                sb.Append(Delimiter);
            }

            return sb.ToString();
        }

        public string Decrypt(string encryptedData) {
            AssertKeyPair();

            var sb = new StringBuilder();
            var parts = encryptedData.Split(Delimiter);
            foreach (var part in parts) {
                if (string.IsNullOrEmpty(part)) {
                    continue;
                }

                sb.Append(DecryptPart(part));
            }

            return sb.ToString();
        }

        private string EncryptPart(string data) {
            AssertKeyPair();

            if (data.Length > _maxLength)
                throw new Exception("Data is too long to encrypt");

            var encryptEngine = new OaepEncoding(new RsaEngine());
            encryptEngine.Init(true, _keyPair.Public);

            var encryptedBytes = encryptEngine.ProcessBlock(Encoding.UTF8.GetBytes(data), 0, data.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        private string DecryptPart(string encryptedData) {
            AssertKeyPair();

            if (string.IsNullOrEmpty(encryptedData))
                throw new Exception("Encrypted data is empty");

            var encryptedBytes = Convert.FromBase64String(encryptedData);

            var decryptEngine = new OaepEncoding(new RsaEngine());
            decryptEngine.Init(false, _keyPair.Private);

            var decryptedBytes = decryptEngine.ProcessBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }

        private void AssertKeyPair() {
            if (_keyPair == null)
                throw new Exception("Key pair is null");
        }
    }
}