using System.Collections.Generic;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Info {
    public interface IPvpResultUserInfo {
        [NotNull]
        string ServerId { get; }

        bool IsBot { get; }
        int TeamId { get; }
        int UserId { get; }

        [NotNull]
        string Username { get; }

        int Rank { get; }
        int Point { get; }
        int MatchCount { get; }
        int WinMatchCount { get; }
        int DeltaPoint { get; }

        [NotNull]
        Dictionary<int, int> UsedBoosters { get; }

        bool Quit { get; }

        [NotNull]
        Dictionary<int, float> Rewards { get; }
    }
}