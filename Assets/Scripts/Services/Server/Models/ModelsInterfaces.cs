using System;
using System.Collections.Generic;
using Constant;
using Data;
using UnityEngine;

namespace App {
    public interface IHeroPower {
        int Rarity { get; }
        int[] Powers { get; }
    }

    public interface IHeroDetails {
        string Details { get; }
        int Id { get; }
        int Rarity { get; }
        HeroAccountType Type { get; }
        int Level { get; }
        int LevelShield { get; }
        int NumResetShield { get; }
        int Color { get; }
        int Skin { get; }
        int Stamina { get; }
        int Speed { get; set; }
        int BombSkin { get; }
        int BombCount { get; set; }
        int BombPower { get; }
        int BombRange { get; set; }
        int[] Abilities { get; }
        int RandomizeAbilityCounter { get; }
        bool IsActive { get; set; }
        int Energy { get; set; }
        HeroStage Stage { get; set; }
        bool IsHeroS { get; }
        List<IHeroSAbility> HeroSAbilities { get; }
        IHeroSAbility Shield { get; set; }

        //TODO: thêm thuộc tính Hp (máu story mode, bắt đầu == stamia, IsPlayed (hero đã chơi story mode trong ngày)
        int StoryHp { get; }
        bool StoryIsPlayed { get; set; }
        long TimeSync { get; set; }

        HeroAccountType AccountType { get; }

        // Các thuộc tính Revive
        bool AllowRevive { get; }
        bool AllowReviveByAds { get; }
        int ReviveGemAmount { get; }
        long TimeLockSince { get; }
        int TimeLockSeconds { get; }
        public double StakeBcoin { get; set; }
        public double StakeSen { get; set; }
    }

    public interface IHeroSAbility {
        int CurrentAmount { get; }
        int TotalAmount { get; }
        HeroSAbilityType AbilityType { get; }
    }

    public interface IHouseDetails {
        string Details { get; }
        int Id { get; }
        int Rarity { get; }
        float Recovery { get; }
        int Capacity { get; }
        bool IsActive { get; }
        long EndTimeRent { get; }
    }

    public interface IOfflineReward {
        double offlineTime { get; }
        int rewardType { get; }
        double amount { get; }
    }

    public interface IRewardType {
        BlockRewardType Type { get; }
        string Name { get; }
    }

    public interface IMapBlock {
        Vector2Int Position { get; }
        int Type { get; }
        float Health { get; }
        float MaxHealth { get; }
        ITokenReward[] Rewards { get; }
    }

    public interface IMapDetails {
        int Tileset { get; }
        IMapBlock[] Blocks { get; }
    }

    public interface ISyncHeroResponse {
        IHeroDetails[] Details { get; }
        int[] NewIds { get; }
    }

    public interface IBuyHeroServerResponse {
        IHeroDetails[] Details { get; }
    }
    
    public interface ISyncHouseResponse {
        IHouseDetails[] Details { get; }
        int[] NewIds { get; }
    }

    public interface IChestReward {
        List<ITokenReward> Rewards { get; }
    }

    public interface ITokenReward {
        IRewardType Type { get; }
        string Network { get; }
        float Value { get; }
        /// <summary>
        /// Số lượng đang chờ Claim hoàn tất
        /// </summary>
        float ClaimPending { get; }
    }

    public interface IAutoMinePackages {
        IAutoMineInfo Info { get; }
        List<IAutoMinePackageDetail> Packages { get; }
    }
    
    public interface IRockPackage {
        int RockReceived { get; }
        int RockTotal { get; }
    }

    public interface IAutoMinePackageDetail {
        string Package { get; }
        double Price { get; }
    }

    public interface IAutoMineInfo {
        bool CanBuyAutoMine { get; }
        bool ActiveAutoMine { get; }
        DateTime EndTime { get; }
    }

    public interface IInvestedDetail {
        float Invested { get; }
        float Mined { get; }
        float Roi { get; }
        float Reward { get; }
    }

    public interface IApproveClaimResponse {
        float ClaimedValue { get; }
    }

    public interface IResetNerfFee {
        float BcoinCost { get; }
        float SenCost { get; }
        int HeroQuantity { get; }
    }

    public interface ILoginResponse {
        bool IsNewUser { get; }
        string WalletAddress { get; }
        string SecondUserName { get; }
        string NickName { get; }
        string TokenType { get; }
        public string IdTelegram { get; }
        public Platform Platform { get; }
    }

