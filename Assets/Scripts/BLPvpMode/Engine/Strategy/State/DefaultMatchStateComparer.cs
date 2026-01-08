using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Delta;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.State {
    public class DefaultMatchStateComparer : IMatchStateComparer {
        [NotNull]
        private readonly IHeroStateComparer _heroComparer;

        [NotNull]
        private readonly IBombStateComparer _bombComparer;

        [NotNull]
        private readonly IMapStateComparer _mapComparer;

        public DefaultMatchStateComparer() {
            _heroComparer = new DefaultHeroStateComparer();
            _bombComparer = new DefaultBombStateComparer();
            _mapComparer = new DefaultMapStateComparer();
        }

        public IMatchStateDelta Compare(
            IMatchState state,
            IMatchState lastState
        ) {
            var heroDelta = _heroComparer.Compare(state.HeroState, lastState.HeroState);
            var bombDelta = _bombComparer.Compare(state.BombState, lastState.BombState);
            var blockDelta = _mapComparer.Compare(state.MapState, lastState.MapState);
            if (heroDelta.IsEmpty() &&
                bombDelta.IsEmpty() &&
                blockDelta.IsEmpty()) {
                return null;
            }
            return new MatchStateDelta(
                hero: heroDelta.ToArray(), //
                bomb: bombDelta.ToArray(),
                block: blockDelta.ToArray()
            );
        }
    }
}