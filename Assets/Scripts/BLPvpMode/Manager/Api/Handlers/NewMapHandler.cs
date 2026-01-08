using System.Threading.Tasks;

using App;

using Engine.Manager;

using PvpMode.Services;

using Senspark;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api.Handlers {
    public class NewMapHandler : IServerHandlerVoid {
        private App.IServerDispatcher _serverDispatcher;
        private ILogManager _logManager;

        public NewMapHandler(App.IServerDispatcher serverDispatcher, ILogManager logManager) {
            _serverDispatcher = serverDispatcher;
            _logManager = logManager;
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
            if (cmd != SFSDefine.SFSCommand.PVE_NEW_MAP) {
                return;
            }
            var response = DefaultPvpServerBridge.PvpGenericResult.FastParse(value);
            _logManager.Log($"[PVE_NEW_MAP] New map response: {response.Code} - {response.Message}");
            ;
            var result = response.Code == 0;
            _serverDispatcher.DispatchEvent(e => e.OnNewMapResponse?.Invoke(result));
        }

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
            if (cmd != SFSDefine.SFSCommand.PVE_NEW_MAP) {
                return;
            }
            _serverDispatcher.DispatchEvent(e => e.OnNewMapResponse?.Invoke(true));
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
            if (cmd != SFSDefine.SFSCommand.PVE_NEW_MAP) {
                return;
            }
            _serverDispatcher.DispatchEvent(e => e.OnNewMapResponse?.Invoke(false));
        }

        public Task Start(IServerBridge bridge) {
            return Task.CompletedTask;
        }
    }
}