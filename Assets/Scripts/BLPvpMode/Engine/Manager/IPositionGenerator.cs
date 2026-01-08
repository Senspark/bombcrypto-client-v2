using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface IPositionGenerator {
        [NotNull]
        Vector2Int[] Generate([NotNull] IMapPattern pattern);
    }
}