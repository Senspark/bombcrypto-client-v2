using JetBrains.Annotations;

namespace BLPvpMode.Engine.Info {
    public interface IMatchResultInfo {
        bool IsDraw { get; }
        int WinningTeam { get; }

        [NotNull]
        int[] Scores { get; }

        int Duration { get; }
        long StartTimestamp { get; }

        [NotNull]
        IMatchResultUserInfo[] Info { get; }
    }
}