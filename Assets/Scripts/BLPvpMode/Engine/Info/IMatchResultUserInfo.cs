using System.Collections.Generic;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Info {
    public interface IMatchResultUserInfo {
        [NotNull]
        string ServerId { get; }

        bool IsTest { get; }
        bool IsBot { get; }
        int TeamId { get; }
        int UserId { get; }

        [NotNull]
        string Username { get; }

        int MatchCount { get; }

        int WinMatchCount { get; }

        [NotNull]
        int[] Boosters { get; }

        [NotNull]
        Dictionary<int, int> UsedBoosters { get; }

        bool Quit { get; }
    }
}