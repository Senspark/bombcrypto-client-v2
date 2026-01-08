using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IPingPongData {
        int RequestId { get; }

        [NotNull]
        int[] Latencies { get; }

        [NotNull]
        int[] TimeDelta { get; }

        [NotNull]
        float[] LossRates { get; }
    }
}