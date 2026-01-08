using System;
using System.Threading.Tasks;

using Senspark;

using UnityEngine;

#if UNITY_EDITOR || UNITY_WEBGL
namespace App {
    public class WebGLBlockchainManager : IBlockchainManager {
        private readonly IAccountManager _accountManager;
        private readonly ILogManager _logManager;
        private readonly IBlockchainBridge _bridge;
        private readonly IApiManager _apiManager;
        private readonly WalletType _walletType;
        private readonly NetworkType _networkType;
        private readonly bool _isProduction;

        public WebGLBlockchainManager(
            IAccountManager accountManager,
            ILogManager logManager,
            IBlockchainBridge bridge,
            IApiManager apiManager,
            NetworkType networkType,
            WalletType walletType,
            bool production) {
            _accountManager = accountManager;
            _logManager = logManager;
            _apiManager = apiManager;
            _bridge = bridge;
            _networkType = networkType;
            _walletType = walletType;
            _isProduction = production;
        }

        public async Task<bool> Initialize() {
            // Ko cần init network nữa do sử dụng kiểu mới của solana
            //await WebGLBlockchainInitializer.InitNetworkConfig(_walletType);
            //await WebGLBlockchainInitializer.InitBlockchainConfig(_networkType, _isProduction);
            return true;
        }
        
        public void Destroy() {
        }

        public Task<double> GetBalance(RpcTokenCategory category) {
            return _bridge.GetBalance(category, _accountManager.Account);
        }

        public async Task<double> GetCoinBalance() {
            _logManager.Log();
            try {
                var result = await _apiManager.GetCoinBalance(_accountManager.Account);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<double> GetSensparkBalance() {
            return _bridge.GetSensparkBalance(_accountManager.Account);
        }

        public Task<double> GetUsdtBalance() {
            return _bridge.GetUsdtBalance(_accountManager.Account);
        }

        public Task<int> GetHeroIdCounter() {
            return _bridge.GetHeroIdCounter();
        }

        public Task<int> GetHeroLimit() {
            return _bridge.GetHeroLimit();
        }

        public Task<BHeroPrice> GetHeroPrice() {
            return _bridge.GetHeroPrice();
        }

        public Task<double[,]> GetHeroUpgradeCost() {
            return _bridge.GetHeroUpgradeCost();
        }

        public Task<AbilityDesign[]> GetHeroAbilityDesigns() {
            return _bridge.GetHeroAbilityDesigns();
        }

        public Task<int> GetClaimableHero() {
            return _bridge.GetClaimableHero(_accountManager.Account);
        }

        public Task<int> GetGiveAwayHero() {
            return Task.FromResult(0);
            //return _bridge.GetGiveAwayHero(_accountManager.Account);
        }

        public Task<ProcessToken> GetPendingHero() {
            return _bridge.GetPendingHero(_accountManager.Account);
        }

        public Task<bool> BuyHero(int count, BuyHeroCategory category, bool isHeroS) {
            return _bridge.BuyHero(_accountManager.Account, count, category, isHeroS);
        }

        public Task<bool> UpgradeHero(int baseId, int materialId) {
            return _bridge.UpgradeHero(_accountManager.Account, baseId, materialId);
        }

        public Task<bool> ClaimHero() {
            return _bridge.ClaimHero(_accountManager.Account);
        }

        public Task<bool> ClaimGiveAwayHero() {
            return _bridge.ClaimGiveAwayHero();
        }

        public Task<HeroProcessTokenResult> ProcessTokenRequests() {
            return _bridge.ProcessTokenRequests(_accountManager.Account);
        }

        public Task<bool> HasPendingHeroRandomization(int heroId) {
            return _bridge.HasPendingHeroRandomization(heroId);
        }

        public Task<bool> RandomizeHeroAbilities(int heroId) {
            return _bridge.RandomizeHeroAbilities(_accountManager.Account, heroId);
        }

        public Task<bool> ProcessHeroRandomizeAbilities(int heroId) {
            return _bridge.ProcessHeroRandomizeAbilities(_accountManager.Account, heroId);
        }

        public Task<bool> IsSuperBoxEnabled() {
            return _bridge.IsSuperBoxEnabled();
        }

        public Task<int> GetHouseLimit() {
            return _bridge.GetHouseLimit();
        }

        public Task<double[]> GetHousePrice() {
            return _bridge.GetHousePrice();
        }

        public Task<int[]> GetAvailableHouse() {
            return _bridge.GetAvailableHouse();
        }

        public Task<int[]> GetHouseMintLimits() {
            return _bridge.GetHouseMintLimits();
        }

        public Task<HouseStats[]> GetHouseStats() {
            return _bridge.GetHouseStats();
        }

        public Task<bool> BuyHouse(int rarity) {
            return _bridge.BuyHouse(_accountManager.Account, rarity);
        }

        public Task<bool> Deposit(int amount, int category) {
            return _bridge.Deposit(_accountManager.Account, amount, category);
        }

        public Task<bool> FusionHero(int[] heroIds) {
            return _bridge.FusionHero(heroIds);
        }

        public Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            return _bridge.Fusion(mainHeroIds, secondHeroIds);
        }

        public Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            return _bridge.RepairShield(idHeroS, idHeroesBurn);
        }

