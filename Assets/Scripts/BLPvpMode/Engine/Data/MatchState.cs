using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public class MatchState : IMatchState {
        [NotNull]
        public static IMatchState DecodeDelta([NotNull] IMatchStateDelta delta) {
            return new MatchState(
                heroState: HeroManagerState.DecodeDelta(delta.Hero),
                bombState: BombManagerState.DecodeDelta(delta.Bomb),
                mapState: MapManagerState.DecodeDelta(delta.Block)
            );
        }

        public IHeroManagerState HeroState { get; }
        public IBombManagerState BombState { get; }
        public IMapManagerState MapState { get; }

        public MatchState(
            [NotNull] IHeroManagerState heroState,
            [NotNull] IBombManagerState bombState,
            [NotNull] IMapManagerState mapState
        ) {
            HeroState = heroState;
            BombState = bombState;
            MapState = mapState;
        }

        public IMatchState Apply(IMatchState state) {
            return new MatchState(
                heroState: HeroState.Apply(state.HeroState),
                bombState: BombState.Apply(state.BombState),
                mapState: MapState.Apply(state.MapState)
            );
        }
    }
}