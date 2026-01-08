using System.Security.Cryptography;
using System.Text;

namespace Services.Server {
    public interface IHasher {
        string GetHash(string plainText);
    }

    public class Hasher : IHasher {
        private readonly MD5CryptoServiceProvider _md5CryptoService = new();

        public string GetHash(string plainText) {
            // Compute hash from the bytes of text
            _md5CryptoService.ComputeHash(Encoding.ASCII.GetBytes(plainText));
            // Get hash result after compute it
            var result = _md5CryptoService.Hash;
            var strBuilder = new StringBuilder();
            foreach (var t in result) {
                strBuilder.Append(t.ToString("x2"));
            }
            return strBuilder.ToString();
        }
    }
}