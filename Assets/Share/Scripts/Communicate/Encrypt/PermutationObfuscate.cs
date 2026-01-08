using System;

namespace Communicate.Encrypt {
    /**
    * Hoán vị bytes theo một permutation order cho trước.
    * Yêu cầu input string phải >= permutation order.
    * Nếu input string dài hơn permutation order, thì chỉ hoán vị những bytes đầu tiên.
    */
    public class PermutationObfuscate : IObfuscate {
        private readonly byte[] _permutationOrder;

        public PermutationObfuscate(byte[] permutationOrder) {
            _permutationOrder = permutationOrder;
        }

        public string Obfuscate(string base64) {
            return Obfuscate2(ByteUtils.Base64ToByteArray(base64));
        }

        public string Obfuscate2(byte[] bytes) {
            var obfuscatedBytes = PermuteKey(bytes, _permutationOrder);
            return ByteUtils.ByteArrayToBase64(obfuscatedBytes);
        }

        public string DeObfuscate(string base64) {
            return ByteUtils.ByteArrayToBase64(DeObfuscate2(base64));
        }

        public byte[] DeObfuscate2(string base64) {
            var bytes = ByteUtils.Base64ToByteArray(base64);
            return ReversePermutation(bytes, _permutationOrder);
        }

        public byte[] DeObfuscateBytes(byte[] data) {
            return ReversePermutation(data, _permutationOrder);
        }

        private byte[] PermuteKey(byte[] key, byte[] permutationOrder) {
            if (key.Length < permutationOrder.Length) {
                throw new ArgumentException("Invalid key length.");
            }

            var permutedKey = new byte[key.Length];
            for (var i = 0; i < permutationOrder.Length; i++) {
                permutedKey[i] = key[permutationOrder[i]];
            }

            for (var i = permutationOrder.Length; i < key.Length; i++) {
                permutedKey[i] = key[i];
            }

            return permutedKey;
        }

        private byte[] ReversePermutation(byte[] key, byte[] permutationOrder) {
            if (key.Length < permutationOrder.Length) {
                throw new ArgumentException("Invalid key length.");
            }

            var originalKey = new byte[key.Length];
            for (var i = 0; i < permutationOrder.Length; i++) {
                originalKey[permutationOrder[i]] = key[i];
            }

            for (var i = permutationOrder.Length; i < key.Length; i++) {
                originalKey[i] = key[i];
            }

            return originalKey;
        }
    }
}