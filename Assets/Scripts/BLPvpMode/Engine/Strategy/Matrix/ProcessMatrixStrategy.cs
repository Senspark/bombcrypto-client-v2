using System;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ProcessMatrixStrategy : IMatrixStrategy {
        [NotNull]
        private readonly IMatrixStrategy _strategy;

        public ProcessMatrixStrategy(MatrixSide side, bool ccw) {
            _strategy = side switch {
                MatrixSide.Top => new ProcessTopMatrixStrategy(ccw),
                MatrixSide.Right => new ProcessRightMatrixStrategy(ccw),
                MatrixSide.Bottom => new ProcessBottomMatrixStrategy(ccw),
                MatrixSide.Left => new ProcessLeftMatrixStrategy(ccw),
                _ => throw new ArgumentOutOfRangeException(nameof(side), side, null),
            };
        }

        public IMatrixState Process(IMatrixState state) {
            return _strategy.Process(state);
        }
    }
}