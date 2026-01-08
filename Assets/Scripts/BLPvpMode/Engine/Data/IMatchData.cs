using System.Collections.Generic;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public enum MatchStatus {
        Ready,
        Started,
        Finished,
        MatchStarted,
        MatchFinished,
    }

    public interface IMatchData {
        [NotNull]
        string Id { get; }

        MatchStatus Status { get; set; }
        int ObserverCount { get; }
        long StartTimestamp { get; }
        long ReadyStartTimestamp { get; set; }
        long RoundStartTimestamp { get; set; }
        int Round { get; set; }

        [ItemNotNull]
        [NotNull]
        List<IMatchResultInfo> Results { get; }
    }
}