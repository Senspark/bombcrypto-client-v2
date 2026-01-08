using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using Data;
using JetBrains.Annotations;
using PvpMode.Manager;
using Services;

namespace PvpMode.Services {
    public interface IPvPHeroEnergy {
        int Balance { get; }
        long Id { get; }
        long RemainingTime { get; }
    }

    public interface ISyncPvPConfigResult {
        int[] Bets { get; }
        bool IsWhitelist { get; }
        long EventExp { get; }
        int OpenChestRequiredSharp { get; }
        float RewardFee { get; }
        bool SeasonValid { get; }
        int TicketPrice { get; }
        IDictionary<int, string> Items { get; }
    }

    public interface ISyncPvPHeroesResult {
        IPvPHeroEnergy[] HeroEnergies { get; }
        long LastPlayedHero { get; }
        int LastBet { get; }
        IDictionary<int, int> Equipments { get; }
    }

    public interface IPvpGenericResult {
        int Code { get; }
        string Message { get; }
    }

    public enum Direction {
        None = -1,
        Left,
        Right,
        Up,
        Down,
    }

    public interface IBoosterResult {
        IBooster[] Boosters { get; }
    }

    public interface IPvpRankingItemResult {
        int UserId { get; }
        int WinMatch { get; }
        int TotalMatch { get; }
        int RankNumber { get; }
        string Name { get; }
        string UserName { get; }
        int Point { get; }
        int Avatar { get; }
        int BombRank { get; }
        RewardData[] Rewards { get; }
    }

    public interface IPvpRankingResult {
        int TotalCount { get; }
        int RemainTime { get; }
        IPvpRankingItemResult[] RankList { get; }
        IPvpRankingItemResult CurrentRank { get; }
        IPvpCurrentRewardResult CurrentReward { get; }
        int CurrentSeason { get; }
        bool SeasonValid { get; }
    }
    
    public interface IPvpOtherUserInfo {
        IPvpRankingItemResult Rank { get; }
        EquipmentData[] EquipData { get; } 
        PlayerData Hero { get; }
    }
    
    public interface ICoinLeaderboardConfigResult {
        string Name { get; }
        int Rank { get; }
        int UpRankPointUser { get; }
        long UpRankPointClub { get; }
    }

    public interface ICoinRankingItemResult {
        int RankNumber { get; set; }
        string Name { get; }
        float Point { get; }
    }

    public interface ICoinRankingResult {
        int RemainTime { get; }
        ICoinRankingItemResult[] RankList { get; }
        ICoinRankingItemResult CurrentRank { get; }
    }
    
    public interface IPvpRewardResult {
        double BCoin { get; }
        int HeroBox { get; }
        int Shield { get; }
        int MinRank { get; }
        int MaxRank { get; }
    }

    public interface IPvpCurrentRewardResult {
        int Rank { get; }
        Dictionary<string, int> Reward { get; }
        double BCoin { get; }
        int HeroBox { get; }
        int Shield { get; }
        bool IsReward { get; }
        bool IsClaim { get; set; }
        int TotalMatch { get; }
        int PvpMatchReward { get; }
    }

    public interface IPvpClaimRewardResult {
        IPvpCurrentRewardResult CurrentReward { get; }
    }

    public interface IPvpClaimMatchRewardResult {
        string RewardId { get; }
        bool IsOutOfChest { get; }
    }

    public interface IPvpHistoryItemResult {
        string MatchId { get; }
        string OpponentName { get; }
        string Opponent { get; }
        string Time { get; }
        string Date { get; }
        bool IsWin { get; }
    }

    public interface IPvpHistoryResult {
        IPvpHistoryItemResult[] HistoryList { get; }
    }

    public interface IOpenChestResult {
        EquipmentData[] Items { get; }
    }

    public interface IGetEquipmentResult {
        EquipmentData[] Equipments { get; }
    }

    public interface IBonusRewardPvp {
        string RewardName { get; }
        string Type { get; }
        int Value { get; }
    }

    public interface IPvpServerBridge {
        Task<ISyncPvPConfigResult> SyncPvPConfig();
        Task<ISyncPvPHeroesResult> SyncPvPHeroes();

        [MustUseReturnValue]
        [NotNull]
        Task JoinQueue(
            int mode,
            [CanBeNull] string matchId,
            int heroId,
            [NotNull] int[] boosters,
            [NotNull] IPingInfo[] pingInfo,
            int avatarId,
            bool test = false
        );

        [MustUseReturnValue]
        [NotNull]
        Task LeaveQueue();

        Task<IBoosterResult> GetUserBooster();

        Task<IPvpRankingResult> GetPvpRanking(int page = 1, int size = 100);
        Task<IPvpOtherUserInfo> GetOtherUserInfo(int userId, string userName);
        Task<ICoinLeaderboardConfigResult[]> GetCoinLeaderboardConfig();
        Task<ICoinRankingResult> GetCoinRanking();
        Task<ICoinRankingResult> GetAllSeasonCoinRanking();
        Task<IPvpClaimRewardResult> ClaimPvpReward();
        Task<IPvpClaimMatchRewardResult> ClaimMatchReward();
        Task<IPvpHistoryResult> GetPvpHistory(int at = 0, int size = 50);
        Task<IOpenChestResult> OpenChest();
        Task<IGetEquipmentResult> GetEquipment();

        [MustUseReturnValue]
        [NotNull]
        Task Equip(int itemType, IEnumerable<(int, long)> itemList);

        Task<IBonusRewardPvp> GetBonusRewardPvp(string matchId, string adsId);
    }
}