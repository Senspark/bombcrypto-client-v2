using System;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ShrinkMatrixStrategy : IMatrixStrategy {
        [NotNull]
        private readonly IMatrixStrategy _strategy;

        public ShrinkMatrixStrategy(MatrixSide side) {
            _strategy = side switch {
                MatrixSide.Top => new ShrinkTopMatrixStrategy(),
                MatrixSide.Right => new ShrinkRightMatrixStrategy(),
                MatrixSide.Bottom => new ShrinkBottomMatrixStrategy(),
                MatrixSide.Left => new ShrinkLeftMatrixStrategy(),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null),
            };
        }

        public IMatrixState Process(IMatrixState state) {
            return _strategy.Process(state);
        }
    }
}