using JetBrains.Annotations;

namespace BLPvpMode.Engine.Info {
    public interface IPvpResultInfo {
        [NotNull]
        string Id { get; }

        PvpMode Mode { get; }

        bool IsDraw { get; }
        int WinningTeam { get; }

        [NotNull]
        int[] Scores { get; }

        [NotNull]
        IPvpResultUserInfo[] Info { get; }
    }
}