    public interface IStakeResult {
        double MyStake { get; }
        double WithdrawFee { get; }
        DateTime StakeDate { get; }
        double DPY { get; }
        double TotalStake { get; }
        double ReceiveAmount { get; }
        double Profit { get; }
    }

    public interface IPveHeroDangerous {
        PveDangerousType DangerousType { get; }
        HeroId HeroId { get; }
        bool HasNewState { get; }
        HeroStage State { get; }
    }

    public interface IStartPveResponse {
        List<IPveHeroDangerous> DangerousData { get; }
        TrialState IsTrial { get; }
    }

    public interface IVipStakeResponse {
        List<IVipBooster> Inventory { get; }
        List<IVipInfo> Rewards { get; }
    }
    
    public interface IRockPackConfigs {
        List<IRockPackConfig> Packages { get; set; }
    }
    
    public interface IRockPackConfig {
        public string PackageName { get; set; }
        public int RockAmount { get; set; }
        public double SenPrice { get; set; }
        public double BcoinPrice { get; set; }
    }

    public interface IRentHousePackageConfigs {
        List<IRentHousePackageConfig> Packages { get; }
    }

    public interface IRentHousePackageConfig {
        public int Rarity { get; set; }
        public float Price { get; set; }
        public int NumDays { get; set; }
    }

    public interface IVipInfo {
        double StakeAmount { get; }
        List<IVipReward> Rewards { get; }
        int VipLevel { get; }
        bool IsCurrentVip { get; }
    }

    public interface IVipBooster {
        int Quantity { get; }
        VipRewardType Type { get; }
    }

    public interface IVipReward : IVipBooster {
        int HavingQuantity { get; }
        DateTime NextClaimUtc { get; }
    }

    public interface IPveBlockData {
        Vector2Int Coord { get; }
        int Type { get; }
        int Hp { get; }
        int MaxHp { get; }
        List<ITokenReward> Rewards { get; }
    }

    public interface IPveExplodeResponse {
        HeroId HeroId { get; }
        int Energy { get; }
        List<IPveBlockData> DestroyedBlocks { get; }
        IPveHeroDangerous Dangerous { get; }
        TrialState IsTrial { get; }
        List<RewardType> AttendPools { get; }
    }

    public interface IAirDropEvent {
        DateTime OpenDate { get; }
        DateTime CloseDate { get; }
        TimeSpan RemainingTime { get; }
        string CodeName { get; }
        string EventName { get; }
        int BomberToBuy { get; }
        int BomberBought { get; }
        int RewardAmount { get; }
        AirDropClaimStatus ClaimStatus { get; }
        bool Closed { get; }
        int SupplyTotal { get; }
        int SupplyClaimed { get; }
    }

    public interface IAirDropResponse {
        List<IAirDropEvent> ActiveEvents { get; }
        List<IAirDropEvent> ClosedEvents { get; }
    }

    public interface IAirDropClaimResponse {
        IAirDropResponse Events { get; }
        int Amount { get; }
        int EventId { get; }
        int Nonce { get; }
        string Signature { get; }
    }

    public interface ILuckyReward {
        int Quantity { get; }
        string Type { get; }
    }

    public interface ILuckyRewardsResponse {
        List<ILuckyReward> RewardsList { get; }
    }

    public interface IDailyMission {
        string Mission { get; }
        string MissionCode { get; }
        bool Claimable { get; }
        int TicketReward { get; }
        int RequestTimes { get; }
        int CompletedTimes { get; }
    }

    public interface IDailyMissionListResponse {
        List<IDailyMission> Missions { get; }
    }

    public interface IEmailResponse {
        bool Verified { get; }
        string Email { get; }
    }

    public interface IVoucherResponse {
        bool Allow { get; }
        string Amount { get; }
        string Signature { get; }
        int Nonce { get; }
        int HeroQuantity { get; }
    }

    public interface IServerConfigResponse {
        bool CanBuyHeroesTrial { get; }
    }

    public interface IExtensionResult<out TData> {
        int Code { get; }
        TData Data { get; }
        string Message { get; }
    }

    public interface IOfferPacksResult {
        List<IOffer> Offers { get; }

        public enum OfferType {
            Starter,
            Premium,
            Hero
        }

        public interface IOffer {
            OfferType Type { get; }
            string Name { get; }
            DateTime SaleEnd { get; }
            bool WillRemoveAds { get; }
            List<IItem> Items { get; }
            bool IsExpired { get; }
        }

