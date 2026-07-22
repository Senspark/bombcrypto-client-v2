using System.Collections.Generic;
using App;
using Game.UI;

namespace Services.Rewards {
    public static class LaunchPadTokenTable {
        public static List<TokenData> Build() {
            return new List<TokenData> {
                new() {
                    code = 1, sortOrder = 1, tokenName = BlockRewardType.BCoin, displayName = "BCOIN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, useTax = true,
                    minValueToClaim = 0, networkSymbol = DataType.BSC
                },
                new() {
                    code = 1, sortOrder = 1, tokenName = BlockRewardType.BCoin, displayName = "BCOIN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, useTax = true,
                    minValueToClaim = 40, networkSymbol = DataType.POLYGON
                },
                new() {
                    code = 2, sortOrder = 2, tokenName = BlockRewardType.Hero, displayName = "BHERO S",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.BSC
                },
                new() {
                    code = 2, sortOrder = 2, tokenName = BlockRewardType.Hero, displayName = "BHERO S",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.POLYGON
                },
                new() {
                    code = 3, tokenName = BlockRewardType.Key, displayName = "Key",
                    networkSymbol = DataType.TR
                },
                new() {
                    code = 4, sortOrder = 4, tokenName = BlockRewardType.BCoinDeposited, displayName = "BCOIN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.BSC
                },
                new() {
                    code = 4, sortOrder = 5, tokenName = BlockRewardType.BCoinDeposited, displayName = "BCOIN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.POLYGON
                },
                new() {
                    code = 4, sortOrder = 4, tokenName = BlockRewardType.BCoinDeposited, displayName = "BCOIN DEPOSIT",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.TON
                },
                new() {
                    code = 4, sortOrder = 4, tokenName = BlockRewardType.BCoinDeposited, displayName = "BCOIN DEPOSIT",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.SOL
                },
                new() {
                    code = 7, sortOrder = 0, tokenName = BlockRewardType.Senspark, displayName = "SEN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, useTax = true,
                    minValueToClaim = 40, networkSymbol = DataType.BSC
                },
                new() {
                    code = 7, sortOrder = 0, tokenName = BlockRewardType.Senspark, displayName = "SEN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, useTax = true,
                    minValueToClaim = 40, networkSymbol = DataType.POLYGON
                },
                new() {
                    code = 8, sortOrder = 3, tokenName = BlockRewardType.SensparkDeposited, displayName = "SEN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.BSC
                },
                new() {
                    code = 8, sortOrder = 3, tokenName = BlockRewardType.SensparkDeposited, displayName = "SEN",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.POLYGON
                },
                new() {
                    code = 11, tokenName = BlockRewardType.BossTicket, displayName = "Boss Hunter Ticket",
                    networkSymbol = DataType.TR
                },
                new() {
                    code = 12, tokenName = BlockRewardType.PvpTicket, displayName = "Pvp Ticket",
                    networkSymbol = DataType.TR
                },
                new() {
                    code = 13, tokenName = BlockRewardType.NftPvP, displayName = "Nft PvP",
                    networkSymbol = DataType.TR
                },
                new() {
                    code = 14, tokenName = BlockRewardType.Gem, displayName = "Gem",
                    networkSymbol = DataType.TR
                },
                new() {
                    code = 17, tokenName = BlockRewardType.BLGold, displayName = "Gold",
                    networkSymbol = DataType.TR
                },
                new() {
                    code = 18, sortOrder = 6, tokenName = BlockRewardType.BLCoin, displayName = "STAR CORE",
                    displayOnLaunchPad = true, alwaysDisplay = true, networkSymbol = DataType.TR
                },
                new() {
                    code = 2, sortOrder = 2, tokenName = BlockRewardType.Hero, displayName = "BHERO",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.TON
                },
                new() {
                    code = 24, sortOrder = 5, tokenName = BlockRewardType.TonDeposited, displayName = "TON DEPOSIT",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.TON
                },
                new() {
                    code = 2, sortOrder = 2, tokenName = BlockRewardType.Hero, displayName = "BHERO",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.SOL
                },
                new() {
                    code = 25, sortOrder = 5, tokenName = BlockRewardType.SolDeposited, displayName = "SOL DEPOSIT",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.SOL
                },
                new() {
                    code = 2, sortOrder = 2, tokenName = BlockRewardType.Hero, displayName = "BHERO",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.RON
                },
                new() {
                    code = 26, sortOrder = 5, tokenName = BlockRewardType.RonDeposited, displayName = "RON DEPOSIT",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.RON
                },
                new() {
                    code = 26, sortOrder = 6, tokenName = BlockRewardType.BLCoin, displayName = "STAR CORE",
                    displayOnLaunchPad = true, alwaysDisplay = true, networkSymbol = DataType.RON
                },
                new() {
                    code = 2, sortOrder = 2, tokenName = BlockRewardType.Hero, displayName = "BHERO",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.BAS
                },
                new() {
                    code = 27, sortOrder = 5, tokenName = BlockRewardType.BasDeposited, displayName = "ETH DEPOSIT",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.BAS
                },
                new() {
                    code = 27, sortOrder = 6, tokenName = BlockRewardType.BLCoin, displayName = "STAR CORE",
                    displayOnLaunchPad = true, alwaysDisplay = true, networkSymbol = DataType.BAS
                },
                new() {
                    code = 2, sortOrder = 2, tokenName = BlockRewardType.Hero, displayName = "BHERO",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableClaim = true, minValueToClaim = 1,
                    networkSymbol = DataType.VIC
                },
                new() {
                    code = 28, sortOrder = 5, tokenName = BlockRewardType.VicDeposited, displayName = "VIC DEPOSIT",
                    displayOnLaunchPad = true, alwaysDisplay = true, enableDeposit = true, minValueToClaim = 1,
                    networkSymbol = DataType.VIC
                },
                new() {
                    code = 28, sortOrder = 6, tokenName = BlockRewardType.BLCoin, displayName = "STAR CORE",
                    displayOnLaunchPad = true, alwaysDisplay = true, networkSymbol = DataType.VIC
                },
            };
        }
    }
}
