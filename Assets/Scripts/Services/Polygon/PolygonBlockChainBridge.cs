using System;
using System.Threading.Tasks;

using Senspark;

using Newtonsoft.Json;

using Share.Scripts.Communicate;

using UnityEngine;

#if UNITY_EDITOR || UNITY_WEBGL
namespace App {
    public class PolygonBlockChainBridge : IBlockchainBridge {
        private readonly IBlockchainBridge _bridge;
        private readonly ILogManager _logManager;
        private readonly JavascriptProcessor _processor;

        public PolygonBlockChainBridge(ILogManager logManager, IMasterUnityCommunication unityCommunication) {
            _bridge = new WebGLBlockchainBridge(logManager,  unityCommunication);
            _logManager = logManager;
            _processor = JavascriptProcessor.Instance;
        }

        public Task<double> GetBalance(RpcTokenCategory category, string walletAddress) {
            return _bridge.GetBalance(category, walletAddress);
        }

        public Task<double> GetSensparkBalance(string walletAddress) {
            return Task.FromResult(0d);
        }

        public Task<double> GetUsdtBalance(string walletAddress) {
            return _bridge.GetUsdtBalance(walletAddress);
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

        public Task<int> GetClaimableHero(string walletAddress) {
            return _bridge.GetClaimableHero(walletAddress);
        }

        public Task<int> GetGiveAwayHero(string walletAddress) {
            return Task.FromResult(0);
        }

        public async Task<ProcessToken> GetPendingHero(string walletAddress) {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("GetPendingHeroV2", walletAddress);
                var result = JsonConvert.DeserializeObject<ProcessToken>(response);
                _logManager.Log($"result = {result.pendingHeroes},{result.pendingHeroesFusion}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<bool> BuyHero(string walletAddress, int count, BuyHeroCategory category, bool isHeroS) {
            return _bridge.BuyHero(walletAddress, count, category, isHeroS);
        }

        public Task<bool> UpgradeHero(string walletAddress, int baseId, int materialId) {
            return Task.FromResult(false);
        }

        public Task<bool> ClaimHero(string walletAddress) {
            return _bridge.ClaimHero(walletAddress);
        }

        public Task<bool> ClaimGiveAwayHero() {
            return Task.FromResult(false);
        }

        public async Task<HeroProcessTokenResult> ProcessTokenRequests(string walletAddress) {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("ProcessTokenRequestsV2", walletAddress);
                var result = JsonConvert.DeserializeObject<HeroProcessTokenResult>(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<bool> HasPendingHeroRandomization(int heroId) {
            return Task.FromResult(false);
        }

        public Task<bool> RandomizeHeroAbilities(string walletAddress, int heroId) {
            return Task.FromResult(false);
        }

        public Task<bool> ProcessHeroRandomizeAbilities(string walletAddress, int heroId) {
            return Task.FromResult(false);
        }

        public Task<bool> IsSuperBoxEnabled() {
            return Task.FromResult(false);
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

        public Task<bool> BuyHouse(string walletAddress, int rarity) {
            return _bridge.BuyHouse(walletAddress, rarity);
        }

        public async Task<bool> Deposit(string walletAddress, int amount, int category) {
            try {
                _logManager.Log();
                var response = await _processor.CallMethod("DepositV2", walletAddress, amount, category);
                var result = bool.Parse(response);
                _logManager.Log($"result = {result}");
                return result;
            } catch (Exception ex) {
                Debug.LogException(ex);
                throw;
            }
        }

        public Task<bool> FusionHero(int[] heroIds) {
            return Task.FromResult(false);
        }

        public Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            return _bridge.Fusion(mainHeroIds, secondHeroIds);
        }

        public Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            return _bridge.RepairShield(idHeroS, idHeroesBurn);
        }

        public Task<bool> GetNFT(int amount, int eventId, int nonce, string signature) {
            return Task.FromResult(false);
        }

        public Task<string> ClaimToken(int tokenType, double amount, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations) {
            return _bridge.ClaimToken(tokenType, amount, nonce, details, signature, formatType, waitConfirmations);
        }

        public Task<int> GetRockAmount(string walletAddress) {
            return _bridge.GetRockAmount(walletAddress);
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
            return _bridge.UpgradeShieldLevelV2(idHero, nonce, signature);
        }
        
        
        public Task<bool> CanUseVoucher(int voucherType, string walletAddress) {
            return _bridge.CanUseVoucher(voucherType, walletAddress);
        }

        public Task<bool> BuyHeroUseVoucher(string walletAddress, string tokenPay, int voucherType, int heroQuantity,
            string amount, int nonce, string signature) {
            return _bridge.BuyHeroUseVoucher(walletAddress, tokenPay, voucherType, heroQuantity, amount, nonce, signature);
        }

        public Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category, string walletAddress) {
            return _bridge.Exchange_BuyBcoin(amount, category, walletAddress);
        }

        public Task<ExchangeInfo> Exchange_GetInfo() {
            return _bridge.Exchange_GetInfo();
        }

        public Task<bool> StakeToHero(string walletAddress, int id, double amount, string tokenAddress, StakeHeroCategory category) {
            return _bridge.StakeToHero(walletAddress, id, amount, tokenAddress, category);
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