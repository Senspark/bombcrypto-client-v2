using BLPvpMode.Engine.Delta;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IMatchObserveData {
        int Id { get; }

        long Timestamp { get; }

        [NotNull]
        string MatchId { get; }

        [NotNull]
        IHeroStateDelta[] HeroDelta { get; }

        [NotNull]
        IBombStateDelta[] BombDelta { get; }

        [NotNull]
        IBlockStateDelta[] BlockDelta { get; }
    }
}