        public interface IItem {
            int ItemId { get; }
            int ItemQuantity { get; }
            public TimeSpan ExpiresAfter { get; }
            public bool IsNeverExpired { get; }
        }
    }
    
    public interface IBurnHeroConfig {
        Dictionary<HeroRarity, IRockBurnData> Data { get; }
    }

    public interface IRockBurnData {
        float heroSRock { get; }
        float heroLRock { get; }
    }
    
    public interface IMinStakeHeroManager {
        Dictionary<HeroRarity, int> MinStakeLegacy { get; }
        Dictionary<HeroRarity, int> MinStakeGetBcoin { get; }
        Dictionary<HeroRarity, int> MinStakeGetSen { get; }
    }
    
    public interface IRepairShieldConfig {
        Dictionary<int, Dictionary<int, float>> Data { get; }
    }
    
    public interface IUpgradeShieldConfig {
        Dictionary<HeroRarity, List<int>> DurabilityPoint { get; }
        Dictionary<HeroRarity, List<float>> PriceRock { get; }
    }
    
    public interface IUpgradeShieldResponse {
        int Nonce { get;}
        string Signature { get; }
    }
    
    public interface IReferralData {
        string referralCode { get; }
        int minClaimReferral { get; }
        int timePayOutReferral { get; }
        int childQuantity { get; }
        double rewards { get; }
    }

    public interface ITreasureHuntConfigResponse {
        int HeroLimit { get; }
        BHeroPrice HeroPrice { get; }
        double[,] HeroUpgradePrice { get; }
        AbilityDesign[] HeroAbilityDesign { get; }
        int HouseLimit { get; }
        double[] HousePrices { get; }
        int[] HouseMintLimits { get; }
        HouseStats[] HouseStats { get; }
        double[] FusionFee { get; }
    }

    public enum AirDropClaimStatus {
        Checking, // Đang trong 1h chờ Retry lại
        Pending, // Chưa Claim lần nào
        Retry, // Cho phép Claim lại
        Completed // Claim thành công
    }

    public enum StoryModeTicketType {
        PlayToEarn,
        PlayForFun,
        Tournament,
        BossHunter
    }

    public enum LoginType {
        Wallet,
        UsernamePassword,
        Master,
        Guest,
        Apple,
        Telegram,
        Solana,
    }

    public enum DeviceType {
        Web, Mobile
    }
    
    public enum NetworkTypeInServer {
        //DevHoang: Add new airdrop
        BSC, POLYGON, TR, TON, SOL, RON, BAS, VIC
    }

    public enum GameModeType {
        TreasureHunt, StoryMode, TreasureHuntV2, PvpMode, UnKnown
    }

    public enum VipRewardType {
        Shield,
        ProtectionCard,
        PremiumProtectionCard,
        ConquestCard,
        PremiumConquestCard,
        ComboDaily,
        Hero,
    }

    public enum PveDangerousType {
        NoDanger,
        Danger,
        DangerButAvoided
    }

    public enum EntryPoint {
        Login,
        JoinZone,
        StartPvE,
        StopPvE,
        GetStoryMap,
        EnterStoryDoor
    }

    public enum HeroAccountType {
        //DevHoang: Add new airdrop
        Nft, Trial, Tr, Ton, Sol, Ron, Bas, Vic
    }

    public enum TrialState {
        Unknown, TrialBegin, TrialEnd
    }
    
    public interface IFusionTonHeroResponse {
        bool Result { get; }
        IHeroDetails[] Details { get; }
        int[] NewIds { get; }
        public int HeroesSize { get; }
    }

    public interface IMemberClubInfo {
        string Name { get; }
        double PointTotal { get; }
        double PointCurrentSeason { get; }
    }

    public interface IClubInfo {
        string ReferralCode { get; }
        long ClubId { get; }
        string Name { get; }
        string Link { get; }
        double PointTotal { get; }
        double PointCurrentSeason { get; }
        IMemberClubInfo[] Members { get; set; }
        IMemberClubInfo CurrentMember { get; set; }
        bool IsTopBidClub { get; }
        byte[] Avatar { get; }
    }

    public interface IClubRank {
        long ClubId { get; }
        string Name { get; }
        double PointTotal { get; }
        double PointCurrentSeason { get; }
    }

    public interface IClubBidPrice {
        int PackageId { get; }
        float Price { get; }
    }

    public enum ZoneName {
        ServerMain,
        ServerPvp,
        ThMode,
    }
}