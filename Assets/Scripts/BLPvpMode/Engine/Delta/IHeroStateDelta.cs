using JetBrains.Annotations;

namespace BLPvpMode.Engine.Delta {
    public interface IHeroStateDelta {
        int Slot { get; }

        [CanBeNull]
        StateDelta<long[]> Base { get; }

        [CanBeNull]
        StateDelta<long> Position { get; }
    }
}