using System;
using System.Threading.Tasks;

using App;

using JetBrains.Annotations;

using Senspark;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api.Handlers {
    public class ConnectHandler : IServerHandlerVoid {
        [NotNull]
        private readonly string _host;

        private readonly int _port;
        private readonly int _timeOutMs;
        private readonly bool _tcpNoDelay;
        private readonly ITaskDelay _taskDelay;

        [CanBeNull]
        private TaskCompletionSource<object> _tcs;

        public ConnectHandler(
            [NotNull] ILogManager logManager,
            [NotNull] string host,
            int port,
            bool tcpNoDelay,
            [NotNull] ITaskDelay taskDelay,
            float timeOut = 30f
        ) {
            _host = host;
            _port = port;
            logManager.Log($"CONNECT: {_host}:{_port}");
            _taskDelay = taskDelay;
            _tcpNoDelay = tcpNoDelay;
            _timeOutMs = 0; //(int)(timeOut * 1000);
        }

        public async Task Start(IServerBridge bridge) {
            if (_tcs != null) {
                await _tcs.Task;
                return;
            }
            try {
                _tcs = new TaskCompletionSource<object>();
                bridge.Connect(_host, _port, _tcpNoDelay);
                if (_timeOutMs > 0) {
                    await _tcs.TimeoutAfter(_timeOutMs, _taskDelay);
                } else {
                    await _tcs.Task;
                }
            } finally {
                _tcs = null;
            }
        }

        public void OnConnection() {
            _tcs?.SetResult(null);
        }

        public void OnConnectionError(string message) {
            _tcs?.SetException(new Exception(message));
        }

        public void OnConnectionRetry() {
        }

        public void OnConnectionResume() {
        }

        public void OnConnectionLost(string reason) {
            _tcs?.SetException(new Exception(reason));
        }

        public void OnLogin() {
        }

        public void OnLoginError(int code, string message) {
        }

        public void OnUdpInit(bool success) {
        }

        public void OnPingPong(int lagValue) {
        }

        public void OnRoomVariableUpdate(SFSRoom room) {
        }

        public void OnJoinRoom(SFSRoom room) {
        }

        public void OnExtensionResponse(string cmd, ISFSObject value) {
        }

        

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }
    }
}