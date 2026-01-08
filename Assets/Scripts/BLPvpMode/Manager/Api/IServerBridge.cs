using System;

using JetBrains.Annotations;

using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Requests;

namespace BLPvpMode.Manager.Api {
    public interface IServerBridge : IDisposable {
        bool IsUdpAvailable { get; }
        bool IsConnected { get; }
        bool IsLagMonitorEnabled { get; set; }
        Room LastJoinedRoom { get; }
        IExtensionRequestBuilder Builder { get; }

        /// <summary>
        /// Re-initializes the underlying system.
        /// </summary>
        void Reinitialize();

        void AddEventListener([NotNull] string eventType, [NotNull] EventListenerDelegate callback);
        void RemoveEventListener([NotNull] string eventType, [NotNull] EventListenerDelegate callback);
        void Connect([NotNull] string host, int port, bool tcpNoDelay);
        void Disconnect();
        void KillConnection();
        void InitUdp([NotNull] string host, int port);
        void Send([NotNull] BaseRequest request);
    }
}