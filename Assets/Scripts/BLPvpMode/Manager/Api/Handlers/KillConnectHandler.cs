using System.Threading.Tasks;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api.Handlers {
    public class KillConnectHandler : IServerHandlerVoid {
        [CanBeNull]
        private TaskCompletionSource<object> _tcs;

        public Task Start(IServerBridge bridge) {
            bridge.KillConnection();
            return Task.CompletedTask;
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
    }
}