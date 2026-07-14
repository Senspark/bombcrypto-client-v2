using Game.UI.Information;

namespace App {
    public static class InformationTable {
        public static InformationData[] Build() {
            return new[] {
                new InformationData {
                    displayName = "BCOIN",
                    code = new[] { "BCOIN" },
                    network = "BSC",
                    content =
                        "1. You must have at least 40 BCOIN to claim.\n" +
                        "2. Must have at least 5 BCOIN in the Wallet to be able to claim.\n\n" +
                        "Claim under 60 BCOIN pays 10% tax.\n" +
                        "Claim 60 to less than 80 BCOIN pays 6% tax.\n" +
                        "Claim over 80 BCOIN pays 3% tax.\n\n" +
                        "BCOIN can only be withdrawn/deposited while logging in to the BNB network\n"
                },
                new InformationData {
                    displayName = "BCOIN Deposit",
                    code = new[] { "BCOIN_DEPOSITED" },
                    network = "BSC",
                    content =
                        "You can claim BCOIN in deposit without fees, observe the following rules:\n" +
                        "1. Must have at least 5 BCOIN in the wallet.\n" +
                        "2. Each claim must be at least 1 BCOIN.\n\n" +
                        "BCOIN can only be withdrawn/deposited while logging in to the BNB network\n"
                },
                new InformationData {
                    displayName = "SEN",
                    code = new[] { "SENSPARK" },
                    network = "BSC",
                    content =
                        "1. You must have at least 40 SEN to claim.\n" +
                        "2. Must have at least 5 SEN in the Wallet to be able to claim.\n\n" +
                        "Claim under 60 SEN pays 10% tax.\n" +
                        "Claim 60 to less than 80 SEN pays 6% tax.\n" +
                        "Claim over 80 SEN pays 3% tax.\n\n" +
                        "SEN can only be withdrawn/deposited while logging in to the BNB network\n"
                },
                new InformationData {
                    displayName = "SEN Deposit",
                    code = new[] { "SENSPARK_DEPOSITED" },
                    network = "BSC",
                    content =
                        "You can claim SEN in deposit without fees, observe the following rules:\n" +
                        "1. Must have at least 5 SEN in the wallet.\n" +
                        "2. Each claim must be at least 1 SEN.\n\n" +
                        "SEN can only be withdrawn/deposited while logging in to the BNB network\n"
                },
                new InformationData {
                    displayName = "SEN",
                    code = new[] { "SENSPARK" },
                    network = "POLYGON",
                    content =
                        "1. You must have at least 40 SEN to claim.\n" +
                        "2. Must have at least 5 SEN in the Wallet to be able to claim.\n\n" +
                        "Claim under 60 SEN pays 10% tax.\n" +
                        "Claim 60 to less than 80 SEN pays 6% tax.\n" +
                        "Claim over 80 SEN pays 3% tax.\n\n" +
                        "SEN can only be withdrawn/deposited while logging in to the POLYGON network\n"
                },
                new InformationData {
                    displayName = "SEN Deposit",
                    code = new[] { "SENSPARK_DEPOSITED" },
                    network = "POLYGON",
                    content =
                        "You can claim SEN in deposit without fees, observe the following rules:\n" +
                        "1. Must have at least 5 SEN in the wallet.\n" +
                        "2. Each claim must be at least 1 SEN.\n\n" +
                        "SEN can only be withdrawn/deposited while logging in to the POLYGON network\n"
                },
                new InformationData {
                    displayName = "STAR CORE",
                    code = new[] { "COIN" },
                    network = "TR",
                    content =
                        "1. STAR CORE is obtained from Treasure Hunt mode of Bombcrypto BNB and Polygon network.\n" +
                        "2. STAR CORE is used to buy Chests in the SHOP\n"
                },
                new InformationData {
                    displayName = "BCOIN",
                    code = new[] { "BCOIN" },
                    network = "POLYGON",
                    content =
                        "1. You must have at least 40 BCOIN to claim.\n" +
                        "2. Must have at least 5 BCOIN in the Wallet to be able to claim.\n\n" +
                        "Claim under 60 BCOIN pays 10% tax.\n" +
                        "Claim 60 to less than 80 BCOIN pays 6% tax.\n" +
                        "Claim over 80 BCOIN pays 3% tax.\n\n" +
                        "BCOIN can only be withdrawn/deposited while logging in to the Polygon network\n"
                },
                new InformationData {
                    displayName = "BCOIN Deposit",
                    code = new[] { "BCOIN_DEPOSITED" },
                    network = "POLYGON",
                    content =
                        "You can claim BCOIN in deposit without fees, observe the following rules:\n" +
                        "1. Must have at least 5 BCOIN in the wallet.\n" +
                        "2. Each claim must be at least 1 BCOIN.\n\n" +
                        "BCOIN can only be withdrawn/deposited while logging in to the Polygon network\n"
                },
                new InformationData {
                    displayName = "BHero S",
                    code = new[] { "BOMBERMAN" },
                    network = "POLYGON",
                    content =
                        "BHero S is used for mining tokens in Treasure Hunt mode (Polygon Network).\n" +
                        "BHero S can only be withdrawn while logging in to the Polygon network.\n"
                },
                new InformationData {
                    displayName = "BHero S",
                    code = new[] { "BOMBERMAN" },
                    network = "BSC",
                    content =
                        "BHero S is used for mining tokens in Treasure Hunt mode (BNB Chain network).\n" +
                        "BHero S can only be withdrawn while logging in to the BNB Chain network.\n"
                },
                new InformationData {
                    displayName = "Auto Mine",
                    code = new[] { "AUTOMINE" },
                    network = null,
                    content =
                        "Auto Mine is the feature that let Heroes automatically go Home or Rest then go back to work.\n\n" +
                        "Price of 2 Auto Mine packages:\n" +
                        "7 Days = 12% of total tokens mined in the last 7 days (Minimum cost = 10 BCOIN).\n" +
                        "30 Days = 35% of total token mined in the last 7 days (Minumum cost = 30 BCOIN)."
                },
                new InformationData {
                    displayName = "Auto Mine",
                    code = new[] { "AUTOMINE_TON" },
                    network = null,
                    content =
                        "Auto Mine is the feature that let Heroes automatically go Home or Rest then go back to work.\n\n" +
                        "Auto mine will also let your Heroes mine while offline more than 1 hours. Maximum 48 hours\n\n" +
                        "Price of 2 Auto Mine packages:\n\n" +
                        "7 Days: 0.075 TON Deposit\n\n" +
                        "30 Days: 0.3 TON Deposit"
                },
                new InformationData {
                    displayName = "Stake",
                    code = new[] { "STAKE" },
                    network = null,
                    content =
                        "- Only S Heroes can mine Base and Ranking Rewards.\n" +
                        "- Stake BCOIN into Legacy Heroes base on their rarity \n" +
                        "to upgrade them into S Heroes.\n" +
                        "- S Heroes can mine Star Core and tokens corresponding to the tokens staked in this hero."
                },
                new InformationData {
                    displayName = "BCOIN Deposit",
                    code = new[] { "BCOIN_DEPOSITED" },
                    network = "SOL",
                    content = "Minimum deposit amount 27"
                },
                new InformationData {
                    displayName = "SOL Deposit",
                    code = new[] { "SOL_DEPOSITED" },
                    network = "SOL",
                    content = "Minimum deposit amount 0.005"
                },
                new InformationData {
                    displayName = "Bhero",
                    code = new[] { "BOMBERMAN" },
                    network = "SOL",
                    content = "BHeroes are used for mining STAR CORE in Treasure Hunt mode"
                },
                new InformationData {
                    displayName = "Auto Mine",
                    code = new[] { "AUTOMINE_SOL" },
                    network = null,
                    content =
                        "Auto Mine is the feature that let Heroes automatically go Home or Rest then go back to work.\n\n" +
                        "Price of 2 Auto Mine packages:\n\n" +
                        "7 Days: 0.0023 SOL Deposit\n\n" +
                        "30 Days: 0.0092 SOL Deposit"
                },
                new InformationData {
                    displayName = "Auto Mine",
                    code = new[] { "AUTOMINE_RON" },
                    network = null,
                    content =
                        "Auto Mine is the feature that let Heroes automatically go Home or Rest then go back to work.\n\n" +
                        "Price of 2 Auto Mine packages:\n\n" +
                        "7 Days: 0.8 RON Deposit\n\n" +
                        "30 Days: 3.1 RON Deposit"
                },
                new InformationData {
                    displayName = "Auto Mine",
                    code = new[] { "AUTOMINE_BAS" },
                    network = null,
                    content =
                        "Auto Mine is the feature that let Heroes automatically go Home or Rest then go back to work.\n\n" +
                        "Price of 2 Auto Mine packages:\n\n" +
                        "7 Days: 0.000214 ETH Deposit\n\n" +
                        "30 Days: 0.00086 ETH Deposit"
                },
                new InformationData {
                    displayName = "Auto Mine",
                    code = new[] { "AUTOMINE_VIC" },
                    network = null,
                    content =
                        "Auto Mine is the feature that let Heroes automatically go Home or Rest then go back to work.\n\n" +
                        "Price of 2 Auto Mine packages:\n\n" +
                        "7 Days: 2.3 VIC Deposit\n\n" +
                        "30 Days: 9.2 VIC Deposit"
                },
                new InformationData {
                    displayName = "TON DEPOSIT",
                    code = new[] { "TON_DEPOSITED" },
                    network = null,
                    content = "Minimum deposit amount 0.2."
                },
                new InformationData {
                    displayName = "Bhero",
                    code = new[] { "BOMBERMAN" },
                    network = "TON",
                    content = "BHeroes are used for mining STAR CORE in Treasure Hunt mode."
                },
                new InformationData {
                    displayName = "BCOIN Deposit",
                    code = new[] { "BCOIN_DEPOSITED" },
                    network = "TON",
                    content = "Minimum deposit amount 35."
                },
                new InformationData {
                    displayName = "Bhero",
                    code = new[] { "BOMBERMAN" },
                    network = "RON",
                    content = "BHeroes are used for mining STAR CORE in Treasure Hunt mode."
                },
                new InformationData {
                    displayName = "RON Deposit",
                    code = new[] { "RON_DEPOSITED" },
                    network = "RON",
                    content = "Minimum deposit amount 2"
                },
                new InformationData {
                    displayName = "Bhero",
                    code = new[] { "BOMBERMAN" },
                    network = "BAS",
                    content = "BHeroes are used for mining STAR CORE in Treasure Hunt mode."
                },
                new InformationData {
                    displayName = "ETH Deposit",
                    code = new[] { "BAS_DEPOSITED" },
                    network = "BAS",
                    content = "Minimum deposit amount 0.00047"
                },
                new InformationData {
                    displayName = "Bhero",
                    code = new[] { "BOMBERMAN" },
                    network = "VIC",
                    content = "BHeroes are used for mining STAR CORE in Treasure Hunt mode."
                },
                new InformationData {
                    displayName = "VIC Deposit",
                    code = new[] { "VIC_DEPOSITED" },
                    network = "VIC",
                    content = "Minimum deposit amount 5"
                },
                new InformationData {
                    displayName = "BCOIN BRIDGE",
                    code = new[] { "BCOIN_BRIDGE" },
                    network = "BP",
                    content =
                        "You can claim BCOIN in deposit with 5% fees\n\n" +
                        "BCOIN can be withdrawn/deposited in BNB or POLYGON network\n"
                },
                new InformationData {
                    displayName = "SEN BRIDGE",
                    code = new[] { "SEN_BRIDGE" },
                    network = "BP",
                    content =
                        "You can claim SEN in deposit with 5% fees\n\n" +
                        "SEN can be withdrawn/deposited in BNB or POLYGON network\n"
                },
            };
        }
    }
}
