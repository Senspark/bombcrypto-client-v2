using System;

using BLPvpMode.Manager.Api.Processors;

namespace BLPvpMode.Manager.Api {
    public interface ISmartFoxApi :
        IServerDispatcher,
        IServerProcessorVoid,
        IServerProcessorVoidT,
        IServerProcessor,
        IServerProcessorT,
        IDisposable {
        bool IsUdpAvailable { get; }
        bool IsConnected { get; }
        void Reinitialize();
    }
}