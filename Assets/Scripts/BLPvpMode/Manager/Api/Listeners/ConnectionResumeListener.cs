using System;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api.Listeners {
    public class ConnectionResumeListener : IServerListener {
        [NotNull]
        private readonly Action _callback;

        public ConnectionResumeListener([NotNull] Action callback) {
            _callback = callback;
        }

        public void OnConnection() {
        }

        public void OnConnectionError(string message) {
        }

        public void OnConnectionRetry() {
        }

        public void OnConnectionResume() {
            _callback.Invoke();
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
    }
}