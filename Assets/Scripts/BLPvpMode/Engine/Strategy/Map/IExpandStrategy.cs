using System.Collections.Generic;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Map {
    public interface IExpandResult {
        [NotNull]
        Dictionary<Vector2Int, int> DamagedPositions { get; }

        [ItemNotNull]
        [NotNull]
        List<IBomb> ExplodedBombs { get; }
    }

    public interface IExpandStrategy {
        [NotNull]
        IExpandResult Expand(
            [NotNull] IBombManager bombManager,
            [NotNull] IMapManager mapManager,
            [NotNull] IBomb bomb
        );
    }
}