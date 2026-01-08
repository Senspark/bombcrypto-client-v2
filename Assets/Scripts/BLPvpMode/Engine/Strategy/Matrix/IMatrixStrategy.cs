using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public interface IMatrixStrategy {
        [NotNull]
        IMatrixState Process([NotNull] IMatrixState state);
    }

    public static class MatrixStrategyExtensions {
        public static IMatrixStrategy Then(this IMatrixStrategy strategy, IMatrixStrategy item) {
            return new ListMatrixStrategy(strategy, item);
        }

        public static IMatrixStrategy Loop(this IMatrixStrategy strategy, int loops) {
            return new LoopMatrixStrategy(strategy, loops);
        }
    }
}