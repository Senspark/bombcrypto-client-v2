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
        Task<ClaimAndProcessResult> ClaimToken(int tokenType, double amount, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations, string walletAddress);
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
        
        Task<StakeResult> StakeToHero(string walletAddress, int id, double amount, StakeHeroCategory category);
        Task<StakeResult> WithDrawFromHeroId(int id, double amount, StakeHeroCategory category);
        Task<double> GetStakeFromHeroId(int id, StakeHeroCategory category);
        Task<double> GetFeeFromHeroId(int id, StakeHeroCategory category);
        Task<bool> DepositTon(string invoice, double amount);
        Task<bool> DepositAirdrop(string invoice, string amount, string chainId);

        Task<string> GetBridgeDeposited(string chain, string walletAddress, string token);
        Task<string> GetBridgeWithdrawn(string chain, string walletAddress, string token);
        Task<bool> GetBridgeDepositEnabled(string chain);
        Task<bool> GetBridgeWithdrawEnabled(string chain);
        Task<BridgeTxResult> BridgeDeposit(string chain, string walletAddress, string token, string amountWei);
        Task<BridgeTxResult> BridgeWithdraw(string chain, string walletAddress, string token, string otherDeposited,
            long deadline, string signature);

        // Native BNB / POL vault. deposit() is payable and withdraw is msg.sender-bound, so the signer
        // wallet is implicit — no walletAddress param (unlike the ERC20 bridge reads).
        Task<double> GetNativeWalletBalance(string chain, string walletAddress);
        Task<bool> GetNativeDepositEnabled(string chain);
        Task<bool> GetNativeWithdrawEnabled(string chain);
        Task<BridgeTxResult> NativeDeposit(string chain, string amountWei);
        Task<BridgeTxResult> NativeWithdraw(string chain, string allowedCumulative, long deadline, string signature);
    }
}