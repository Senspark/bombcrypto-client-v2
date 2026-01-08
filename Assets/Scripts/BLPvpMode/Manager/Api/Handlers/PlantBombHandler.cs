using System;
using System.Threading.Tasks;
using BLPvpMode.Engine.Data;
using CustomSmartFox;
using JetBrains.Annotations;
using PvpMode.Services;
using Services.Server.Exceptions;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

namespace BLPvpMode.Manager.Api.Handlers {
    public class PlantBombHandler : IServerHandler<IPlantBombData> {
        private readonly IExtResponseEncoder _encoder;
        
        [NotNull]
        private readonly ITimeManager _timeManager;

        [CanBeNull]
        private TaskCompletionSource<IPlantBombData> _tcs;

        [CanBeNull]
        private string _taskId;

        public PlantBombHandler(
            IExtResponseEncoder encoder,
            ITimeManager timeManager
        ) {
            _encoder = encoder;
            _timeManager = timeManager;
        }

        public async Task<IPlantBombData> Start(IServerBridge bridge) {
            if (_tcs != null) {
                return await _tcs.Task;
            }
            try {
                _tcs = new TaskCompletionSource<IPlantBombData>();
                var timestamp = _timeManager.Timestamp;
                _taskId = $"{timestamp}";
                var data = new SFSObject().Apply(it => {
                    it.PutUtfString("task_id", _taskId);
                    it.PutLong("timestamp", timestamp);
                });
                var sfsData = new SFSObject();
                sfsData.PutUtfString(SFSDefine.SFSField.Data, _encoder.EncodeData(data.ToJson()));
                bridge.Send(new ExtensionRequest(
                    SFSDefine.SFSCommand.PVP_PLANT_BOMB,
                    sfsData,
                    bridge.LastJoinedRoom,
                    false
                ));
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
        }

        public void OnPingPong(int lagValue) {
        }

        public void OnRoomVariableUpdate(SFSRoom room) {
        }

        public void OnJoinRoom(SFSRoom room) {
        }

        public void OnExtensionResponse(string cmd, ISFSObject value) {
            if (cmd != SFSDefine.SFSCommand.PVP_PLANT_BOMB) {
                return;
            }
            var taskId = value.GetUtfString("task_id");
            if (taskId != _taskId) {
                return;
            }
            var response = DefaultPvpServerBridge.PvpGenericResult.FastParse(value);
            if (response.Code == 0) {
                _tcs?.SetResult(PlantBombData.FastParse(value));
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