using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.Matrix {
    public class ListMatrixStrategy : IMatrixStrategy {
        [NotNull]
        private readonly IMatrixStrategy[] _items;

        public ListMatrixStrategy([NotNull] params IMatrixStrategy[] items) {
            _items = items;
        }

        public IMatrixState Process(IMatrixState state) {
            var currentState = state;
            foreach (var processor in _items) {
                if (!currentState.IsValid) {
                    break;
                }
                currentState = processor.Process(currentState);
            }
            return currentState;
        }
    }
}