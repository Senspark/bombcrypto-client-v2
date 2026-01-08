using Services;

namespace PvpSchedule.Models {
    public enum MatchStatus {
        Unknown,
        Waiting,
        InProgress,
        Upcoming,
        Ended,
    }

    public interface IPvpPlayerMatch {
        int Score { get; }
        int UserId { get; }
        string UserName { get; }
        string DisplayName { get; }
        PvpRankType Rank { get; }
    }

    public interface IPvpMatchSchedule {
        string MatchId { get; }
        MatchStatus Status { get; set; }
        BLPvpMode.Engine.Info.PvpMode Mode { get; }
        long FindBeginTimestamp { get; }
        long FindEndTimestamp { get; }
        long StartTimestamp { get; }
        long FinishTimestamp { get; }
        IPvpPlayerMatch[] Players { get; }
        int ObserverCount { get; }
    }
}