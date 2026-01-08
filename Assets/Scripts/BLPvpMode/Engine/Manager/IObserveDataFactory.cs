using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Delta;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IObserveDataFactory {
        [NotNull]
        IMatchObserveData Generate(long timestamp, [NotNull] IMatchStateDelta stateDelta);
    }
}