using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public interface IMatrixState {
        bool IsValid { get; }
        int Left { get; }
        int Right { get; }
        int Top { get; }
        int Bottom { get; }

        [NotNull]
        Vector2Int[] Positions { get; }
    }
}