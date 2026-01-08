using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Manager;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.State {
    public class DefaultHeroStateComparer : IHeroStateComparer {
        private static readonly IHeroState DeadHeroState = new HeroState(
            isAlive: false,
            position: Vector2.zero,
            direction: Direction.Down,
            health: 0,
            damageSource: HeroDamageSource.Null,
            items: new Dictionary<HeroItem, int>(),
            effects: new Dictionary<HeroEffect, IHeroEffectState>()
        );

        public List<IHeroStateDelta> Compare(
            IHeroManagerState state,
            IHeroManagerState lastState
        ) {
            var data = new List<IHeroStateDelta>();
            var keys = state.Heroes.Keys.ToList();
            keys.ForEach(slot => {
                var itemState = state.Heroes.TryGetValue(slot, out var value1) ? value1 : DeadHeroState;
                var lastItemState = lastState.Heroes.TryGetValue(slot, out var value2) ? value2 : DeadHeroState;
                var delta = HeroStateDelta.Compare(slot, itemState, lastItemState);
                if (delta == null) {
                    return;
                }
                data.Add(delta);
            });
            return data;
        }
    }
}