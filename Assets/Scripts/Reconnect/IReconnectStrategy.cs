using System;
using System.Threading.Tasks;

namespace Reconnect {
    public interface IReconnectStrategy : IDisposable {
        Task Start();
    }
}