using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

using Sfs2X.Entities.Data;

using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Delta {
    public class HeroStateDelta : IHeroStateDelta {
        [NotNull]
        public static IHeroStateDelta Parse([NotNull] ISFSObject item) {
            var slot = item.GetInt("slot");
            return new HeroStateDelta(
                slot: slot,
                baseDelta: StateDeltaLongArray.Parse(item.GetSFSObject("base")),
                position: StateDeltaLong.Parse(item.GetSFSObject("position"))
            );
        }

        [CanBeNull]
        public static IHeroStateDelta Compare(
            int slot,
            [NotNull] IHeroState state,
            [NotNull] IHeroState lastState
        ) {
            var baseState = state.BaseState;
            var lastBaseState = lastState.BaseState;
            Assert.IsTrue(baseState != null && lastBaseState != null);
            var positionState = state.PositionState;
            var lastPositionState = lastState.PositionState;
            Assert.IsTrue(positionState != null && lastPositionState != null);
            var baseDelta = StateDeltaLongArray.Compare(baseState.Encode(), lastBaseState.Encode());
            var positionDelta = StateDeltaLong.Compare(positionState.Encode(), lastPositionState.Encode());
            if (baseDelta == null && positionDelta == null) {
                return null;
            }
            return new HeroStateDelta(
                slot: slot, //
                baseDelta: baseDelta,
                position: positionDelta
            );
        }

        public int Slot { get; }
        public StateDelta<long[]> Base { get; }
        public StateDelta<long> Position { get; }

        private HeroStateDelta(
            int slot,
            [CanBeNull] StateDelta<long[]> baseDelta,
            [CanBeNull] StateDelta<long> position
        ) {
            Slot = slot;
            Base = baseDelta;
            Position = position;
        }
    }
}