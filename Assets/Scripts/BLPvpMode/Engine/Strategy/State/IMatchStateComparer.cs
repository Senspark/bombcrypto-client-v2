using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Delta;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.State {
    public interface IMatchStateComparer {
        [CanBeNull]
        IMatchStateDelta Compare(
            [NotNull] IMatchState state,
            [NotNull] IMatchState lastState
        );
    }
}