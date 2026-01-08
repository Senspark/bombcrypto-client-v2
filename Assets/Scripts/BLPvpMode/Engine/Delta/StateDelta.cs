using System;
using System.Linq;

using JetBrains.Annotations;

using Sfs2X.Entities.Data;

namespace BLPvpMode.Engine.Delta {
    public class StateDelta<T> {
        public T State { get; set; }
        public T LastState { get; set; }
    }

    public class StateDeltaLong : StateDelta<long> {
        [CanBeNull]
        public static StateDeltaLong Parse([CanBeNull] ISFSObject item) {
            if (item == null) {
                return null;
            }
            var state = Convert.ToInt64(item.GetLong("state"));
            var lastState = Convert.ToInt64(item.GetLong("last_state"));
            return new StateDeltaLong {
                State = state, //
                LastState = lastState,
            };
        }

        [CanBeNull]
        public static StateDeltaLong Compare(long state, long lastState) {
            if (state == lastState) {
                return null;
            }
            return new StateDeltaLong {
                State = state, //
                LastState = lastState,
            };
        }
    }

    public class StateDeltaLongArray : StateDelta<long[]> {
        [CanBeNull]
        public static StateDeltaLongArray Parse([CanBeNull] ISFSObject item) {
            if (item == null) {
                return null;
            }
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
            return new StateDeltaLongArray {
                State = state, //
                LastState = lastState,
            };
        }

        [CanBeNull]
        public static StateDeltaLongArray Compare([NotNull] long[] state, [NotNull] long[] lastState) {
            if (state.SequenceEqual(lastState)) {
                return null;
            }
            return new StateDeltaLongArray {
                State = state, //
                LastState = lastState,
            };
        }
    }
}