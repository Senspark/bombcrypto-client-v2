using System.Collections.Generic;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.State {
    public interface IMapStateComparer {
        [ItemNotNull]
        [NotNull]
        List<IBlockStateDelta> Compare(
            [NotNull] IMapManagerState state,
            [NotNull] IMapManagerState lastState
        );
    }
}