using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using CustomSmartFox;
using JetBrains.Annotations;
using Sfs2X.Core;

namespace BLPvpMode.Manager.Api {
    public class SmartFoxApi : ISmartFoxApi {
        [NotNull]
        private readonly IServerBridge _bridge;

        [NotNull]
        private readonly List<IServerListener> _listeners;

        [NotNull]
        private readonly List<(IServerListener, bool)> _pendingListeners;
        
        [NotNull]
        private readonly ISmartFoxDispatcher _dispatcherSfsEvent;
        
        [CanBeNull]
        private readonly App.IServerDispatcher _dispatcher;

        private readonly KeepAliveExtension _keepAlive;

        private readonly object _locker;
        private int _dispatchCounter;
        private bool _disposed;

        public bool IsUdpAvailable {
            get {
                lock (_locker) {
                    return _bridge.IsUdpAvailable;
                }
            }
        }

        public bool IsConnected {
            get {
                lock (_locker) {
                    return _bridge.IsConnected;
                }
            }
        }

        public void Reinitialize() {
            lock (_locker) {
                _bridge.Reinitialize();
            }
        }

        public SmartFoxApi([NotNull] IServerBridge bridge, IExtResponseEncoder encoder, App.IServerDispatcher dispatcher) {
            _bridge = bridge;
            _dispatcher = dispatcher;
            _dispatcherSfsEvent = new DefaultSmartFoxDispatcher(DispatchEvent, encoder);
            _bridge.AddEventListener(SFSEvent.CONNECTION, OnConnection);
            _bridge.AddEventListener(SFSEvent.CONNECTION_RETRY, OnConnectionRetry);
            _bridge.AddEventListener(SFSEvent.CONNECTION_RESUME, OnConnectionResume);
            _bridge.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
            _bridge.AddEventListener(SFSEvent.LOGIN, OnLogin);
            _bridge.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
            _bridge.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);
            _bridge.AddEventListener(SFSEvent.PING_PONG, OnPingPong);
            _bridge.AddEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
            _bridge.AddEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
            _bridge.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            _listeners = new List<IServerListener>();
            _pendingListeners = new List<(IServerListener, bool)>();
            _locker = new object();
            _keepAlive = new KeepAliveExtension(this, _dispatcher);
        }

        ~SmartFoxApi() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _bridge.RemoveEventListener(SFSEvent.CONNECTION, OnConnection);
                _bridge.RemoveEventListener(SFSEvent.CONNECTION_RETRY, OnConnectionRetry);
                _bridge.RemoveEventListener(SFSEvent.CONNECTION_RESUME, OnConnectionResume);
                _bridge.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
                _bridge.RemoveEventListener(SFSEvent.LOGIN, OnLogin);
                _bridge.RemoveEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
                _bridge.RemoveEventListener(SFSEvent.UDP_INIT, OnUdpInit);
                _bridge.RemoveEventListener(SFSEvent.PING_PONG, OnPingPong);
                _bridge.RemoveEventListener(SFSEvent.ROOM_VARIABLES_UPDATE, OnRoomVariableUpdate);
                _bridge.RemoveEventListener(SFSEvent.ROOM_JOIN, OnRoomJoin);
                _bridge.RemoveEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
            }
            _keepAlive.Dispose();
            _disposed = true;
        }

        private void OnConnection(BaseEvent evt) {
            _dispatcherSfsEvent.OnConnection(evt);
        }

        private void OnConnectionRetry(BaseEvent evt) {
            _dispatcherSfsEvent.OnConnectionRetry(evt);
        }

        private void OnConnectionResume(BaseEvent evt) {
            _dispatcherSfsEvent.OnConnectionResume(evt);
        }

        private void OnConnectionLost(BaseEvent evt) {
            _dispatcherSfsEvent.OnConnectionLost(evt);
        }

        private void OnLogin(BaseEvent evt) {
            _dispatcherSfsEvent.OnLogin(evt);
        }

        private void OnLoginError(BaseEvent evt) {
            _dispatcherSfsEvent.OnLoginError(evt);
        }

        private void OnUdpInit(BaseEvent evt) {
            _dispatcherSfsEvent.OnUdpInit(evt);
        }

        private void OnPingPong(BaseEvent evt) {
            _dispatcherSfsEvent.OnPingPong(evt);
        }

        private void OnRoomVariableUpdate(BaseEvent evt) {
            _dispatcherSfsEvent.OnRoomVariableUpdate(evt);
        }

        private void OnRoomJoin(BaseEvent evt) {
            _dispatcherSfsEvent.OnRoomJoin(evt);
        }

        private void OnExtensionResponse(BaseEvent evt) {
            _dispatcherSfsEvent.OnExtensionResponse(evt);
        }

        private void FlushListeners() {
            List<(IServerListener, bool)> listeners = null;
            if (_pendingListeners.Count > 0) {
                listeners = _pendingListeners.ToList();
                _pendingListeners.Clear();
            }
            if (listeners == null) {
                return;
            }
            foreach (var (listener, added) in listeners) {
                if (added) {
                    _listeners.Add(listener);
                } else {
                    _listeners.Remove(listener);
                }
            }
        }

        private void DispatchEvent(Action<IServerListener> callback) {
            lock (_locker) {
                if (_dispatchCounter == 0) {
                    FlushListeners();
                }
                try {
                    ++_dispatchCounter;
                    foreach (var listener in _listeners) {
                        callback(listener);
                    }
                } finally {
                    --_dispatchCounter;
                }
            }
        }

        public void AddListener(IServerListener listener) {
            lock (_locker) {
                if (_dispatchCounter > 0) {
                    _pendingListeners.Add((listener, true));
                } else {
                    FlushListeners();
                    _listeners.Add(listener);
                }
            }
        }

        public void RemoveListener(IServerListener listener) {
            lock (_locker) {
                if (_dispatchCounter > 0) {
                    _pendingListeners.Add((listener, false));
                } else {
                    FlushListeners();
                    _listeners.Remove(listener);
                }
            }
        }

        public async Task Process(IServerHandlerVoid handler) {
            try {
                AddListener(handler);
                await handler.Start(_bridge);
            } finally {
                RemoveListener(handler);
            }
        }

        public async Task Process<T>(IServerHandlerVoid<T> handler, T arg) {
            try {
                AddListener(handler);
                await handler.Start(_bridge, arg);
            } finally {
                RemoveListener(handler);
            }
        }

        public async Task<R> Process<R>(IServerHandler<R> handler) {
            try {
                AddListener(handler);
                var result = await handler.Start(_bridge);
                return result;
            } finally {
                RemoveListener(handler);
            }
        }

        public async Task<R> Process<R, T>(IServerHandler<R, T> handler, T arg) {
            try {
                AddListener(handler);
                var result = await handler.Start(_bridge, arg);
                return result;
            } finally {
                RemoveListener(handler);
            }
        }
    }
}