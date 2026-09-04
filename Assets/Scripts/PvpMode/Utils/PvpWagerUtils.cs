using Constant;

namespace PvpMode.Utils {
    public static class PvpWagerUtils {
        public static float GetAmount(PvpWagerTier tier) {
            return tier switch {
                PvpWagerTier.TIER_1 => 1f,
                PvpWagerTier.TIER_5 => 5f,
                PvpWagerTier.TIER_10 => 10f,
                PvpWagerTier.TIER_25 => 25f,
                PvpWagerTier.TIER_50 => 50f,
                PvpWagerTier.TIER_100 => 100f,
                PvpWagerTier.TIER_1K => 1000f,
                PvpWagerTier.TIER_5K => 5000f,
                PvpWagerTier.TIER_10K => 10000f,
                PvpWagerTier.TIER_25K => 25000f,
                PvpWagerTier.TIER_50K => 50000f,
                PvpWagerTier.TIER_100K => 100000f,
                _ => 0f
            };
        }

        public static string GetTokenName(PvpWagerToken token) {
            return token switch {
                PvpWagerToken.BCOIN_BSC => "BCOIN (BSC)",
                PvpWagerToken.BCOIN_POLYGON => "BCOIN (POLYGON)",
                PvpWagerToken.SEN_BSC => "SEN (BSC)",
                PvpWagerToken.SEN_POLYGON => "SEN (POLYGON)",
                _ => "Unknown"
            };
        }
    }
}
