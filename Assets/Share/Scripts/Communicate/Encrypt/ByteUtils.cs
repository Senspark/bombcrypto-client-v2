using System;
using System.Linq;
using System.Text;

namespace Communicate.Encrypt {
    public static class ByteUtils {
        /// <summary>
        /// Converts a string to Base64.
        /// </summary>
        /// <param name="text">Text input (can be ASCII, UTF-8, UTF-16, UTF-32).</param>
        /// <returns>Base64 encoded string.</returns>
        public static string StringToBase64(string text) {
            var bytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(bytes);
        }
        
        /// <summary>
        /// Converts a byte array to a string.
        /// </summary>
        /// <param name="base64">A valid Base64 string.</param>
        /// <returns>Decoded string (could be ASCII, UTF-8, UTF-16, UTF-32).</returns>
        public static string Base64ToString(string base64) {
            var bytes = Convert.FromBase64String(base64);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Converts a Base64 string to a byte array.
        /// </summary>
        /// <param name="base64">A valid Base64 string.</param>
        /// <returns>Byte array.</returns>
        public static byte[] Base64ToByteArray(string base64) {
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Converts a string to a byte array.
        /// </summary>
        /// <param name="text">Text input (can be ASCII, UTF-8, UTF-16, UTF-32).</param>
        /// <returns>Byte array.</returns>
        public static byte[] StringToByteArray(string text) {
            return Encoding.UTF8.GetBytes(text);
        }

        /// <summary>
        /// Converts a byte array to a string.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <returns>Decoded string (could be ASCII, UTF-8, UTF-16, UTF-32).</returns>
        public static string ByteArrayToString(byte[] bytes) {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Converts a byte array to a Base64 string.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <returns>Base64 encoded string.</returns>
        public static string ByteArrayToBase64(byte[] bytes) {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Converts a uint (positive integer) to a 4-byte array.
        /// </summary>
        /// <param name="uintValue">Unsigned 32-bit integer.</param>
        /// <returns>4-byte array.</returns>
        public static byte[] Uint32ToByteArray(uint uintValue) {
            var result = new byte[4];
            BitConverter.GetBytes(uintValue).CopyTo(result, 0);
            return result;
        }

        /// <summary>
        /// Converts a 4-byte array to a uint.
        /// </summary>
        /// <param name="bytes">4-byte array.</param>
        /// <returns>Unsigned 32-bit integer.</returns>
        public static uint ByteArrayToUint32(byte[] bytes) {
            if (bytes.Length != 4) {
                throw new ArgumentException("bytes must be 4 bytes.");
            }
            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Merges multiple byte arrays into a single byte array in order.
        /// </summary>
        /// <param name="arrays">Byte arrays to merge.</param>
        /// <returns>Concatenated byte array.</returns>
        public static byte[] MergeByteArray(params byte[][] arrays) {
            var totalLength = arrays.Sum(arr => arr.Length);

            var result = new byte[totalLength];
            var offset = 0;

            foreach (var arr in arrays) {
                Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
                offset += arr.Length;
            }

            return result;
        }
    }
}