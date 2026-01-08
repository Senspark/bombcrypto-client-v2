using BLPvpMode.Engine.User;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface INetworkManager : IUpdater {
        [NotNull]
        int[] Latencies { get; }

        [NotNull]
        int[] TimeDeltas { get; }

        [NotNull]
        float[] LossRates { get; }

        void Pong([NotNull] IUserController controller, long timestamp, int requestId);
    }
}