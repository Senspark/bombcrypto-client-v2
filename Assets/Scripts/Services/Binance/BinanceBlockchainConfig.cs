using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace App {
    public class BinanceBlockchainConfig : IBlockchainConfig {
        public string Chain => "binance";
        public int NetworkId => _production ? 56 : 97;
        public string Network => _production ? "mainnet" : "testnet";

        private readonly bool _production;

        public string CoinTokenAddress => _production
            ? "0x00e1656e45f18ec6747F5a8496Fd39B50b38396D"
            : "";

        public string SensparkTokenAddress => _production
            ? "0xb43Ac9a81eDA5a5b36839d5b6FC65606815361b0"
            : "";

        public string UsdtTokenAddress => null;

        public string HeroTokenAddress => _production
            ? "0x30cc0553f6fa1faf6d7847891b9b36eb559dc618"
            : "";

        public string HeroSTokenAddress => _production
            ? "0x9fb9b7349279266c85c0C9dd264D71d2a4B79AB4"
            : "";

        public string HeroStakeAddress => _production
            ? "0x053282c295419E67655a5032A4DA4e3f92D11F17"
            : "";

        public string HeroExtendedAddress => _production
            ? "0x1f3EE5a5a153e5a30C65a82Efd7598Fd32bBF507"
            : "";

        public string HouseTokenAddress => _production
            ? "0xea3516fEB8F3e387eeC3004330Fd30Aff615496A"
            : "";

        public string DepositAddress => _production
            ? "0xad5669fD304aF930C04B5bc7541e5285b638169d"
            : "";

        public string AirDropAddress => _production
            ? "0x4b70D3Cd925b21363DB045F9a8B0cf4B16937CeA"
            : "";

        public string ClaimManagerAddress => _production
            ? "0x39328612EC8A6C45b490D524b1C103ACC32f6b6d"
            : "";

        public string CoinExchangeAddress => null;

        public string BirthdayEventAddress => _production
            ? "0x65FDF6550C422a80222E9343a0D12C223c3EE4c5"
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

        public BinanceBlockchainConfig(bool production) {
            _production = production;
        }
        
        

        public async UniTask LoadAbi() {
            const string nullObj = "{}";

            CoinTokenAbi = await AbiResourceLoader.GetAbi("CoinTokenAbi");
            SensparkTokenAbi = await AbiResourceLoader.GetAbi("SensparkTokenAbi");
            HeroTokenAbi = await AbiResourceLoader.GetAbi("HeroTokenAbi");
            HeroSTokenAbi = await AbiResourceLoader.GetAbi("HeroSTokenAbi");
            HeroExtendedAbi = await AbiResourceLoader.GetAbi("HeroExtendedAbi");
            HouseTokenAbi = await AbiResourceLoader.GetAbi("HouseTokenAbi");
            HeroDesignAbi = await AbiResourceLoader.GetAbi("HeroDesignAbi");
            HouseDesignAbi = await AbiResourceLoader.GetAbi("HouseDesignAbi");
            DepositAbi = await AbiResourceLoader.GetAbi("DepositAbi");
            AirDropAbi = await AbiResourceLoader.GetAbi("AirDropAbi");
            ClaimManagerAbi = await AbiResourceLoader.GetAbi("ClaimManagerAbi");
            HeroStakeAbi = await AbiResourceLoader.GetAbi("HeroStakeAbi");

            CoinExchangeAbi = nullObj;
            BirthdayEventAbi = nullObj;
        }
    }
}