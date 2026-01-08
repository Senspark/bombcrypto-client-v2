using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Strategy.State;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public class DefaultStateManager : IStateManager {
        [NotNull]
        private readonly IMatch _match;

        [NotNull]
        private readonly IMatchStateComparer _matchComparer;

        [NotNull]
        private readonly IMatchState _initialState;

        [NotNull]
        private IMatchState _matchState;

        public IMatchStateDelta AccumulativeChangeData {
            get {
                var state = _match.State;
                return _matchComparer.Compare(state, _initialState);
            }
        }

        public DefaultStateManager(IMatch match) {
            _match = match;
            _matchComparer = new DefaultMatchStateComparer();
            _initialState = _match.State;
            _matchState = _initialState;
        }

        public IMatchStateDelta ProcessState() {
            var state = _match.State;
            var delta = _matchComparer.Compare(state, _matchState);
            if (delta == null) {
                return null;
            }
            _matchState = state;
            return delta;
        }
    }
}