using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Util;

using UnityEngine;

namespace BLPvpMode.Manager.Api {
    public class ServerBridge : IServerBridge {
        private class DefaultBuilder : IExtensionRequestBuilder {
            public ISFSObject Build(string command, ISFSObject data) {
                return data;
            }
        }

        private class Runner : MonoBehaviour {
            [CanBeNull]
            public Action Updater { get; set; }

            private void Awake() {
                DontDestroyOnLoad(gameObject);
            }

            private void Update() {
                Updater?.Invoke();
            }
        }

        private readonly bool _useWebSocket;
        private readonly bool _useSSL;

        [NotNull]
        private SmartFox _api;

        [NotNull]
        private readonly Runner _runner;

        [NotNull]
        private readonly object _locker;

        private readonly List<(string, EventListenerDelegate)> _listeners;

        private bool _lagMonitorEnabled;
        private bool _disposed;

        public bool IsUdpAvailable {
            get {
                lock (_locker) {
                    return _api.UdpAvailable;
                }
            }
        }

        public bool IsConnected {
            get {
                lock (_locker) {
                    return _api.IsConnected;
                }
            }
        }

        public bool IsLagMonitorEnabled {
            get {
                lock (_locker) {
                    return _lagMonitorEnabled;
                }
            }
            set {
                lock (_locker) {
                    _lagMonitorEnabled = value;
                    _api.EnableLagMonitor(value);
                }
            }
        }

        public Room LastJoinedRoom {
            get {
                lock (_locker) {
                    return _api.LastJoinedRoom;
                }
            }
        }

        public IExtensionRequestBuilder Builder { get; }

        /// <summary>
        /// Initializes a new config.
        /// </summary>
        /// <param name="useWebSocket">Whether to use websocket.</param>
        /// <param name="useSSL">Tạo WSS hay WS</param>
        /// <param name="useThreadSafe">Whether thread-safe is enabled (tcp/udp only).</param>
        public ServerBridge(
            bool useWebSocket,
            bool useSSL,
            bool useThreadSafe
        ) : this(
            useWebSocket,
            useSSL,
            useWebSocket
                ? new SmartFox(useSSL ? UseWebSocket.WSS_BIN : UseWebSocket.WS_BIN) { ThreadSafeMode = true }
                : new SmartFox { ThreadSafeMode = useThreadSafe },
            new DefaultBuilder()
        ) { }

        private ServerBridge(
            bool useWebSocket,
            bool useSSL,
            [NotNull] SmartFox api,
            [NotNull] IExtensionRequestBuilder builder
        ) {
            _useWebSocket = useWebSocket;
            _useSSL = useSSL;
            _api = api;
            Builder = builder;
            _locker = new object();
            _listeners = new List<(string, EventListenerDelegate)>();
            _runner = CreateRunner();
        }

        ~ServerBridge() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                lock (_locker) {
                    _api.RemoveAllEventListeners();
                    if (_api.IsConnected) {
                        _api.Disconnect();
                    }
                }
                if (_runner) {
                    UnityEngine.Object.Destroy(_runner.gameObject);
                }
            }
            _disposed = true;
        }

        private Runner CreateRunner() {
            var runner = new GameObject(nameof(ServerBridge)).AddComponent<Runner>();
            runner.Updater = () => { //
                lock (_locker) {
                    _api.ProcessEvents();
                }
            };
            return runner;
        }

        public void Reinitialize() {
            lock (_locker) {
                _api.RemoveAllEventListeners();
                if (_api.IsConnected) {
                    _api.Disconnect();
                }
                _api = _useWebSocket
                    ? new SmartFox(_useSSL ? UseWebSocket.WSS_BIN : UseWebSocket.WS_BIN) { ThreadSafeMode = true }
                    : new SmartFox { ThreadSafeMode = _api.ThreadSafeMode };
                foreach (var (eventType, listener) in _listeners) {
                    _api.AddEventListener(eventType, listener);
                }
            }
        }

        public void AddEventListener(string eventType, EventListenerDelegate callback) {
            lock (_locker) {
                _listeners.Add((eventType, callback));
                _api.AddEventListener(eventType, callback);
            }
        }

        public void RemoveEventListener(string eventType, EventListenerDelegate callback) {
            lock (_locker) {
                _listeners.Remove((eventType, callback));
                _api.RemoveEventListener(eventType, callback);
            }
        }

        public void Connect(string host, int port, bool tcpNoDelay) {
            lock (_locker) {
                _api.Connect(new ConfigData {
                    Host = host, // 
                    Port = port,
                    TcpNoDelay = tcpNoDelay,
                    Zone = "_", // Will be set later.
                    BlueBox = { IsActive = false },
                });
            }
        }

        public void Disconnect() {
            lock (_locker) {
                _api.Disconnect();
            }
        }

        public void KillConnection() {
            lock (_locker) {
                _api.KillConnection();
            }
        }

        public void InitUdp(string host, int port) {
            lock (_locker) {
                _api.InitUDP(host, port);
            }
        }

        public void Send(BaseRequest request) {
            lock (_locker) {
                _api.Send(request);
            }
        }
    }
}