using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public enum BombReason {
        Null,

        /** Add reasons. */
        /** Planted by hero. */
        Planted,

        /** Remove reasons. */
        /** Explode itself. */
        Exploded,

        /** Removed by falling blocks. */
        Removed,

        Throw,

        /** Planted by skull effect. */
        PlantedBySkull,
    }

    public interface IBomb : IEntity {
        [NotNull]
        IBombState State { get; }

        int Slot { get; }
        int Id { get; }
        BombReason Reason { get; }
        Vector2 Position { get; }
        int Range { get; }
        int Damage { get; }
        bool Piercing { get; }
        int PlantTimestamp { get; }
        void ApplyState(IBombState state);
        void Kill(BombReason reason);
    }
}