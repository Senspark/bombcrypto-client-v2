using System;
using System.Threading.Tasks;

namespace Reconnect {
    public interface IReconnectBackend : IDisposable {
        /// <summary>
        /// Waits for connection lost.
        /// </summary>
        Task WaitForConnectionLost();

        /// <summary>
        /// Attempts to reconnect.
        /// </summary>
        Task Reconnect();
    }
}