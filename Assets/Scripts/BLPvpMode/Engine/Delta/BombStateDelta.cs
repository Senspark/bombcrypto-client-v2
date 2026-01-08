using System;

using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Delta {
    public class BombStateDelta : IBombStateDelta {
        [NotNull]
        public static IBombStateDelta Parse([NotNull] ISFSObject item) {
            var id = item.GetInt("id");
            var stateArray = item.GetSFSArray("state");
            var state = new long[stateArray.Count];
            for (var j = 0; j < stateArray.Count; ++j) {
                // Issue: may be int or long.
                state[j] = Convert.ToInt64(stateArray.GetElementAt(j));
            }
            var lastStateArray = item.GetSFSArray("last_state");
            var lastState = new long[lastStateArray.Count];
            for (var j = 0; j < lastStateArray.Count; ++j) {
                // Issue: may be int or long.
                lastState[j] = Convert.ToInt64(lastStateArray.GetElementAt(j));
            }
            return new BombStateDelta(
                id: id,
                state: state,
                lastState: lastState
            );
        }

        [CanBeNull]
        public static IBombStateDelta Compare(
            int id,
            [NotNull] IBombState state,
            [NotNull] IBombState lastState
        ) {
            var encodedState = state.Encode();
            var encodedLastState = lastState.Encode();
            if (encodedState == encodedLastState) {
                return null;
            }
            return new BombStateDelta(
                id: id,
                state: encodedState,
                lastState: encodedLastState
            );
        }

        public int Id { get; }
        public long[] State { get; }
        public long[] LastState { get; }

        private BombStateDelta(
            int id,
            [NotNull] long[] state,
            [NotNull] long[] lastState
        ) {
            Id = id;
            State = state;
            LastState = lastState;
        }
    }
}