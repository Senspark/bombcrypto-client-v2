using System;
using System.Threading.Tasks;
using BLPvpMode.Engine;
using CustomSmartFox;
using JetBrains.Annotations;
using PvpMode.Services;
using Services.Server.Exceptions;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

namespace BLPvpMode.Manager.Api.Handlers {
    public class UseBoosterHandler : IServerHandlerVoid {
        private readonly IExtResponseEncoder _encoder;
        
        [NotNull]
        private readonly ITimeManager _timeManager;

        private readonly Booster _booster;

        [CanBeNull]
        private TaskCompletionSource<object> _tcs;

        [CanBeNull]
        private string _taskId;

        public UseBoosterHandler(
            IExtResponseEncoder encoder,
            ITimeManager timeManager,
            Booster booster
        ) {
            _encoder = encoder;
            _timeManager = timeManager;
            _booster = booster;
        }

        public async Task Start(IServerBridge bridge) {
            if (_tcs != null) {
                await _tcs.Task;
                return;
            }
            try {
                _tcs = new TaskCompletionSource<object>();
                var timestamp = _timeManager.Timestamp;
                _taskId = $"{timestamp}";
                var data = new SFSObject().Apply(it => {
                    it.PutUtfString("task_id", _taskId);
                    it.PutLong("timestamp", timestamp);
                    it.PutInt("item_id", (int)_booster);
                });
                var sfsData = new SFSObject();
                sfsData.PutUtfString(SFSDefine.SFSField.Data, _encoder.EncodeData(data.ToJson()));
                bridge.Send(new ExtensionRequest(
                    SFSDefine.SFSCommand.PVP_USE_BOOSTER,
                    sfsData,
                    bridge.LastJoinedRoom,
                    false
                ));
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
            if (cmd != SFSDefine.SFSCommand.PVP_USE_BOOSTER) {
                return;
            }
            var taskId = value.GetUtfString("task_id");
            if (taskId != _taskId) {
                return;
            }
            var response = DefaultPvpServerBridge.PvpGenericResult.FastParse(value);
            if (response.Code == 0) {
                _tcs?.SetResult(null);
            } else {
                _tcs?.SetException(new ErrorCodeException(response.Code, response.Message));
            }
        }

        

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }
    }
}