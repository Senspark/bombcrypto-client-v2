using System.Collections.Generic;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.State {
    public interface IHeroStateComparer {
        [ItemNotNull]
        [NotNull]
        List<IHeroStateDelta> Compare(
            [NotNull] IHeroManagerState state,
            [NotNull] IHeroManagerState lastState
        );
    }
}