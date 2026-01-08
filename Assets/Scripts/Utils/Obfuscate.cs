using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using BLPvpMode.Engine.Utility;

using UnityEngine;

using Random = System.Random;

namespace Utils {
    public static class Obfuscate {
        private static readonly Random Random = new();
        
        /**
         * @param[plainText] is utf string
         * @param[key] is utf string
         * @return utf string
         */
        public static string ObfuscateText(string plainText, string key) {
            var resultText = "";
            for (int i = 0; i < plainText.Length; i++) {
                int random = Random.Next(0, 10); // random from [0 -> 9]
                resultText += random;
                resultText += plainText[i];
            }
            return Xor(resultText, key);
        }
        
        /**
         * @param[cipherText] is utf string
         * @param[key] is utf string
         * @return utf string
         */
        public static string DeobfuscateText(string cipherText, string key) {
            string firstText = Xor(cipherText, key);
            var resultText = "";
            for (int i = 0; i < firstText.Length; i++) {
                if (i % 2 != 0) {
                    resultText += firstText[i];
                }
            }
            return resultText;
        }
        
        /**
         * @param[plainText] is Utf string
         * @param[key] is Utf string
         * @return cipherText is Utf string
         */
        public static string Xor(string plainText, string key) {
            int keyLength = key.Length;
            int dataLength = plainText.Length;
            var result = "";
            
            for (int i = 0; i < dataLength; i++) {
                result += (char)(plainText[i] ^ key[i % keyLength]);
            }
            return result;
        }
        
        /// <summary>
        /// Maximum chỉ support 64 bit
        /// </summary>
        /// <param name="heroId"></param>
        /// <param name="bombNo"></param>
        /// <param name="colBoom"></param>
        /// <param name="rowBoom"></param>
        /// <returns></returns>
        public static long ObfuscateData(int heroId, int bombNo, int colBoom, int rowBoom) {
            var randomInt = Random.Next(0, 32);
            var bitEncoder = new LongBitEncoder()
                .Push(heroId, 32) // 32 bit [0, 2_147_483_647]
                .Push(bombNo, 5) // 5 bit [0, 31]
                .Push(colBoom, 8) // 8 bit [0, 255]
                .Push(rowBoom, 8) // 8 bit [0, 255]
                .Push(randomInt, 5); // 5 bit [0, 31]
            // 6 bits remaining
            return bitEncoder.Value;
        }
        
        /// <summary>
        /// Mã hoá dùng RSA (public key)
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        
        public static string RsaEncryption(string plainText, string publicKey) {
            
            //Debug.Log("plant text "+ plainText);
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
            //Debug.Log("Bytes "+ dataToEncrypt.Length);
            byte[] encryptedData;
            
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(GetRsaParametersFromX509PublicKey(publicKey));
                //Debug.Log("key size " +rsa.KeySize);
                encryptedData = rsa.Encrypt(dataToEncrypt, false);  // The second parameter indicates whether to use OAEP padding
            }
            //Debug.Log("Final "+encryptedData.Length);
            return Convert.ToBase64String(encryptedData);
            
            
        }
        
        private static RSAParameters GetRsaParametersFromX509PublicKey(string publicKeyBase64)
        {
            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
            
            // Assumes the key is in the SubjectPublicKeyInfo structure (X.509)
            // It requires ASN.1 decoding
            using (var ms = new System.IO.MemoryStream(publicKeyBytes))
            using (var reader = new System.IO.BinaryReader(ms))
            {
                byte[] modulus, exponent;
                
                // Read the sequence
                reader.ReadBytes(25); // Skip the sequence header
                
                // Read the modulus
                int modulusSize = GetIntegerSize(reader);
                modulus = reader.ReadBytes(modulusSize);
                
                // Read the exponent
                int exponentSize = GetIntegerSize(reader);
                exponent = reader.ReadBytes(exponentSize);
                
                return new RSAParameters
                {
                    Modulus = modulus,
                    Exponent = new byte[] { 1, 0, 1 }
                };
            }
        }
        
