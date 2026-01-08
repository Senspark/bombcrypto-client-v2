using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

namespace App {
    public interface IBlockchainConfig {
        UniTask LoadAbi();
        string Chain { get; }
        int NetworkId { get; }
        string Network { get; }
        
        string CoinTokenAddress { get; }
        string SensparkTokenAddress { get; }
        string UsdtTokenAddress { get; }
        string HeroTokenAddress { get; }
        string HeroSTokenAddress { get; }
        string HeroExtendedAddress { get; }
        string HouseTokenAddress { get; }
        string DepositAddress { get; }
        string AirDropAddress { get; }
        string ClaimManagerAddress { get; }
        string CoinExchangeAddress { get; }
        string HeroStakeAddress { get; }


        string CoinTokenAbi { get; }
        string SensparkTokenAbi { get; }
        string HeroTokenAbi { get; }
        string HeroSTokenAbi { get; }
        string HeroExtendedAbi { get; }
        string HouseTokenAbi { get; }
        string HeroDesignAbi { get; }
        string HouseDesignAbi { get; }
        string DepositAbi { get; }
        string AirDropAbi { get; }
        string ClaimManagerAbi { get; }
        string CoinExchangeAbi { get; }
        public string HeroStakeAbi { get; }
    }
}