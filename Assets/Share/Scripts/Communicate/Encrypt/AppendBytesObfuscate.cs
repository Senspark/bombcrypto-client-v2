namespace Communicate.Encrypt {
    /**
     * Thêm random bytes vào đầu
     */
    public class AppendBytesObfuscate : IObfuscate {
        public AppendBytesObfuscate(int numberBytes) {
            _numberBytes = numberBytes;
        }

        private readonly int _numberBytes;

        public string Obfuscate(string base64) {
            return Obfuscate2(ByteUtils.Base64ToByteArray(base64));
        }

        public string Obfuscate2(byte[] bytes) {
            var newBytes = new byte[bytes.Length + _numberBytes];
            var randomBytes = new byte[_numberBytes];
            new System.Random().NextBytes(randomBytes);
            randomBytes.CopyTo(newBytes, 0);
            bytes.CopyTo(newBytes, _numberBytes);
            return ByteUtils.ByteArrayToBase64(newBytes);
        }

        public string DeObfuscate(string base64) {
            return ByteUtils.ByteArrayToBase64(DeObfuscate2(base64));
        }

        public byte[] DeObfuscate2(string base64) {
            return DeObfuscateBytes(ByteUtils.Base64ToByteArray(base64));
        }

        public byte[] DeObfuscateBytes(byte[] data) {
            var newBytes = new byte[data.Length - _numberBytes];
            for (var i = 0; i < newBytes.Length; i++) {
                newBytes[i] = data[i + _numberBytes];
            }
            return newBytes;
        }
    }
}