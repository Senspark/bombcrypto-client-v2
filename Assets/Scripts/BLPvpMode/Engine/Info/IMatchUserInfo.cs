using System.Collections.Generic;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Info {
    public interface IMatchUserInfo {
        [NotNull]
        string ServerId { get; }

        int BuildVersion { get; }

        [NotNull]
        string MatchId { get; }

        int Mode { get; }
        bool IsTest { get; }
        bool IsWhitelisted { get; }
        bool IsBot { get; }
        int UserId { get; }

        [NotNull]
        string Username { get; }

        [NotNull]
        string DisplayName { get; }

        int TotalMatchCount { get; }
        int MatchCount { get; }
        int WinMatchCount { get; }
        int Rank { get; }
        int Point { get; }

        [NotNull]
        int[] Boosters { get; }

        [NotNull]
        Dictionary<int, int> AvailableBoosters { get; }

        [NotNull]
        IMatchHeroInfo Hero { get; }
        int Avatar { get; }
    }
}