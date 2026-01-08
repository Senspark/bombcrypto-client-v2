using System;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

using Sfs2X.Entities.Data;

using UnityEngine;

namespace BLPvpMode.Engine.Delta {
    public class BlockStateDelta : IBlockStateDelta {
        [NotNull]
        public static IBlockStateDelta Parse([NotNull] ISFSObject item) {
            var x = item.GetInt("x");
            var y = item.GetInt("y");
            var state = Convert.ToInt64(item.GetLong("state"));
            var lastState = Convert.ToInt64(item.GetLong("last_state"));
            return new BlockStateDelta(
                position: new Vector2Int(x, y), //
                state: state,
                lastState: lastState
            );
        }

        [CanBeNull]
        public static IBlockStateDelta Compare(
            Vector2Int position,
            [NotNull] IBlockState state,
            [NotNull] IBlockState lastState
        ) {
            var encodedState = state.Encode();
            var encodedLastState = lastState.Encode();
            if (encodedState == encodedLastState) {
                return null;
            }
            return new BlockStateDelta(
                position: position, //
                state: encodedState,
                lastState: encodedLastState
            );
        }

        public Vector2Int Position { get; }
        public long State { get; }
        public long LastState { get; }

        private BlockStateDelta(
            Vector2Int position,
            long state,
            long lastState
        ) {
            Position = position;
            State = state;
            LastState = lastState;
        }
    }
}