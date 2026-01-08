using System.Threading.Tasks;

using Senspark;

namespace Services {
    public interface IPingInfo {
        string ZoneId { get; }
        int Latency { get; }
    }

    [Service(nameof(IPingManager))]
    public interface IPingManager {
        /// <summary>
        /// Gets zone latencies.
        /// </summary>
        Task<IPingInfo[]> GetLatencies();
    }
}