namespace BLPvpMode.Engine.Strategy.Matrix {
    public class LoopMatrixStrategy : IMatrixStrategy {
        private readonly IMatrixStrategy _strategy;
        private readonly int _loops;

        public LoopMatrixStrategy(IMatrixStrategy strategy, int loops) {
            _strategy = strategy;
            _loops = loops;
        }

        public IMatrixState Process(IMatrixState state) {
            var currentState = state;
            var index = 0;
            while (index < _loops && currentState.IsValid) {
                currentState = _strategy.Process(currentState);
                ++index;
            }
            return currentState;
        }
    }
}