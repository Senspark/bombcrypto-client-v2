using System;
using System.Threading.Tasks;

using App;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

using UnityEngine;

namespace BLPvpMode.Manager.Api.Handlers {
    public class ExtensionResponseHandler : IServerHandler<ISFSObject> {
        [NotNull]
        private readonly string _command;

        [NotNull]
        private readonly ISFSObject _data;

        [CanBeNull]
        private TaskCompletionSource<ISFSObject> _tcs;

        private readonly int _timeOutMs;
        private readonly ITaskDelay _taskDelay;
        private readonly bool _enableLog;
        private const int MAX_LOG_LENGTH = 900;

        public ExtensionResponseHandler(
            bool enableLog,
            [NotNull] string command,
            [NotNull] ISFSObject data,
            [NotNull] ITaskDelay taskDelay,
            float timeOut = 15f
        ) {
            _enableLog = enableLog;
            _command = command;
            _data = data;
            _taskDelay = taskDelay;
            _timeOutMs = (int)(timeOut * 1000);
        }

        public async Task<ISFSObject> Start(IServerBridge bridge) {
            if (!bridge.IsConnected) {
                throw new Exception("Not connected");
            }
            if (_tcs != null) {
                return await _tcs.Task;
            }
            try {
                _tcs = new TaskCompletionSource<ISFSObject>();
                Log(true, _command, _data);
                var data = bridge.Builder.Build(_command, _data);
                bridge.Send(new ExtensionRequest(_command, data));
                var result = _timeOutMs > 0 ? await _tcs.TimeoutAfter(_timeOutMs, _taskDelay) : await _tcs.Task;
                return result;
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
            if (_command != cmd) {
                return;
            }
            Log(false, cmd, value);
            _tcs?.SetResult(value);
        }

        

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }

        private void Log(bool send, string cmd, ISFSObject data) {
            if (!_enableLog) {
                return;
            }
            var json = data.ToJson();
            if (json.Length <= MAX_LOG_LENGTH) {
                var message = $"{(send ? "SEND" : "RECEIVED")}: {cmd} {data.ToJson()}";
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "LOG: {0}", message);
            } else {
                var message = $"{(send ? "SEND" : "RECEIVED")}: {cmd}:";
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "LOG: {0}", message);

                for (var i = 0; i < json.Length; i += MAX_LOG_LENGTH) {
                    var chunk = json.Substring(i, Math.Min(900, json.Length - i));
                    Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "{0}", chunk);
                }
            }
        }
    }
}