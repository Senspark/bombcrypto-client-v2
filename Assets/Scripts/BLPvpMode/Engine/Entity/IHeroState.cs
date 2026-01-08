using System.Collections.Generic;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public interface IHeroBaseState : IEntityState {
        int Health { get; }
        HeroDamageSource DamageSource { get; }

        [NotNull]
        Dictionary<HeroItem, int> Items { get; }

        [NotNull]
        Dictionary<HeroEffect, IHeroEffectState> Effects { get; }

        [NotNull]
        long[] Encode();
    }

    public interface IHeroPositionState {
        Vector2 Position { get; }
        Vector2Int PositionInt { get; }
        Direction Direction { get; }
        long Encode();
    }

    public interface IHeroState : IEntityState {
        [CanBeNull]
        IHeroBaseState BaseState { get; }

        [CanBeNull]
        IHeroPositionState PositionState { get; }
    }
}