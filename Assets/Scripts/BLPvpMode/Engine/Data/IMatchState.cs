using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IMatchState {
        [NotNull]
        IHeroManagerState HeroState { get; }

        [NotNull]
        IBombManagerState BombState { get; }

        [NotNull]
        IMapManagerState MapState { get; }

        [NotNull]
        IMatchState Apply([NotNull] IMatchState state);
    }
}