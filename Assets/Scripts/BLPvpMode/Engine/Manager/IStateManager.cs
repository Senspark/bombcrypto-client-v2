using BLPvpMode.Engine.Delta;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IStateManager {
        [CanBeNull]
        IMatchStateDelta AccumulativeChangeData { get; }

        [CanBeNull]
        IMatchStateDelta ProcessState();
    }
}