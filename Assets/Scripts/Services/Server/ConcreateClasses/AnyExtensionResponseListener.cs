using BLPvpMode.Manager.Api;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace App {
    public class AnyExtensionResponseListener : IServerListener {
        private readonly IServerDispatcher _serverDispatcher;

        public AnyExtensionResponseListener(IServerDispatcher serverDispatcher) {
            _serverDispatcher = serverDispatcher;
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
            _serverDispatcher.DispatchEvent(e => e.OnExtensionResponse?.Invoke(cmd, value));
        }

        

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }
    }
}