using System;
using System.Security.Cryptography;
using System.Text;

namespace Communicate.Encrypt {
    public class AesEncryption {
        public AesEncryption() {
            _aes = Aes.Create();
            _aes.KeySize = 256;
            _aes.BlockSize = 128;
            _aes.Mode = CipherMode.CBC;
            _aes.Padding = PaddingMode.PKCS7;
            IvSize = _aes.BlockSize / 8; // 16
            IvStrLength = 24;
        }

        private readonly Aes _aes;
        public readonly int IvSize;
        public readonly int IvStrLength;

        public void GenerateKey() {
            _aes.GenerateKey();
            _aes.GenerateIV();
        }

        public string GetKey() {
            return Convert.ToBase64String(_aes.Key);
        }

        public string GetIv() {
            return Convert.ToBase64String(_aes.IV);
        }

        public void SetKeyAndIv(string key, string iv) {
            _aes.Key = Convert.FromBase64String(key);
            _aes.IV = Convert.FromBase64String(iv);
        }

        public string Encrypt(string data) {
            using var encryptor = _aes.CreateEncryptor();
            var inputBytes = Encoding.UTF8.GetBytes(data);
            var encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);
            return Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string encryptedData) {
            using var decryptor = _aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedData);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}