using System;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;

using Utils;

namespace App {
    public class AutoUpdateBlockchainManager : IBlockchainManager {
        private readonly IBlockchainManager _manager;
        private readonly IStorageManager _storeManager;
        private readonly IBlockchainStorageManager _blockchainStorageManager;
        private readonly IFeatureManager _featureManager;

        public AutoUpdateBlockchainManager(
            IBlockchainManager manager,
            IBlockchainStorageManager blockchainStorageManager,
            IStorageManager storeManager,
            IFeatureManager featureManager) {
            _manager = manager;
            _blockchainStorageManager = blockchainStorageManager;
            _storeManager = storeManager;
            _featureManager = featureManager;
        }

        public Task<bool> Initialize() {
            return _manager.Initialize();
        }

        public void Destroy() {
            _manager.Destroy();
        }

        public async Task<double> GetBalance(RpcTokenCategory category) {
            var b = await _manager.GetBalance(category);
            if (category != RpcTokenCategory.Usdt) {
                _blockchainStorageManager.SetBalance(category, b);
            }
            return b;
        }

        public async Task<int> GetHeroIdCounter() {
            var value = await _manager.GetHeroIdCounter();
            _storeManager.HeroTotalSale = value;
            return value;
        }

        public async Task<int> GetHeroLimit() {
            var value = await _manager.GetHeroLimit();
            _storeManager.HeroLimit = value;
            return value;
        }

        public async Task<BHeroPrice> GetHeroPrice() {
            var value = await _manager.GetHeroPrice();
            _storeManager.HeroPrice = value;
            return value;
        }

        public async Task<double[,]> GetHeroUpgradeCost() {
            var value = await _manager.GetHeroUpgradeCost();
            _storeManager.UpgradePrice = value;
            return value;
        }

        public async Task<AbilityDesign[]> GetHeroAbilityDesigns() {
            var value = await _manager.GetHeroAbilityDesigns();
            _storeManager.HeroRandomizeAbilityCost = value;
            return value;
        }

        public async Task<int> GetClaimableHero() {
            var value = await _manager.GetClaimableHero();
            return value;
        }

        public async Task<int> GetGiveAwayHero() {
            var value = await _manager.GetGiveAwayHero();
            return value;
        }

        public async Task<ProcessToken> GetPendingHero() {
            var value = await _manager.GetPendingHero();
            return value;
        }

        public Task<bool> BuyHero(int count, BuyHeroCategory category, bool isHeroS) {
            return _manager.BuyHero(count, category, isHeroS);
        }
        
        public Task<bool> UpgradeHero(int baseId, int materialId) {
            return _manager.UpgradeHero(baseId, materialId);
        }

        public Task<bool> ClaimHero() {
            return _manager.ClaimHero();
        }

        public Task<bool> ClaimGiveAwayHero() {
            return _manager.ClaimGiveAwayHero();
        }

        public Task<HeroProcessTokenResult> ProcessTokenRequests() {
            return _manager.ProcessTokenRequests();
        }

        public Task<bool> HasPendingHeroRandomization(int heroId) {
            return _manager.HasPendingHeroRandomization(heroId);
        }

        public Task<bool> RandomizeHeroAbilities(int heroId) {
            return _manager.RandomizeHeroAbilities(heroId);
        }

        public Task<bool> ProcessHeroRandomizeAbilities(int heroId) {
            return _manager.ProcessHeroRandomizeAbilities(heroId);
        }

        public async Task<bool> IsSuperBoxEnabled() {
            var isEvent = await _manager.IsSuperBoxEnabled();
            _storeManager.IsEventSuperLegend = isEvent;
            return isEvent;
        }

        public async Task<int> GetHouseLimit() {
            var value = await _manager.GetHouseLimit();
            _storeManager.HouseLimit = value;
            return value;
        }

        public async Task<double[]> GetHousePrice() {
            var value = await _manager.GetHousePrice();
            _storeManager.HousePrice = value;
            return value;
        }

