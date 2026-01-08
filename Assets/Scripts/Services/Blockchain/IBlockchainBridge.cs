using System.Threading.Tasks;

namespace App {
    public interface IBlockchainBridge {
        Task<double> GetBalance(RpcTokenCategory category, string walletAddress);
        Task<double> GetSensparkBalance(string walletAddress);
        Task<double> GetUsdtBalance(string walletAddress);
        Task<int> GetHeroIdCounter();
        Task<int> GetHeroLimit();
        Task<BHeroPrice> GetHeroPrice();
        Task<double[,]> GetHeroUpgradeCost();
        Task<AbilityDesign[]> GetHeroAbilityDesigns();
        Task<int> GetClaimableHero(string walletAddress);
        Task<int> GetGiveAwayHero(string walletAddress);
        Task<ProcessToken> GetPendingHero(string walletAddress);
        Task<bool> BuyHero(string walletAddress, int count, BuyHeroCategory category, bool isHeroS);
        Task<bool> UpgradeHero(string walletAddress, int baseId, int materialId);
        Task<bool> ClaimHero(string walletAddress);
        Task<bool> ClaimGiveAwayHero();
        Task<HeroProcessTokenResult> ProcessTokenRequests(string walletAddress);
        Task<bool> HasPendingHeroRandomization(int heroId);
        Task<bool> RandomizeHeroAbilities(string walletAddress, int heroId);
        Task<bool> ProcessHeroRandomizeAbilities(string walletAddress, int heroId);
        Task<bool> IsSuperBoxEnabled();
        Task<int> GetHouseLimit();
        Task<double[]> GetHousePrice();
        Task<int[]> GetAvailableHouse();
        Task<int[]> GetHouseMintLimits();
        Task<HouseStats[]> GetHouseStats();
        Task<bool> BuyHouse(string walletAddress, int rarity);
        Task<bool> Deposit(string walletAddress, int amount, int category);
        Task<bool> FusionHero(int[] heroIds);
        Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds);
        Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn);
        Task<bool> GetNFT(int amount, int eventId, int nonce, string signature);
        Task<string> ClaimToken(int tokenType, double amount, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations);
        Task<int> GetRockAmount(string walletAddress);
        Task<string> CreateRock(int[] idHeroesBurn);
        Task<bool> RepairShieldWithRock(int idHeroS, int amountRock);
        Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock);
        Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature);
        Task<bool> CanUseVoucher(int voucherType, string walletAddress);
        Task<bool> BuyHeroUseVoucher(string walletAddress, string tokenPay, int voucherType, int heroQuantity,
            string amount, int nonce, string signature);
        
        Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category, string walletAddress);
        Task<ExchangeInfo> Exchange_GetInfo();
        
        Task<bool> StakeToHero(string walletAddress, int id, double amount, string tokenAddress, StakeHeroCategory category);
        Task<bool> WithDrawFromHeroId(int id, double amount, string tokenAddress);
        Task<double> GetStakeFromHeroId(int id, string tokenAddress);
        Task<double> GetFeeFromHeroId(int id, string tokenAddress);
        Task<bool> DepositTon(string invoice, double amount);
        Task<bool> DepositAirdrop(string invoice, string amount, string chainId);
    }
}