        private static int GetIntegerSize(System.IO.BinaryReader reader)
        {
            byte bt = 0;
            int count = 0;
            bt = reader.ReadByte();
            if (bt != 0x02) // expect integer
                return 0;
            
            bt = reader.ReadByte();
            if (bt == 0x81)
                count = reader.ReadByte();
            else if (bt == 0x82)
            {
                var highbyte = reader.ReadByte();
                var lowbyte = reader.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
                count = bt;
            
            while (reader.ReadByte() == 0x00)
                count -= 1; // Remove leading zeroes
            reader.BaseStream.Seek(-1, System.IO.SeekOrigin.Current);
            
            return count;
        }
        
#region Có thể dùng, để tạm
        
        // public static string RsaEncryption(string plainText, string publicKey) {
        //     try
        //     {
        //         Debug.Log(publicKey);
        //         
        //         byte[] publicKeyBytes = Convert.FromBase64String(publicKey);
        //         
        //         // Create an X509Certificate2 object from the public key bytes
        //         X509Certificate2 cert = new X509Certificate2(publicKeyBytes);
        //         
        //         // Get the RSA public key from the certificate
        //         using (RSA rsa = cert.GetRSAPublicKey())
        //         {
        //             // Encrypt the data
        //             byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
        //             byte[] encryptedData = rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.Pkcs1);
        //             
        //             // Return the encrypted data as a base64 string
        //             return Convert.ToBase64String(encryptedData);
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         // Handle or log the exception as needed
        //         throw new CryptographicException("An error occurred during RSA encryption.", ex);
        //     }
        // }
    //     public static string RsaEncryption(string plainText, string publicKey) {
    //     try
    //     {
    //         // Generate a new AES key and IV
    //         byte[] aesKey, aesIV;
    //         using (Aes aes = Aes.Create())
    //         {
    //             aesKey = aes.Key;
    //             aesIV = aes.IV;
    //         }
    //
    //         // Encrypt the data using AES
    //         byte[] encryptedData = EncryptWithAes(plainText, aesKey, aesIV);
    //
    //         // Convert the base64 string to a byte array
    //         byte[] publicKeyBytes = Convert.FromBase64String(publicKey);
    //
    //         // Parse the RSA public key to get modulus and exponent
    //         RSAParameters rsaParameters = DecodeRsaPublicKey(publicKeyBytes);
    //
    //         // Create an RSA instance and import the public key
    //         using (RSA rsa = RSA.Create())
    //         {
    //             rsa.ImportParameters(rsaParameters);
    //
    //             // Encrypt the AES key and IV with RSA
    //             byte[] encryptedAesKey = rsa.Encrypt(aesKey, RSAEncryptionPadding.Pkcs1);
    //             byte[] encryptedAesIV = rsa.Encrypt(aesIV, RSAEncryptionPadding.Pkcs1);
    //
    //             // Combine the encrypted AES key, IV, and data
    //             byte[] combinedData = Combine(encryptedAesKey, encryptedAesIV, encryptedData);
    //
    //             // Return the combined data as a base64 string
    //             return Convert.ToBase64String(combinedData);
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Handle or log the exception as needed
    //         throw new CryptographicException("An error occurred during encryption.", ex);
    //     }
    // }
    //
    // private static byte[] EncryptWithAes(string plainText, byte[] key, byte[] iv)
    // {
    //     using (Aes aes = Aes.Create())
    //     {
    //         aes.Key = key;
    //         aes.IV = iv;
    //         aes.Mode = CipherMode.CBC;
    //         aes.Padding = PaddingMode.PKCS7;
    //
    //         using (MemoryStream ms = new MemoryStream())
    //         {
    //             using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
    //             {
    //                 using (StreamWriter writer = new StreamWriter(cs))
    //                 {
    //                     writer.Write(plainText);
    //                 }
    //             }
    //             return ms.ToArray();
    //         }
    //     }
    // }
    //
    // private static RSAParameters DecodeRsaPublicKey(byte[] publicKeyBytes)
    // {
    //     using (MemoryStream ms = new MemoryStream(publicKeyBytes))
    //     using (BinaryReader reader = new BinaryReader(ms))
    //     {
    //         reader.ReadBytes(22); // Skip the initial sequence
    //         byte[] modulus = reader.ReadBytes((int)(reader.BaseStream.Length - 25)); // Read the modulus
    //         reader.ReadByte(); // Skip the next byte
    //         byte[] exponent = reader.ReadBytes(3); // Read the exponent (usually 3 bytes for 65537)
    //
    //         return new RSAParameters { Modulus = modulus, Exponent = exponent };
    //     }
    // }
    //
    // private static byte[] Combine(params byte[][] arrays)
    // {
    //     int totalLength = 0;
    //     foreach (byte[] array in arrays)
    //     {
    //         totalLength += array.Length;
    //     }
    //
    //     byte[] combined = new byte[totalLength];
    //     int offset = 0;
    //     foreach (byte[] array in arrays)
    //     {
    //         Buffer.BlockCopy(array, 0, combined, offset, array.Length);
    //         offset += array.Length;
    //     }
    //     return combined;
    // }
    //
    
    // public static string RsaEncryption(string plainText, string publicKey)
    //     {
    //         RSAParameters rsaParams = ConvertFromBase64String(publicKey);
    //         
    //         using (RSA rsa = RSA.Create())
    //         {
    //             rsa.ImportParameters(rsaParams);
    //             
    //             byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.Pkcs1);
    //             return Convert.ToBase64String(encryptedData);
    //         }
    //     }
    //     
    //     private static RSAParameters ConvertFromBase64String(string publicKey)
    //     {
    //         // Convert the Base64 string to bytes
    //         byte[] publicKeyBytes = Convert.FromBase64String(publicKey);
    //         Debug.Log("length " + publicKeyBytes.Length);
    //         RSAParameters rsaParams = new RSAParameters();
    //         
    //         // Modulus length for a 1024-bit key
    //         int modulusLength = 128;
    //         
    //         // Extract the modulus bytes
    //         rsaParams.Modulus = new byte[modulusLength];
    //         Buffer.BlockCopy(publicKeyBytes, 11, rsaParams.Modulus, 0, modulusLength);
    //         
    //         // Exponent length for a 1024-bit key
    //         //int exponentLength = 3; // Assuming 3 bytes for the exponent, it's usually 65537 (0x10001)
    //         
    //         // Extract the exponent bytes
    //         // rsaParams.Exponent = new byte[exponentLength];
    //         // Buffer.BlockCopy(publicKeyBytes, modulusLength, rsaParams.Exponent, 0, exponentLength);
    //         rsaParams.Exponent = new byte[] { 1, 0, 1 };
    //         
    //         return rsaParams;
    //     }
        
        // public static string RsaEncryption(string plainText, string publicKey) {
        //     
        //     Debug.Log("plant text "+ plainText);
        //     byte[] dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
        //     Debug.Log("Bytes "+ dataToEncrypt.Length);
        //     byte[] encryptedData;
        //     
        //     using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        //     {
        //         rsa.ImportParameters(GetRSAParametersFromPublicKey(publicKey));
        //         Debug.Log("key size " +rsa.KeySize);
        //         encryptedData = rsa.Encrypt(dataToEncrypt, false);  // The second parameter indicates whether to use OAEP padding
        //     }
        //     Debug.Log("Final "+encryptedData.Length);
        //     return Convert.ToBase64String(encryptedData);
        //     
        //     
        //     // using (RSA rsa = RSA.Create())
        //     // {
        //     //     rsa.ImportParameters(GetRSAParametersFromPublicKey(publicKey));
        //     //     byte[] encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(plainText), RSAEncryptionPadding.Pkcs1);
        //     //     return Convert.ToBase64String(encryptedData);
        //     // }
        //     
        // }
        // private static RSAParameters GetRSAParametersFromPublicKey(string publicKeyBase64)
        // {
        //     byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);
        //     
        //     RSAParameters rsaParameters = new RSAParameters();
        //     
        //     // Decode the base64 public key bytes directly
        //     //rsaParameters.Modulus = publicKeyBytes;
        //     
        //     int modulusLength = 128; // Example modulus length (in bytes) for a 1024-bit key
        //     rsaParameters.Modulus = new byte[modulusLength];
        //     Array.Copy(publicKeyBytes, 0, rsaParameters.Modulus, 0, modulusLength);
        //     
        //     int exponentLength = publicKeyBase64.Length - modulusLength;
        //     rsaParameters.D = new byte[exponentLength];
        //     Array.Copy(publicKeyBytes, modulusLength, rsaParameters.D, 0, exponentLength);
        //     
        //     // Public exponent for RSA is usually 65537 (0x10001 in hexadecimal).
        //     // You can hardcode this value or use a different approach to obtain it if it varies.
        //     rsaParameters.Exponent = new byte[] { 1, 0, 1 };
        //     
        //     return rsaParameters;
        // }
        
        public static string RsaDecryption(string encryptedTextBase64, string privateKeyBase64) {
            byte[] encryptedData = Convert.FromBase64String(encryptedTextBase64);
            byte[] decryptedData;
            
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider()) {
                rsa.ImportParameters(GetRSAParametersFromPrivateKey(privateKeyBase64));
                decryptedData =
                    rsa.Decrypt(encryptedData,
                        false); // The second parameter should match the padding used during encryption
            }
            
            return Encoding.UTF8.GetString(decryptedData);
        }
        
