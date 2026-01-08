using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace App {
    public class PolygonBlockchainConfig : IBlockchainConfig {

        public string Chain => "polygon";
        public int NetworkId => _production ? 137 : 80002;
        public string Network => _production ? "mainnet" : "testnet";

        private readonly bool _production;

        public string CoinTokenAddress => _production
            ? "0xB2C63830D4478cB331142FAc075A39671a5541dC"
            : "";

        public string SensparkTokenAddress => _production
            ? "0xFe302B8666539d5046cd9aA0707bB327F5f94C22"
            : "";

        public string UsdtTokenAddress => _production
            ? "0xc2132D05D31c914a87C6611C10748AEb04B58e8F"
            : null;

        public string HeroTokenAddress => _production
            ? "0xd8a06936506379dbBe6e2d8aB1D8C96426320854"
            : "";

        public string HeroSTokenAddress => _production
            ? "0x27313635E6B7AA3CC8436E24BE2317D4A0e56BeB"
            : "";

        public string HeroExtendedAddress => null;

        public string HouseTokenAddress => _production
            ? "0x2d5F4Ba3E4a2D991bD72EdBf78F607C174636618"
            : "";

        public string DepositAddress => _production
            ? "0x14EDbb72bd3318F84345bbe816bDef37814AC568"
            : "";

        public string AirDropAddress => null;

        public string ClaimManagerAddress => _production
            ? "0x83B5E78c10257bb4990Eba73E00BbC20c5581745"
            : "";

        public string CoinExchangeAddress => _production
            ? "0x700619afcC6400024dc7Cd3A96A5bFd80637c02D"
            : null;

        public string HeroStakeAddress => _production
            ? "0x810570aa7e16cf14defd69d4c9796f3c1abe2d13"
            : "";

        public string CoinTokenAbi { get; private set; }
        public string SensparkTokenAbi { get; private set; }
        public string HeroTokenAbi { get; private set; }
        public string HeroSTokenAbi { get; private set; }
        public string HeroExtendedAbi { get; private set; }
        public string HouseTokenAbi { get; private set; }
        public string HeroDesignAbi { get; private set; }
        public string HouseDesignAbi { get; private set; }
        public string DepositAbi { get; private set; }
        public string AirDropAbi { get; private set; }
        public string ClaimManagerAbi { get; private set; }
        public string CoinExchangeAbi { get; private set; }
        public string BirthdayEventAbi { get; private set; }
        public string HeroStakeAbi { get; private set; }

        public PolygonBlockchainConfig(bool production) {
            _production = production;
        }
        
        
        public async UniTask LoadAbi() {
            const string nullObj = "{}";

            CoinTokenAbi = await AbiResourceLoader.GetAbi("CoinTokenAbi");
            HeroTokenAbi = await AbiResourceLoader.GetAbi("HeroTokenAbi");
            HeroSTokenAbi = await AbiResourceLoader.GetAbi("HeroSTokenAbi");
            HouseTokenAbi = await AbiResourceLoader.GetAbi("HouseTokenAbi");
            HeroDesignAbi = await AbiResourceLoader.GetAbi("HeroDesignAbi");
            HouseDesignAbi = await AbiResourceLoader.GetAbi("HouseDesignAbi");
            DepositAbi = await AbiResourceLoader.GetAbi("DepositAbi");
            ClaimManagerAbi = await AbiResourceLoader.GetAbi("ClaimManagerAbi");
            HeroStakeAbi = await AbiResourceLoader.GetAbi("HeroStakeAbi");

            SensparkTokenAbi = nullObj;
            HeroExtendedAbi = nullObj;
            AirDropAbi = nullObj;
            CoinExchangeAbi = nullObj;
            BirthdayEventAbi = nullObj;
        }
    }
}