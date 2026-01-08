using System.Collections.Generic;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.State {
    public interface IBombStateComparer {
        [ItemNotNull]
        [NotNull]
        List<IBombStateDelta> Compare(
            [NotNull] IBombManagerState state,
            [NotNull] IBombManagerState lastState
        );
    }
}