        private static RSAParameters GetRSAParametersFromPrivateKey(string privateKeyBase64) {
            byte[] privateKeyBytes = Encoding.UTF8.GetBytes(privateKeyBase64);
            
            // The private key usually contains both modulus and the private exponent
            // This example assumes a simple key structure and you may need to adjust it based on your actual key format
            RSAParameters rsaParameters = new RSAParameters();
            
            // Here, the private key format is assumed to be directly in the byte array
            // You need to parse the byte array to get the actual parameters
            // For simplicity, assume the first part is modulus and the second part is the private exponent
            // In a real-world scenario, you would likely need to handle ASN.1 or PEM format parsing
            //
            // int modulusLength = 128; // Example modulus length (in bytes) for a 1024-bit key
            // rsaParameters.Modulus = new byte[modulusLength];
            // Array.Copy(privateKeyBytes, 0, rsaParameters.Modulus, 0, modulusLength);
            //
            // int exponentLength = privateKeyBytes.Length - modulusLength;
            // rsaParameters.D = new byte[exponentLength];
            // Array.Copy(privateKeyBytes, modulusLength, rsaParameters.D, 0, exponentLength);
            
            rsaParameters.Modulus = privateKeyBytes;
            // Public exponent is typically known and can be hardcoded
            rsaParameters.Exponent = new byte[] { 1, 0, 1 };
            
            return rsaParameters;
        }
        
        #endregion        
        
    }
}