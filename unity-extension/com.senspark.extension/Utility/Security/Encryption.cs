using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Senspark.Security {
    public static class Encryption {
        public static string EncryptDataWithAes(string plainText, out string keyBase64, out string vectorBase64) {
            using var aesAlgorithm = Aes.Create();
            //set the parameters with out keyword
            keyBase64 = Convert.ToBase64String(aesAlgorithm.Key);
            vectorBase64 = Convert.ToBase64String(aesAlgorithm.IV);

            // Create encryptor object
            var encryptor = aesAlgorithm.CreateEncryptor();

            byte[] encryptedData;

            //Encryption will be done in a memory stream through a CryptoStream object
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plainText);
                    }
                    encryptedData = ms.ToArray();
                }
            }

            return Convert.ToBase64String(encryptedData);
        }

        public static string DecryptDataWithAes(string cipherText, string keyBase64, string vectorBase64) {
            using var aesAlgorithm = Aes.Create();
            aesAlgorithm.Key = Convert.FromBase64String(keyBase64);
            aesAlgorithm.IV = Convert.FromBase64String(vectorBase64);

            // Create decryptor object
            var decryptor = aesAlgorithm.CreateDecryptor();

            var cipher = Convert.FromBase64String(cipherText);

            //Decryption will be done in a memory stream through a CryptoStream object
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        public static HashData HashSha256(string str) {
            // You can perform additional operations to modify the class name before generating the key
            // For example, you can concatenate it with a secret value or apply a hashing algorithm

            // Convert the class name to bytes
            var keyBytes = Encoding.UTF8.GetBytes(str);

            // Use a hash algorithm to derive a fixed-length key
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(keyBytes);

            return new HashData(hashBytes);
        }

        public static HashData HashMd5(byte[] bytes) {
            using var md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(bytes);
            return new HashData(hashBytes);
        }

        public static string Obfuscate(string input) {
            var bytes = Encoding.UTF8.GetBytes(input);

            // Reverse the byte order
            Array.Reverse(bytes);

            // Convert bytes to a hex string
            var hexString = BitConverter.ToString(bytes).Replace("-", "");

            return hexString;
        }

        public static string DeObfuscate(string input) {
            // Convert the hex string back to bytes
            var bytes = new byte[input.Length / 2];
            for (var i = 0; i < input.Length; i += 2) {
                bytes[i / 2] = Convert.ToByte(input.Substring(i, 2), 16);
            }

            // Reverse the byte order back to the original
            Array.Reverse(bytes);

            // Convert bytes back to the original string
            var originalString = Encoding.UTF8.GetString(bytes);

            return originalString;
        }
        
        public class HashData {
            private readonly byte[] _hashBytes;
            
            public HashData(byte[] hashBytes) {
                _hashBytes = hashBytes;
            }
            
            public string ToBase64String() {
                return Convert.ToBase64String(_hashBytes);
            }
            
            public string ToHexString() {
                var sb = new StringBuilder();
                foreach (var t in _hashBytes) {
                    sb.Append(t.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}