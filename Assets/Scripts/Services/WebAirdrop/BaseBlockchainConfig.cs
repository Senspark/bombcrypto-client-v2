using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace App {
    public class BaseBlockchainConfig : IBlockchainConfig {
        public UniTask LoadAbi() {
            return UniTask.CompletedTask;
        }

        public string Chain => NetworkTypeInServer.BAS.ToString();
        public int NetworkId => 0;
        public string Network => _production ? "mainnet" : "testnet";

        private readonly bool _production;
        private bool _isReady;

        public string CoinTokenAddress => null;
        public string SensparkTokenAddress => null;
        public string UsdtTokenAddress => null;
        public string HeroTokenAddress => null;
        public string HeroSTokenAddress => null;
        public string HeroStakeAddress => null;
        public string HeroExtendedAddress => null;
        public string HouseTokenAddress => null;
        public string DepositAddress => null;
        public string AirDropAddress => null;
        public string ClaimManagerAddress => null;
        public string CoinExchangeAddress => null;
        public string BirthdayEventAddress => null;
        public string CoinTokenAbi  => null;
        public string SensparkTokenAbi  => null;
        public string HeroTokenAbi => null;
        public string HeroSTokenAbi => null;
        public string HeroExtendedAbi => null;
        public string HouseTokenAbi => null;
        public string HeroDesignAbi => null;
        public string HouseDesignAbi => null;
        public string DepositAbi => null;
        public string AirDropAbi => null;
        public string ClaimManagerAbi => null;
        public string CoinExchangeAbi => null;
        public string BirthdayEventAbi => null;
        public string HeroStakeAbi => null;

        public BaseBlockchainConfig(bool production) {
            _production = production;
            _isReady = true;
        }
        
        public Task WaitForReady() {
            return Task.FromResult(true);
        }
    }
}