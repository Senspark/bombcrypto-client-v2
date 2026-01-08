using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public class MatrixState : IMatrixState {
        public bool IsValid => Left <= Right && Top <= Bottom;
        public int Left { get; }
        public int Right { get; }
        public int Top { get; }
        public int Bottom { get; }
        public Vector2Int[] Positions { get; }

        public MatrixState(
            int left,
            int right,
            int top,
            int bottom,
            [NotNull] Vector2Int[] positions
        ) {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
            Positions = positions;
        }
    }
}