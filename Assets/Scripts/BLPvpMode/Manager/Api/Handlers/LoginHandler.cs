using System;
using System.Threading.Tasks;

using BLPvpMode.Engine.Info;

using Data;

using JetBrains.Annotations;

using Newtonsoft.Json;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

namespace BLPvpMode.Manager.Api.Handlers {
    public class LoginHandler : IServerHandlerVoid {
        private readonly string _zone;

        [NotNull]
        private readonly IMatchInfo _info;

        [NotNull]
        private readonly string _username;

        [CanBeNull]
        private TaskCompletionSource<object> _tcs;

        public LoginHandler(
            [NotNull] string zone,
            [NotNull] IMatchInfo info,
            [NotNull] string username
        ) {
            _zone = zone;
            _info = info;
            _username = username;
        }

        public async Task Start(IServerBridge bridge) {
            if (_tcs != null) {
                await _tcs.Task;
                return;
            }
            try {
                _tcs = new TaskCompletionSource<object>();
                var value = JsonConvert.SerializeObject(new PvpMatchResponse {
                    Info = _info, //
                    Hash = _info.Hash,
                });
                var obj = SFSObject.NewFromJsonData(value);
                bridge.Send(new LoginRequest(_username, null, _zone, obj));
                await _tcs.Task;
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
            _tcs?.SetResult(null);
        }

        public void OnLoginError(int code, string message) {
            _tcs?.SetException(new Exception($"{message} ({code})"));
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