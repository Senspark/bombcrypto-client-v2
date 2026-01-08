using System;
using System.Threading.Tasks;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api.Handlers {
    public class InitUdpHandler : IServerHandler<bool> {
        [NotNull]
        private readonly string _host;

        private readonly int _port;

        [CanBeNull]
        private TaskCompletionSource<bool> _tcs;

        public InitUdpHandler(
            [NotNull] string host,
            int port
        ) {
            _host = host;
            _port = port;
        }

        public async Task<bool> Start(IServerBridge bridge) {
            if (_tcs != null) {
                return await _tcs.Task;
            }
            try {
                _tcs = new TaskCompletionSource<bool>();
                bridge.InitUdp(_host, _port);
                return await _tcs.Task;
            } finally {
                _tcs = null;
            }
        }

        public void OnConnection() {
        }

        public void OnConnectionError(string message) {
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
            _tcs?.SetResult(success);
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