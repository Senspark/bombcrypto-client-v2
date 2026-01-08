using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IMatch : IUpdater {
        [NotNull]
        IMatchState State { get; }

        [NotNull]
        IHeroManager HeroManager { get; }

        [NotNull]
        IBombManager BombManager { get; }

        [NotNull]
        IMapManager MapManager { get; }

        void ApplyState([NotNull] IMatchState state);
    }
}