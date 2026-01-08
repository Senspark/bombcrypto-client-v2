using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

using UnityEngine;

namespace Services.Server.Handlers {
    public class LegacyExtensionHandler : IServerHandlerVoid {
        private readonly bool _enableLog;
        private readonly string _cmd;
        private readonly ISFSObject _data;

        public LegacyExtensionHandler(
            bool enableLog,
            [NotNull] string cmd,
            [NotNull] ISFSObject data) {
            _enableLog = enableLog;
            _cmd = cmd;
            _data = data;
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

        public Task Start(IServerBridge bridge) {
            Log(_cmd, _data);
            bridge.Send(new ExtensionRequest(_cmd, _data));
            return Task.CompletedTask;
        }

        private void Log(string cmd, ISFSObject data) {
            if (!_enableLog) {
                return;
            }
            var message = $"SEND EXTENSION: {cmd} {data.ToJson()}";
            Debug.Log(message);
        }
    }
}