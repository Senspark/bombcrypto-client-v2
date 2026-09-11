using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Senspark;

namespace App {
    
    [Serializable]
    public class AbilityDesign {
        [JsonProperty("min_cost")]
        public double MinCost { get; set; }
        [JsonProperty("max_cost")]
        public double MaxCost { get; set; }
        [JsonProperty("incremental_cost")]
        public double IncrementalCost { get; set; }
    }

    public class BHeroPrice {
        //DevHoang: Add new airdrop
        public readonly float Coin;
        public readonly float Sen;
        public readonly float Ton;
        public readonly float StarCore;
        public readonly float Sol;
        public readonly float BcoinDeposited;
        public readonly float Ron;
        public readonly float Bas;
        public readonly float Vic;

        [JsonConstructor]
        public BHeroPrice(float coin, float sen, float ton, float star_core, float bcoin_deposited, float sol = 0, float ron = 0, float bas = 0, float vic = 0) {
            Coin = coin;
            Sen = sen;
            Ton = ton;
            StarCore = star_core;
            BcoinDeposited = bcoin_deposited;
            Sol = sol;
            Ron = ron;
            Bas = bas;
            Vic = vic;
        }
    }

    [Serializable]
    public class HouseStats {
        [JsonProperty("recovery")]
        public int Recovery { get; set; }
        [JsonProperty("capacity")]
        public int Capacity { get; set; }
    }
    
    [Serializable]
    public class Message {
        public bool code;
        public string message;
    }

    [Serializable]
    public class HeroProcessTokenResult {
        public bool result;
        /// <summary>
        /// Số lượng hero bị mất do fusion fail
        /// </summary>
        public int fusionFailAmount;
        public List<int> fusionSuccessHeroIds;
    }

    [Serializable]
    public class ClaimAndProcessResult {
        public string txHash;
        public HeroProcessTokenResult processResult;
    }

    /// <summary>
    /// Kết quả của ba hành động BHeroS trả bằng native (upgrade / reset skill / reset skin).
    /// <see cref="details"/> là details word đọc lại từ chain ngay sau receipt, nên UI reveal
    /// dựng được hero mới mà không phải chờ server sync. Rỗng khi tx thất bại.
    /// </summary>
    [Serializable]
    public class HeroActionResult {
        public bool success;
        public string txHash;
        public string details;
    }

    [Serializable]
    public class StakeResult {
        public bool success;
        public string txHash;
    }

    [Serializable]
    public class BridgeTxResult {
        public bool success;
        public string txHash;
        // Net token amount received (after fee), read from the on-chain Withdraw
        // event by the web/editor layer. 0 for deposit / failure.
        public double net;
    }
    
    [Serializable]
    public struct ProcessToken {
        public int pendingHeroes;
        public int pendingHeroesFusion;
    }
    
    [Serializable]
    public class ExchangeInfo {
        /// <summary>
        /// 1 Bcoin = ? Usdt
        /// </summary>
        public double price;
        
        /// <summary>
        /// 0 -> 100%
        /// </summary>
        public double slippage;
        
        /// <summary>
        /// 0 -> 100%
        /// </summary>
        public double fee;
    }

    public enum BuyHeroCategory {
        //DevHoang: Add new airdrop
        WithBcoin, WithSen, SuperBox, WithTon, WithStarCore, WithSol, WithRon, WithBas, WithVic
    }

    public enum WalletType {
        Metamask, Coinbase, TrustWallet, OperaWallet
    }

    public enum BuyBcoinCategory {
        UsdtAmount, BcoinAmount
    }

    public enum RpcTokenCategory {
        Bcoin, Bomb, SenBsc, Usdt, SenPolygon
    }
    
    public enum StakeHeroCategory {
        Bcoin, Sen
    }

    [Service(nameof(IBlockchainManager))]
    public interface IBlockchainManager : IService {
        Task<double> GetBalance(RpcTokenCategory category);
        Task<int> GetHeroIdCounter();
        Task<int> GetHeroLimit();
        Task<BHeroPrice> GetHeroPrice();
        Task<double[,]> GetHeroUpgradeCost();
        Task<AbilityDesign[]> GetHeroAbilityDesigns(); 
        Task<int> GetClaimableHero();
        Task<int> GetGiveAwayHero();
        Task<ProcessToken> GetPendingHero();
        Task<bool> BuyHero(int count, BuyHeroCategory category, bool isHeroS);
        Task<HeroActionResult> UpgradeHero(int baseId, int materialId, string priceWei);

