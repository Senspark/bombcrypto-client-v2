using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.State {
    public class DefaultBombStateComparer : IBombStateComparer {
        [NotNull]
        private static readonly IBombState DeadBombState = new BombState(
            isAlive: false,
            slot: 0,
            reason: BombReason.Null,
            position: Vector2.zero,
            damage: 0,
            range: 0,
            piercing: false,
            explodeDuration: 0,
            explodeRanges: new Dictionary<Direction, int>(),
            plantTimestamp: 0
        );

        public List<IBombStateDelta> Compare(
            IBombManagerState state,
            IBombManagerState lastState
        ) {
            var data = new List<IBombStateDelta>();
            var keys = state.Bombs.Keys.Concat(lastState.Bombs.Keys).ToHashSet();
            keys.ForEach(id => {
                var itemState = state.Bombs.TryGetValue(id, out var value1) ? value1 : DeadBombState;
                var lastItemState = lastState.Bombs.TryGetValue(id, out var value2) ? value2 : DeadBombState;
                var delta = BombStateDelta.Compare(id, itemState, lastItemState);
                if (delta == null) {
                    return;
                }
                data.Add(delta);
            });
            return data;
        }
    }
}