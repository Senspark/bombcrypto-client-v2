using System;
using System.Globalization;

namespace App {
    // The `reward_type` int on the wire is the server's own enum value, not BlockRewardType. The two
    // numberings agree up to 28 and diverge after it: BNB_DEPOSITED is 31 on the wire but 34 here, and
    // 31 here is Other. A raw cast in either direction compiles and yields the wrong token, so every
    // conversion goes through this map.
    public static class ServerRewardTypes {
        public const int Bcoin = 1;
        public const int BcoinDeposited = 4;
        public const int Senspark = 7;
        public const int Rock = 23;
        public const int TonDeposited = 24;
        public const int SolDeposited = 25;
        public const int RonDeposited = 26;
        public const int BasDeposited = 27;
        public const int VicDeposited = 28;
        public const int BnbDeposited = 31;
        public const int PolDeposited = 32;

        public static int ToWire(BlockRewardType type) {
            return type switch {
                BlockRewardType.BCoin => Bcoin,
                BlockRewardType.BCoinDeposited => BcoinDeposited,
                BlockRewardType.Senspark => Senspark,
                BlockRewardType.Rock => Rock,
                BlockRewardType.TonDeposited => TonDeposited,
                BlockRewardType.SolDeposited => SolDeposited,
                BlockRewardType.RonDeposited => RonDeposited,
                BlockRewardType.BasDeposited => BasDeposited,
                BlockRewardType.VicDeposited => VicDeposited,
                BlockRewardType.BnbDeposited => BnbDeposited,
                BlockRewardType.PolDeposited => PolDeposited,
                _ => throw new Exception($"No server reward type for {type}")
            };
        }

        public static BlockRewardType FromWire(int wire) {
            return wire switch {
                Bcoin => BlockRewardType.BCoin,
                BcoinDeposited => BlockRewardType.BCoinDeposited,
                Senspark => BlockRewardType.Senspark,
                Rock => BlockRewardType.Rock,
                TonDeposited => BlockRewardType.TonDeposited,
                SolDeposited => BlockRewardType.SolDeposited,
                RonDeposited => BlockRewardType.RonDeposited,
                BasDeposited => BlockRewardType.BasDeposited,
                VicDeposited => BlockRewardType.VicDeposited,
                BnbDeposited => BlockRewardType.BnbDeposited,
                PolDeposited => BlockRewardType.PolDeposited,
                _ => BlockRewardType.Other
            };
        }

        public static bool IsNative(int wire) {
            return wire == BnbDeposited || wire == PolDeposited;
        }

        // Only BSC and POLYGON have a native balance; every other network returns null and the caller
        // hides its native payment option.
        public static BlockRewardType? NativeOf(NetworkType network) {
            return network switch {
                NetworkType.Binance => BlockRewardType.BnbDeposited,
                NetworkType.Polygon => BlockRewardType.PolDeposited,
                _ => null
            };
        }

        // A pack costs ~0.0027 BNB or ~1714 POL, so the 2 decimals BCOIN is formatted with would round a
        // BNB price to zero. Trailing zeros are stripped and the output stays ASCII for the WebGL font.
        public static string FormatAmount(double value) {
            return value.ToString("0.#########", CultureInfo.InvariantCulture);
        }
    }
}
