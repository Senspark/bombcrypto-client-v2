using JetBrains.Annotations;

namespace BLPvpMode.Engine.Delta {
    public interface IBombStateDelta {
        int Id { get; }

        [NotNull]
        long[] State { get; }

        [NotNull]
        long[] LastState { get; }
    }
}