        public Task<bool> GetNFT(int amount, int eventId, int nonce, string signature) {
            return _bridge.GetNFT(amount, eventId, nonce, signature);
        }

        public Task<string> ClaimToken(double amount, int tokenType, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations) {
            return _bridge.ClaimToken(tokenType, amount, nonce, details, signature, formatType, waitConfirmations);
        }

        public Task<int> GetRockAmount() {
            return _bridge.GetRockAmount(_accountManager.Account);
        }

        public Task<string> CreateRock(int[] idHeroesBurn) {
            return _bridge.CreateRock(idHeroesBurn);
        }

        public Task<bool> RepairShieldWithRock(int idHeroS, int amountRock) {
            return _bridge.RepairShieldWithRock(idHeroS, amountRock);
        }

        public Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock) {
            return _bridge.UpgradeShieldLevel(idHeroS, amountRock);
        }
        
        public Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature) {
            return _bridge.UpgradeShieldLevelV2( idHero, nonce, signature);
        }
        
        
        public Task<bool> CanUseVoucher(int voucherType) {
            return _bridge.CanUseVoucher(voucherType, _accountManager.Account);
        }

        public Task<bool> BuyHeroUseVoucher(string tokenPay, int voucherType, int heroQuantity, string amount,
            int nonce, string signature) {
            return _bridge.BuyHeroUseVoucher(_accountManager.Account, tokenPay, voucherType, heroQuantity, amount,
                nonce, signature);
        }

        public Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category) {
            return _bridge.Exchange_BuyBcoin(amount, category, _accountManager.Account);
        }

        public Task<ExchangeInfo> Exchange_GetInfo() {
            return _bridge.Exchange_GetInfo();
        }

        public Task<bool> StakeToHero(int id, double amount, string tokenAddress, StakeHeroCategory category) {
            return _bridge.StakeToHero(_accountManager.Account, id, amount, tokenAddress, category);
        }

        public Task<bool> WithDrawFromHeroId(int id, double amount, string tokenAddress) {
            return _bridge.WithDrawFromHeroId(id, amount, tokenAddress);
        }

        public Task<double> GetStakeFromHeroId(int id, string tokenAddress) {
            return _bridge.GetStakeFromHeroId(id, tokenAddress);
        }

        public Task<double> GetFeeFromHeroId(int id, string tokenAddress) {
            return _bridge.GetFeeFromHeroId(id, tokenAddress);
        }

        public Task<bool> DepositTon(string invoice, double amount) {
            return _bridge.DepositTon(invoice, amount);
        }
        
        public Task<bool> DepositAirdrop(string invoice, string amount, string chainId) {
            return _bridge.DepositAirdrop(invoice, amount, chainId);
        }
    }
}
#endif