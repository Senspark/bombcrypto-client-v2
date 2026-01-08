using System;
using System.Threading.Tasks;

using Senspark;

using Utils;

using UnityEngine;

namespace App {
    public class MobileBlockchainManager : IBlockchainManager {
        private readonly ILogManager _logManager;
        private readonly IAccountManager _accountManager;
        private readonly IApiManager _apiManager;
        private readonly Exception _notSupportException = new Exception("Feature not available");

        public MobileBlockchainManager(
            ILogManager logManager, 
            IAccountManager accountManager, 
            IApiManager apiManager) {
            _logManager = logManager;
            _accountManager = accountManager;
            _apiManager = apiManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public Task<bool> InitBlockchainConfig(NetworkType networkType, bool production) {
            return Task.FromResult(true);
        }

        public Task<double> GetBalance(RpcTokenCategory category) {
            _logManager.Log();
            return Task.FromResult(0d);
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
            return Task.FromResult<double>(0);
        }

        public Task<double> GetUsdtBalance() {
            return Task.FromResult(0d);
        }

        public Task<int> GetHeroIdCounter() {
            return Task.FromResult(0);
        }

        public Task<int> GetHeroLimit() {
            return Task.FromResult(0);
        }

        public Task<BHeroPrice> GetHeroPrice() {
            return Task.FromResult(new BHeroPrice(0, 0, 0, 0, 0));
        }

        public Task<double[,]> GetHeroUpgradeCost() {
            var result = new double[6, 4];
            return Task.FromResult(result);
        }

        public Task<AbilityDesign[]> GetHeroAbilityDesigns() {
            var result = new AbilityDesign[6];
            Array.Fill(result, new AbilityDesign {
                MinCost = 5,
                MaxCost = 10,
                IncrementalCost = 1,
            });
            return Task.FromResult(result);
        }

        public Task<int> GetClaimableHero() {
            return Task.FromResult(0);
        }

        public Task<int> GetGiveAwayHero() {
            return Task.FromResult(0);
        }

        public Task<ProcessToken> GetPendingHero() {
            var processToken = new ProcessToken {
                pendingHeroes = 0,
                pendingHeroesFusion = 0
            };
            return Task.FromResult(processToken);
        }

        public Task<bool> BuyHero(int count, BuyHeroCategory category, bool isHeroS) {
            return Task.FromResult(false);
        }

        public Task<bool> UpgradeHero(int baseId, int materialId) {
            return Task.FromResult(false);
        }

        public Task<bool> ClaimHero() {
            return Task.FromResult(false);
        }

        public Task<bool> ClaimGiveAwayHero() {
            return Task.FromResult(false);
        }

        public Task<HeroProcessTokenResult> ProcessTokenRequests() {
            throw _notSupportException;
        }

        public Task<bool> HasPendingHeroRandomization(int heroId) {
            return Task.FromResult(false);
        }

        public Task<bool> RandomizeHeroAbilities(int heroId) {
            return Task.FromResult(false);
        }

        public Task<bool> ProcessHeroRandomizeAbilities(int heroId) {
            return Task.FromResult(false);
        }

        public Task<bool> IsSuperBoxEnabled() {
            return Task.FromResult(false);
        }

        public Task<int> GetHouseLimit() {
            return Task.FromResult(0);
        }

        public Task<double[]> GetHousePrice() {
            var result = new double[6];
            return Task.FromResult(result);
        }

        public Task<int[]> GetAvailableHouse() {
            var result = new int[6];
            return Task.FromResult(result);
        }

        public Task<int[]> GetHouseMintLimits() {
            var result = new int[6];
            return Task.FromResult(result);
        }

        public Task<HouseStats[]> GetHouseStats() {
            var result = new HouseStats[6];
            result[0] = new HouseStats() {
                Capacity = 4, Recovery = 120
            };
            result[1] = new HouseStats() {
                Capacity = 6, Recovery = 300
            };
            result[2] = new HouseStats() {
                Capacity = 8, Recovery = 480
            };
            result[3] = new HouseStats() {
                Capacity = 10, Recovery = 660
            };
            result[4] = new HouseStats() {
                Capacity = 12, Recovery = 840
            };
            result[5] = new HouseStats() {
                Capacity = 14, Recovery = 1020
            };
            return Task.FromResult(result);
        }

        public Task<bool> BuyHouse(int rarity) {
            return Task.FromResult(false);
        }

        public Task<bool> Deposit(int amount, int category) {
            throw _notSupportException;
        }

        public Task<string> Sign(string message, string address) {
            throw _notSupportException;
        }

        public Task<(string, string)> GetAddressAndSignature() {
            throw _notSupportException;
        }

        public Task<bool> VerifySignature(string address, string signature) {
            throw _notSupportException;
        }

        public Task<string> ConnectAccount() {
            throw _notSupportException;
        }

        public Task<bool> IsValidChainId() {
            throw _notSupportException;
        }

        public Task<bool> FusionHero(int[] heroIds) {
            throw _notSupportException;
        }

        public Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            throw _notSupportException;
        }

        public Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            throw _notSupportException;
        }

        public Task<bool> GetNFT(int amount, int eventId, int nonce, string signature) {
            throw _notSupportException;
        }

        public Task<string> ClaimToken(double amount, int tokenType, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations) {
            throw _notSupportException;
        }

        public Task<int> GetRockAmount() {
            return Task.FromResult(0);
        }

        public Task<string> CreateRock(int[] idHeroesBurn) {
            throw _notSupportException;
        }

        public Task<bool> RepairShieldWithRock(int idHeroS, int amountRock) {
            throw _notSupportException;
        }

        public Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock) {
            throw _notSupportException;
        }
        
        public Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature) {
            throw _notSupportException;
        }
        
        public Task<bool> CanUseVoucher(int voucherType) {
            throw _notSupportException;
        }

        public Task<bool> BuyHeroUseVoucher(string tokenPay, int voucherType, int heroQuantity, string amount,
            int nonce, string signature) {
            throw _notSupportException;
        }

        public Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category) {
            throw _notSupportException;
        }
        
        public Task<ExchangeInfo> Exchange_GetInfo() {
            throw _notSupportException;
        }

        public Task<bool> StakeToHero(string walletAddress, int id, int amount) {
            throw _notSupportException;
        }

        public Task<bool> StakeToHero(int id, double amount, string tokenAddress, StakeHeroCategory category) {
            throw _notSupportException;
        }

        public Task<bool> WithDrawFromHeroId(int id, double amount, string tokenAddress) {
            throw _notSupportException;
        }

        public Task<double> GetStakeFromHeroId(int id, string tokenAddress) {
            throw _notSupportException;
        }

        public Task<double> GetFeeFromHeroId(int id, string tokenAddress) {
            throw _notSupportException;
        }

        public Task<bool> DepositTon(string invoice, double amount) {
            throw _notSupportException;
        }
        
        public Task<bool> DepositAirdrop(string invoice, string amount, string chainId) {
            throw _notSupportException;
        }
    }
}