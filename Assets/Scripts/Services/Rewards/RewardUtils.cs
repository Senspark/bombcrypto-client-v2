using System;

using Game.UI;

namespace App {
    public static class RewardUtils {
        public static BlockRewardType ConvertToBlockRewardType(string str) {
            return str switch {
                //DevHoang: Add new airdrop
                "BCOIN" => BlockRewardType.BCoin,
                "BOMBERMAN" => BlockRewardType.Hero,
                "KEY" => BlockRewardType.Key,
                "BCOIN_DEPOSITED" => BlockRewardType.BCoinDeposited,
                "SENSPARK" => BlockRewardType.Senspark,
                "SENSPARK_DEPOSITED" => BlockRewardType.SensparkDeposited,
                "BOSS_TICKET" => BlockRewardType.BossTicket,
                "PVP_TICKET" => BlockRewardType.PvpTicket,
                "LUCKY_TICKET" => BlockRewardType.LuckyTicket,
                "NFT_PVP" => BlockRewardType.NftPvP,
                "GEM" => BlockRewardType.Gem,
                "GEM_LOCKED" => BlockRewardType.LockedGem,
                "GOLD" => BlockRewardType.BLGold,
                "COIN" => BlockRewardType.BLCoin,
                "ROCK" => BlockRewardType.Rock,
                "TON_DEPOSITED" => BlockRewardType.TonDeposited,
                "SOL_DEPOSITED" => BlockRewardType.SolDeposited,
                "RON_DEPOSITED" => BlockRewardType.RonDeposited,
                "BAS_DEPOSITED" => BlockRewardType.BasDeposited,
                "VIC_DEPOSITED" => BlockRewardType.VicDeposited,
                _ => BlockRewardType.Other,
            };
        }
        
        public static BlockRewardType ConvertToBlockRewardType(this BuyHeroCategory category) {
            return category switch {
                BuyHeroCategory.WithBcoin => BlockRewardType.BCoin,
                BuyHeroCategory.WithSen => BlockRewardType.Senspark,
                BuyHeroCategory.SuperBox => BlockRewardType.Senspark,
                _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
            };
        }

        public static string ConvertToBlockRewardType(BlockRewardType type) {
            return type switch {
                //DevHoang: Add new airdrop
                BlockRewardType.BCoin => "BCOIN",
                BlockRewardType.Hero => "BOMBERMAN",
                BlockRewardType.Key => "KEY",
                BlockRewardType.BCoinDeposited => "BCOIN_DEPOSITED",
                BlockRewardType.Senspark => "SENSPARK",
                BlockRewardType.SensparkDeposited => "SENSPARK_DEPOSITED",
                BlockRewardType.BossTicket => "BOSS_TICKET",
                BlockRewardType.PvpTicket => "PVP_TICKET",
                BlockRewardType.LuckyTicket => "LUCKY_TICKET",
                BlockRewardType.NftPvP => "NFT_PVP",
                BlockRewardType.Gem => "GEM",
                BlockRewardType.LockedGem => "GEM_LOCKED",
                BlockRewardType.BLGold => "GOLD",
                BlockRewardType.BLCoin => "COIN",
                BlockRewardType.Rock => "ROCK",
                BlockRewardType.TonDeposited => "TON_DEPOSITED",
                BlockRewardType.SolDeposited => "SOL_DEPOSITED",
                BlockRewardType.RonDeposited => "RON_DEPOSITED",
                BlockRewardType.BasDeposited => "BAS_DEPOSITED",
                BlockRewardType.VicDeposited => "VIC_DEPOSITED",
                _ => throw new Exception($"Wrong Block Reward Type {type}")
            };
        }

        public static string ConvertToNetworkType(NetworkType type) {
            return type switch {
                NetworkType.Binance => "BSC",
                NetworkType.Polygon => "POLYGON",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        
        public static string ConvertBuyHeroToNetworkType(BuyHeroCategory type) {
            return type switch {
                BuyHeroCategory.WithRon => nameof(DataType.RON),
                BuyHeroCategory.WithBas => nameof(DataType.BAS),
                BuyHeroCategory.WithVic => nameof(DataType.VIC),
                _ => nameof(DataType.TR)
            };
        }
        
        public static string GetDataTypeStarCore() {
            if (AppConfig.IsRonin()) {
                return nameof(DataType.RON);
            }
            if (AppConfig.IsBase()) {
                return nameof(DataType.BAS);
            }
            if (AppConfig.IsViction()) {
                return nameof(DataType.VIC);
            }
            return nameof(DataType.TR);
        }
        
        public static NetworkType ConvertToNetworkType(string networkName) {
            return networkName switch {
                //DevHoang: Add new airdrop
                 "BSC" => NetworkType.Binance,
                 "POLYGON" => NetworkType.Polygon,
                 "TON" => NetworkType.Ton,
                 "SOL" => NetworkType.Solana,
                 "RON" => NetworkType.Ronin,
                 "BAS" => NetworkType.Base,
                 "VIC" => NetworkType.Viction,
                 _ => throw new ArgumentOutOfRangeException(nameof(networkName), networkName, null)
            };
        }
        
        public static DataType ConvertNetworkToDatatype(NetworkType network) {
            return network switch {
                NetworkType.Binance => DataType.BSC,
                NetworkType.Polygon => DataType.POLYGON,
                NetworkType.Ton => DataType.TON,
                NetworkType.Solana => DataType.SOL,
                NetworkType.Ronin => DataType.RON,
                NetworkType.Base => DataType.BAS,
                NetworkType.Viction => DataType.VIC,
                
                _ => DataType.TR
            };
        }
    }
}