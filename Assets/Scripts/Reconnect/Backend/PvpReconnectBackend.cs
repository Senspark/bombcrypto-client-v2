using System;
using System.Threading.Tasks;

using App;

using BLPvpMode.Engine.User;

using JetBrains.Annotations;

using Senspark;

namespace Reconnect.Backend {
    public class PvpReconnectBackend : IReconnectBackend {
        [NotNull]
        private readonly IUser _user;

        [NotNull]
        private readonly ObserverHandle _handle;

        private TaskCompletionSource<object> _connectionLostTcs;
        private TaskCompletionSource<object> _reconnectTcs;

        private bool _finished;
        private bool _disposed;

        public PvpReconnectBackend([NotNull] IUser user) {
            _user = user;
            _handle = new ObserverHandle();
            _handle.AddObserver(_user, new UserObserver {
                OnChangeStatus = status => {
                    if (status
                        is UserStatus.Disconnected
                        or UserStatus.Connecting) {
                        _connectionLostTcs?.SetResult(null);
                        return;
                    }
                    if (status == UserStatus.Connected) {
                        _reconnectTcs?.SetResult(null);
                        return;
                    }
                },
                OnFinishRound = OnFinish,
                OnFinishMatch = OnFinish,
            });
        }

        ~PvpReconnectBackend() => Dispose(false);

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing) {
            if (_disposed) {
                return;
            }
            if (disposing) {
                _handle.Dispose();
                _connectionLostTcs?.SetException(new ObjectDisposedException(nameof(_connectionLostTcs)));
                _reconnectTcs?.SetException(new ObjectDisposedException(nameof(_reconnectTcs)));
            }
            _disposed = true;
        }

        public async Task WaitForConnectionLost() {
            if (_connectionLostTcs != null) {
                await _connectionLostTcs.Task;
                return;
            }
            if (_finished) {
                throw new Exception("Match is finished");
            }
            try {
                _connectionLostTcs = new TaskCompletionSource<object>();
                await _connectionLostTcs.Task;
            } finally {
                _connectionLostTcs = null;
            }
        }

        public async Task Reconnect() {
            if (_reconnectTcs != null) {
                await _reconnectTcs.Task;
                return;
            }
            if (_finished) {
                throw new Exception("Match is finished");
            }
            try {
                _reconnectTcs = new TaskCompletionSource<object>();
                if (_user.Status == UserStatus.Connecting) {
                    // Re-connection mechanism by system.
                } else {
                    await WebGLTaskDelay.Instance.Delay(1000);
                    if (_reconnectTcs.Task.IsCompleted) {
                        // Match finished.
                    } else {
                        await _user.Connect();
                    }
                }
                await _reconnectTcs.Task;
            } finally {
                _reconnectTcs = null;
            }
        }

        private void OnFinish(object _) {
            _finished = true;
            _connectionLostTcs?.SetException(new Exception("Match is finished"));
            _reconnectTcs?.SetException(new Exception("Match is finished"));
        }
    }
}