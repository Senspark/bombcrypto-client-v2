using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.State {
    public class DefaultMapStateComparer : IMapStateComparer {
        [NotNull]
        private static readonly IBlockState DeadBlockState = new BlockState(
            isAlive: false,
            reason: BlockReason.Null,
            type: BlockType.Null,
            health: 0,
            maxHealth: 0
        );

        public List<IBlockStateDelta> Compare(
            IMapManagerState state,
            IMapManagerState lastState
        ) {
            var data = new List<IBlockStateDelta>();
            var keys = state.Blocks.Keys.Concat(lastState.Blocks.Keys).ToHashSet();
            keys.ForEach(key => {
                var itemState = state.Blocks.TryGetValue(key, out var value1) ? value1 : DeadBlockState;
                var lastItemState = lastState.Blocks.TryGetValue(key, out var value2) ? value2 : DeadBlockState;
                var delta = BlockStateDelta.Compare(key, itemState, lastItemState);
                if (delta == null) {
                    return;
                }
                data.Add(delta);
            });
            return data;
        }
    }
}