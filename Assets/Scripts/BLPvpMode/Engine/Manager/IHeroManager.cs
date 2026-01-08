using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface IHeroManagerState {
        [NotNull]
        Dictionary<int, IHeroState> Heroes { get; }

        [NotNull]
        IHeroManagerState Apply([NotNull] IHeroManagerState state);
    }

    public interface IHeroManager : IUpdater {
        [NotNull]
        IHeroManagerState State { get; }

        void ApplyState([NotNull] IHeroManagerState state);

        [NotNull]
        IHero GetHero(int slot);

        void DamageBomb(Vector2Int position, int amount);
        void DamageFallingBlock(Vector2Int position);
    }
}