using System.Collections.Generic;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public interface IBombState : IEntityState {
        int Slot { get; }
        BombReason Reason { get; }
        Vector2 Position { get; }

        int Range { get; }
        int Damage { get; }
        bool Piercing { get; }
        int ExplodeDuration { get; }

        [NotNull]
        Dictionary<Direction, int> ExplodeRanges { get; }

        int PlantTimestamp { get; }

        long[] Encode();
    }
}