        public async Task<int[]> GetAvailableHouse() {
            var value = await _manager.GetAvailableHouse();
            _storeManager.HouseMinAvailable = value;
            return value;
        }
        
        public async Task<int[]> GetHouseMintLimits() {
            var value = await _manager.GetHouseMintLimits();
            _storeManager.HouseMintLimits = value;
            return value;
        }

        public async Task<HouseStats[]> GetHouseStats() {
            var value = await _manager.GetHouseStats();
            _storeManager.Charge = value.Select(item => item.Recovery / 60f).ToArray();
            _storeManager.Slot = value.Select(item => item.Capacity).ToArray();
            return value;
        }

        public Task<bool> BuyHouse(int rarity) {
            return _manager.BuyHouse(rarity);
        }

        public Task<bool> Deposit(int amount, int category) {
            return _manager.Deposit(amount, category);
        }

        public Task<bool> FusionHero(int[] heroIds) {
            return _manager.FusionHero(heroIds);
        }

        public Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds) {
            return _manager.Fusion(mainHeroIds, secondHeroIds);
        }

        public Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn) {
            return _manager.RepairShield(idHeroS, idHeroesBurn);
        }

        public Task<bool> GetNFT(int amount, int eventId, int nonce, string signature) {
            return _manager.GetNFT(amount, eventId, nonce, signature);
        }

        public Task<string> ClaimToken(double amount, int tokenType, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations) {
            return _manager.ClaimToken(amount, tokenType, nonce, details, signature, formatType, waitConfirmations);
        }

        public async Task<int> GetRockAmount() {
            var amountRock = await _manager.GetRockAmount();
            _blockchainStorageManager.SetBalance(ObserverCurrencyType.Rock, amountRock);
            return amountRock;
        }

        public Task<string> CreateRock(int[] idHeroesBurn) {
            return _manager.CreateRock(idHeroesBurn);
        }

        public Task<bool> RepairShieldWithRock(int idHeroS, int amountRock) {
            return _manager.RepairShieldWithRock(idHeroS, amountRock);
        }

        public Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock) {
            return _manager.UpgradeShieldLevel(idHeroS, amountRock);
        }
        
        public Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature) {
            return _manager.UpgradeShieldLevelV2(idHero, nonce, signature);
        }
        
        public Task<bool> CanUseVoucher(int voucherType) {
            return _manager.CanUseVoucher(voucherType);
        }

        public Task<bool> BuyHeroUseVoucher(string tokenPay, int voucherType, int heroQuantity, string amount,
            int nonce, string signature) {
            return _manager.BuyHeroUseVoucher(tokenPay, voucherType, heroQuantity, amount, nonce, signature);
        }

        public Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category) {
            return _manager.Exchange_BuyBcoin(amount, category);
        }

        public Task<ExchangeInfo> Exchange_GetInfo() {
            return _manager.Exchange_GetInfo();
        }

        public Task<bool> StakeToHero(int id, double amount, string tokenAddress, StakeHeroCategory category) {
            return _manager.StakeToHero(id, amount, tokenAddress, category);
        }

        public Task<bool> WithDrawFromHeroId(int id, double amount, string tokenAddress) {
            return _manager.WithDrawFromHeroId(id, amount, tokenAddress);
        }

        public Task<double> GetStakeFromHeroId(int id, string tokenAddress) {
            return _manager.GetStakeFromHeroId(id, tokenAddress);
        }

        public Task<double> GetFeeFromHeroId(int id, string tokenAddress) {
            return _manager.GetFeeFromHeroId(id, tokenAddress);
        }

        public Task<bool> DepositTon(string invoice, double amount) {
            return _manager.DepositTon(invoice, amount);
        }
        
        public Task<bool> DepositAirdrop(string invoice, string amount, string chainId) {
            return _manager.DepositAirdrop(invoice, amount, chainId);
        }
    }
}