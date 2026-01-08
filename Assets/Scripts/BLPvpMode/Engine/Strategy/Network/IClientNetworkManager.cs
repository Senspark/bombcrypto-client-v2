using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.Network {
    public interface IClientNetworkManager {
        int Latency { get; }
        int TimeDelta { get; }
        float LossRate { get; }
        void Ping([NotNull] int[] latencies, [NotNull] int[] timeDeltas, [NotNull] float[] lossRates);
        void Pong(long clientTimestamp, int requestId);
    }
}