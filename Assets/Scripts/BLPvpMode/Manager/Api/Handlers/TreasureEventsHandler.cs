using System.Threading.Tasks;

using App;

using Senspark;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api.Handlers {
    // TREASURE_EVENTS push (server-driven treasure mode): parse and hand to the scene's playback.
    public class TreasureEventsHandler : IServerHandlerVoid {
        private readonly App.IServerDispatcher _serverDispatcher;
        private readonly ILogManager _logManager;

        public TreasureEventsHandler(App.IServerDispatcher serverDispatcher, ILogManager logManager) {
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
            if (cmd != SFSDefine.SFSCommand.TREASURE_EVENTS) {
                return;
            }
            if (value.ContainsKey("code") && value.GetInt("code") != 0) {
                _logManager.Log($"[TREASURE] events error code={value.GetInt("code")} message={value.GetUtfString("message")}");
                return;
            }
            try {
                var events = DefaultPveServerBridge.ParseTreasureEvents(value);
                if (events.Count == 0) {
                    return;
                }
                _serverDispatcher.DispatchEvent(e => e.OnTreasureEvents?.Invoke(events));
                // Session stats listen to OnPveExploded; the scene applies blasts through its own playback.
                foreach (var ev in events) {
                    if (ev.Explode != null) {
                        _serverDispatcher.DispatchEvent(e => e.OnPveExploded?.Invoke(ev.Explode));
                    }
                }
            } catch (System.Exception e) {
                _logManager.Log($"[TREASURE] events parse failed: {e}");
            }
        }

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }

        public Task Start(IServerBridge bridge) {
            return Task.CompletedTask;
        }
    }
}
