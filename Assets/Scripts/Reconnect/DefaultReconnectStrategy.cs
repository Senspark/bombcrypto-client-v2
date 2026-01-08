using System;
using System.Threading.Tasks;

using App;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace Reconnect {
    public class DefaultReconnectStrategy : IReconnectStrategy {
        [NotNull]
        private readonly ILogManager _logManager;

        [NotNull]
        private readonly IReconnectBackend _backend;

        [NotNull]
        private readonly IReconnectView _view;

        private bool _disposed;
        //private const int MaxRetryCount = int.MaxValue;
        private const int MaxRetryCount = 10;

        public DefaultReconnectStrategy(
            [NotNull] ILogManager logManager,
            [NotNull] IReconnectBackend backend,
            [NotNull] IReconnectView view
        ) {
            _logManager = logManager;
            _backend = backend;
            _view = view;
        }

        ~DefaultReconnectStrategy() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _backend.Dispose();
            }
            _disposed = true;
        }

        public async Task Start() {
            while (true) {
                if (_disposed) {
                    return;
                }
                _logManager.Log("[ReconnectStrategy] WaitForConnectionLost");
                try {
                    await _backend.WaitForConnectionLost();
                } catch (Exception ex) {
                    _logManager.Log($"[ReconnectStrategy] ex={ex.Message}");
                    Debug.LogException(ex);
                    return;
                }
                if (_disposed) {
                    return;
                }
                var successful = false;
                try {
                    _logManager.Log("[ReconnectStrategy] StartReconnection");
                    await _view.StartReconnection();
                    if (_disposed) {
                        return;
                    }
                    for (var i = 0; i < MaxRetryCount; ++i) {
                        try {
                            _logManager.Log($"[ReconnectStrategy] Reconnect attempt={i + 1}");
                            _view.UpdateProgress(i);
                            await _backend.Reconnect();
                            if (_disposed) {
                                return;
                            }
                            successful = true;
                            break;
                        } catch (ObjectDisposedException ex) {
                            _logManager.Log($"[ReconnectStrategy] ex={ex.Message}");
                            return;
                        } catch (Exception ex) {
                            _logManager.Log($"[ReconnectStrategy] ex={ex.Message}");
                            Debug.LogException(ex);
                        }
                        await WebGLTaskDelay.Instance.Delay(3000);
                        if (_disposed) {
                            return;
                        }
                    }
                } finally {
                    _logManager.Log($"[ReconnectStrategy] FinishReconnection result = {successful}");
                    await _view.FinishReconnection(successful);
                    
                }
                if(!successful)
                    break;
            }
        }
    }
}