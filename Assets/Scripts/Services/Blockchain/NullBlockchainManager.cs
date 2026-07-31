using System;
using System.Threading.Tasks;

namespace App {
    public class NullBlockchainManager : IBlockchainManager {
        public virtual Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public virtual Task<double> GetBalance(RpcTokenCategory category) {
            return Task.FromResult(100d);
        }

        public virtual Task<int> GetHeroIdCounter() {
            return Task.FromResult(0);
        }

        public virtual Task<int> GetHeroLimit() {
            return Task.FromResult(500);
        }

        public virtual Task<BHeroPrice> GetHeroPrice() {
            return Task.FromResult(new BHeroPrice(45, 10, 10, 10, 10));
        }

        public virtual Task<double[,]> GetHeroUpgradeCost() {
            return Task.FromResult(new double[6, 4]);
        }

        public virtual Task<AbilityDesign[]> GetHeroAbilityDesigns() {
            var design = new AbilityDesign[6];
            Array.Fill(design, new AbilityDesign {
                MinCost = 5,
                MaxCost = 10,
                IncrementalCost = 1,
            });
            return Task.FromResult(design);
        }

        public virtual Task<int> GetClaimableHero() {
            return Task.FromResult(0);
        }

        public virtual Task<int> GetGiveAwayHero() {
            return Task.FromResult(0);
        }

        public virtual Task<ProcessToken> GetPendingHero() {
            return Task.FromResult(new ProcessToken {
                pendingHeroes = 0,
                pendingHeroesFusion = 0,
            });
        }

        public virtual Task<bool> BuyHero(int count, BuyHeroCategory category, bool isHeroS) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> UpgradeHero(int baseId, int materialId) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> ClaimHero() {
            return Task.FromResult(true);
        }

        public virtual Task<bool> ClaimGiveAwayHero() {
            return Task.FromResult(true);
        }

        public virtual Task<HeroProcessTokenResult> ProcessTokenRequests() {
            return Task.FromResult(new HeroProcessTokenResult());
        }

        public virtual Task<bool> HasPendingHeroRandomization(int heroId) {
            return Task.FromResult(false);
        }

        public virtual Task<bool> RandomizeHeroAbilities(int heroId) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> ProcessHeroRandomizeAbilities(int heroId) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> IsSuperBoxEnabled() {
            return Task.FromResult(true);
        }

        public virtual Task<int> GetHouseLimit() {
            return Task.FromResult(0);
        }

        public virtual Task<double[]> GetHousePrice() {
            return Task.FromResult(new double[6]);
        }

        public virtual Task<int[]> GetAvailableHouse() {
            return Task.FromResult(new int[6]);
        }

        public virtual Task<int[]> GetHouseMintLimits() {
            return Task.FromResult(new int[6]);
        }

        public virtual Task<HouseStats[]> GetHouseStats() {
            var result = new HouseStats[6];
            result[0] = new HouseStats { Capacity = 4, Recovery = 120 };
            result[1] = new HouseStats { Capacity = 6, Recovery = 300 };
            result[2] = new HouseStats { Capacity = 8, Recovery = 480 };
            result[3] = new HouseStats { Capacity = 10, Recovery = 660 };
            result[4] = new HouseStats { Capacity = 12, Recovery = 840 };
            result[5] = new HouseStats { Capacity = 14, Recovery = 1020 };
            return Task.FromResult(result);
        }

        public virtual Task<bool> BuyHouse(int rarity) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> Deposit(int amount, int category) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> FusionHero(int[] heroIds) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> GetNFT(int amount, int eventId, int nonce, string signature) {
            return Task.FromResult(true);
        }

        public virtual Task<ClaimAndProcessResult> ClaimToken(double amount, int tokenType, int nonce, string[] details,
            string signature, string formatType, int waitConfirmations) {
            return Task.FromResult(new ClaimAndProcessResult { txHash = "", processResult = null });
        }

        public virtual Task<int> GetRockAmount() {
            return Task.FromResult(1000);
        }

        public virtual Task<string> CreateRock(int[] idHeroesBurn) {
            return Task.FromResult("");
        }

        public virtual Task<bool> RepairShieldWithRock(int idHeroS, int amountRock) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> CanUseVoucher(int voucherType) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> BuyHeroUseVoucher(string tokenPay, int voucherType, int heroQuantity, string amount,
            int nonce, string signature) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category) {
            return Task.FromResult(true);
        }

        public virtual Task<ExchangeInfo> Exchange_GetInfo() {
            return Task.FromResult(new ExchangeInfo { price = 0.35, slippage = 0.55, fee = 0.55 });
        }

        public virtual Task<StakeResult> StakeToHero(int id, double amount, StakeHeroCategory category) {
            return Task.FromResult(new StakeResult { success = true, txHash = "" });
        }

        public virtual Task<StakeResult> WithDrawFromHeroId(int id, double amount, StakeHeroCategory category) {
            return Task.FromResult(new StakeResult { success = true, txHash = "" });
        }

        public virtual Task<double> GetStakeFromHeroId(int id, StakeHeroCategory category) {
            return Task.FromResult(0.0);
        }

        public virtual Task<double> GetFeeFromHeroId(int id, StakeHeroCategory category) {
            return Task.FromResult(0.0);
        }

        public virtual Task<bool> DepositTon(string invoice, double amount) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> DepositAirdrop(string invoice, string amount, string chainId) {
            return Task.FromResult(true);
        }

        public virtual Task<string> GetBridgeDeposited(string chain, string token) {
            return Task.FromResult("0");
        }

        public virtual Task<string> GetBridgeWithdrawn(string chain, string token) {
            return Task.FromResult("0");
        }

        public virtual void InvalidateBridgeRead(string chain, string token) {
        }

        public virtual Task<bool> GetBridgeDepositEnabled(string chain) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> GetBridgeWithdrawEnabled(string chain) {
            return Task.FromResult(true);
        }

        public virtual Task<BridgeTxResult> BridgeDeposit(string chain, string token, string amountWei) {
            return Task.FromResult(new BridgeTxResult { success = true, txHash = "" });
        }

        public virtual Task<BridgeTxResult> BridgeWithdraw(string chain, string token, string otherDeposited,
            long deadline, string signature) {
            return Task.FromResult(new BridgeTxResult { success = true, txHash = "" });
        }

        public virtual Task<double> GetNativeWalletBalance(string chain) {
            return Task.FromResult(0d);
        }

        public virtual Task<bool> GetNativeDepositEnabled(string chain) {
            return Task.FromResult(true);
        }

        public virtual Task<bool> GetNativeWithdrawEnabled(string chain) {
            return Task.FromResult(true);
        }

        public virtual Task<BridgeTxResult> NativeDeposit(string chain, string amountWei) {
            return Task.FromResult(new BridgeTxResult { success = true, txHash = "" });
        }

        public virtual Task<BridgeTxResult> NativeWithdraw(string chain, string allowedCumulative, long deadline,
            string signature) {
            return Task.FromResult(new BridgeTxResult { success = true, txHash = "" });
        }
    }
}