        // Niveis 6-10: preco em tres moedas e upgrade no contrato de teste.
        Task<UpgradePrice> GetUpgradeV2Price(int baseId);

        Task<HeroActionResult> UpgradeHeroV2(int baseId, int materialId, UpgradePrice price);
        Task<bool> ClaimHero();
        Task<bool> ClaimGiveAwayHero();
        Task<HeroProcessTokenResult> ProcessTokenRequests();
        // BHeroS trả bằng native (BNB / POL). Contract dùng require(msg.value == price) và không
        // hoàn phần dư, nên giá là chuỗi wei chính xác: đọc ngay trước khi ký, không cache từ lúc
        // mở dialog, và không bao giờ cho đi qua double. level là index 0-based (= level hiển thị - 1).
        // times là randomizeAbilityCounter hiện tại của hero. Giá 0 nghĩa là tính năng đóng cho
        // rarity đó -> ẩn nút, đừng gọi (contract revert).
        Task<HeroActionResult> ResetSkill(int heroId, string priceWei);
        Task<HeroActionResult> ResetSkin(int heroId, string priceWei);
        Task<string> GetUpgradeNativePrice(int rarity, int level);
        Task<string> GetResetSkillNativePrice(int rarity, int times);
        Task<string> GetResetSkinNativePrice(int rarity);
        Task<string> GetNativeRate();
        Task<bool> IsSuperBoxEnabled();
        Task<int> GetHouseLimit();
        Task<double[]> GetHousePrice();
        Task<int[]> GetAvailableHouse();
        Task<int[]> GetHouseMintLimits();
        Task<HouseStats[]> GetHouseStats();
        Task<bool> BuyHouse(int rarity);
        Task<bool> Deposit(int amount, int category);
        Task<bool> FusionHero(int[] heroIds);
        Task<bool> Fusion(int[] mainHeroIds, int[] secondHeroIds);
        Task<bool> RepairShield(int idHeroS, int[] idHeroesBurn);
        Task<bool> GetNFT(int amount, int eventId, int nonce, string signature);
        Task<ClaimAndProcessResult> ClaimToken(double amount, int tokenType, int nonce, string[] details, string signature,
            string formatType, int waitConfirmations);
        Task<int> GetRockAmount();
        Task<string> CreateRock(int[] idHeroesBurn);
        Task<bool> RepairShieldWithRock(int idHeroS, int amountRock);
        Task<bool> UpgradeShieldLevel(int idHeroS, int amountRock);
        Task<bool> UpgradeShieldLevelV2(int idHero, int nonce, string signature);
        Task<bool> CanUseVoucher(int voucherType);
        Task<bool> BuyHeroUseVoucher(string tokenPay, int voucherType, int heroQuantity, string amount, int nonce,
            string signature);

        Task<bool> Exchange_BuyBcoin(double amount, BuyBcoinCategory category);
        Task<ExchangeInfo> Exchange_GetInfo();
        Task<StakeResult> StakeToHero(int id, double amount, StakeHeroCategory category);
        Task<StakeResult> WithDrawFromHeroId(int id, double amount, StakeHeroCategory category);
        Task<double> GetStakeFromHeroId(int id, StakeHeroCategory category);
        Task<double> GetFeeFromHeroId(int id, StakeHeroCategory category);
        Task<bool> DepositTon(string invoice, double amount);
        Task<bool> DepositAirdrop(string invoice, string amount, string chainId);

        Task<string> GetBridgeDeposited(string chain, string token);
        Task<string> GetBridgeWithdrawn(string chain, string token);
        void InvalidateBridgeRead(string chain, string token);
        Task<bool> GetBridgeDepositEnabled(string chain);
        Task<bool> GetBridgeWithdrawEnabled(string chain);
        Task<BridgeTxResult> BridgeDeposit(string chain, string token, string amountWei);
        Task<BridgeTxResult> BridgeWithdraw(string chain, string token, string otherDeposited, long deadline, string signature);

        // Native BNB / POL vault (DepositNative). Deposit is payable (no token, no approve); withdraw
        // relays the server signature and the contract self-computes amount = allowedCumulative −
        // withdrawn[user]. allowedCumulative is an exact wei string, relayed verbatim.
        Task<double> GetNativeWalletBalance(string chain);
        Task<bool> GetNativeDepositEnabled(string chain);
        Task<bool> GetNativeWithdrawEnabled(string chain);
        Task<BridgeTxResult> NativeDeposit(string chain, string amountWei);
        Task<BridgeTxResult> NativeWithdraw(string chain, string allowedCumulative, long deadline, string signature);
    }
}