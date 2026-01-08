using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface IBombListener {
        void OnAdded([NotNull] IBomb bomb, BombReason reason);
        void OnRemoved([NotNull] IBomb bomb, BombReason reason);
        void OnExploded([NotNull] IBomb bomb, [NotNull] Dictionary<Direction, int> ranges);
        void OnDamaged(Vector2Int position, int amount);
    }

    public interface IBombManagerState {
        int BombCounter { get; }

        [NotNull]
        Dictionary<int, IBombState> Bombs { get; }

        [NotNull]
        IBombManagerState Apply([NotNull] IBombManagerState state);
    }

    public interface IBombManager : IUpdater {
        [NotNull]
        IBombManagerState State { get; }

        void ApplyState([NotNull] IBombManagerState state);

        [NotNull]
        List<IBomb> GetBombs(int slot);

        [CanBeNull]
        IBomb GetBomb(Vector2Int position);

        [NotNull]
        IBomb PlantBomb([NotNull] IBombState state);

        void AddBomb([NotNull] IBomb bomb);
        void RemoveBomb([NotNull] IBomb bomb);
        void ExplodeBomb([NotNull] IBomb bomb);

        [NotNull]
        Dictionary<Direction, int> GetExplodeRanges([NotNull] IBomb bomb);

        void ThrowBomb([NotNull] IBomb bomb, Direction direction, int distance, int duration);
    }
}