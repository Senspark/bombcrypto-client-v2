using System;

public class TonAddressConverter
{
    public static string ToFriendlyAddress(string rawAddress, bool bounceable = true, bool testnet = false) {
        var parts = rawAddress.Split(':');
        if (parts.Length != 2) {
            // Invalid raw address
            return rawAddress;
        }

        int workchain = int.Parse(parts[0]);
        byte[] addressBytes = HexToBytes(parts[1]);
        if (addressBytes.Length != 32) {
            // Invalid address length
            return rawAddress;
        }

        // Compute tag byte
        byte tag = bounceable ? (byte)0x11 : (byte)0x51;
        if (testnet) tag |= 0x80;

        // Create 34-byte body (tag + workchain + 32-byte address)
        byte[] body = new byte[34];
        body[0] = tag;
        body[1] = workchain == -1 ? (byte)0xFF : (byte)0x00;
        Array.Copy(addressBytes, 0, body, 2, 32);

        // Add CRC16 checksum
        ushort crc = CRC16CCITT(body, 34);
        byte[] full = new byte[36];
        Array.Copy(body, 0, full, 0, 34);
        full[34] = (byte)(crc >> 8);
        full[35] = (byte)(crc & 0xFF);

        return Convert.ToBase64String(full)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    private static ushort CRC16CCITT(byte[] data, int length)
    {
        const ushort polynomial = 0x1021;
        ushort crc = 0xFFFF;
        for (int i = 0; i < length; i++)
        {
            crc ^= (ushort)(data[i] << 8);
            for (int j = 0; j < 8; j++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ polynomial);
                else
                    crc <<= 1;
                crc &= 0xFFFF;
            }
        }
        return crc;
    }

    private static byte[] HexToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hex string must have an even length");

        byte[] result = new byte[hex.Length / 2];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return result;
    }
}
