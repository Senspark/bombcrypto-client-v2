using System;

using Analytics;

using Constant;

namespace Utils {
    public static class ConversionConvert {
        public static ConversionType ConvertUsedBoosterPve(int booster) {
            return (GachaChestProductId) booster switch {
                GachaChestProductId.Shield => ConversionType.UsedBoosterShieldPve,
                GachaChestProductId.BombAdd1 => ConversionType.UsedBoosterBombPve,
                GachaChestProductId.SpeedAdd1 => ConversionType.UsedBoosterSpeedPve,
                GachaChestProductId.RangeAdd1 => ConversionType.UsedBoosterRangePve,
                _ => throw new ArgumentOutOfRangeException(nameof(booster), booster, null)
            };
        }
        
        public static ConversionType ConvertUseBoosterPvp(int booster) {
            return (GachaChestProductId) booster switch {
                GachaChestProductId.Key => ConversionType.UsedBoosterKeyPvp,
                GachaChestProductId.Shield => ConversionType.UsedBoosterShieldPvp,
                GachaChestProductId.RankGuardian => ConversionType.UsedBoosterRankGuardianPvp,
                GachaChestProductId.FullRankGuardian => ConversionType.UsedBoosterFullRankGuardianPvp,
                GachaChestProductId.ConquestCard => ConversionType.UsedBoosterConquestCardPvp,
                GachaChestProductId.FullConquestCard => ConversionType.UsedBoosterFullConquestCardPvp,
                GachaChestProductId.BombAdd1 => ConversionType.UsedBoosterBombPvp,
                GachaChestProductId.SpeedAdd1 => ConversionType.UsedBoosterSpeedPvp,
                GachaChestProductId.RangeAdd1 => ConversionType.UsedBoosterRangePvp,
                _ => throw new ArgumentOutOfRangeException(nameof(booster), booster, null)
            };
        }
    }
}