using System;
using System.Security.Cryptography;
using System.Text;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Engine.Utils
{
    public class RsaEncryption
    {
        private readonly RSAParameters _publicKey;
    
        /// <param name="base64PublicKey">Base64 string</param>
        public RsaEncryption(string base64PublicKey)
        {
            var publicKeyBytes = Convert.FromBase64String(base64PublicKey);
            var asymmetricKeyParameter = PublicKeyFactory.CreateKey(publicKeyBytes);
            _publicKey = DotNetUtilities.ToRSAParameters((RsaKeyParameters)asymmetricKeyParameter);
        }
    
        /// <returns>Base64 string</returns>
        public string Encrypt(string utf8PlainText)
        {
            try
            {
                var dataToEncrypt = Encoding.UTF8.GetBytes(utf8PlainText);
            
                byte[] encryptedData;
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportParameters(_publicKey);
                    encryptedData = rsa.Encrypt(dataToEncrypt, false);
                }
            
                return Convert.ToBase64String(encryptedData);
            }
            catch(Exception e)
            {
                //Debug.LogError("Encryption failed." + e);
                return null;
            }
        }
    }
}