using System;
using System.Threading.Tasks;

using App;

using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

using UnityEngine;

using Debug = UnityEngine.Debug;
using IServerBridge = BLPvpMode.Manager.Api.IServerBridge;

namespace Services.Server.Handlers {
    public class LegacyExtensionHandler<T> : IServerHandler<T> {
        private readonly bool _enableLog;
        private readonly string _cmd;
        private readonly ISFSObject _data;
        private readonly int _timeOutMs;
        private readonly ITaskDelay _taskDelay;
        private readonly ResponseParser<T> _responseParser;

        public ISFSObject ResponseObject { get; private set; }
        public ResponseParser<T> ResponseParser => _responseParser;

        [CanBeNull]
        private TaskCompletionSource<T> _tcs;

        [CanBeNull]
        private IServerBridge _bridge;

        public LegacyExtensionHandler(
            bool enableLog,
            [NotNull] string cmd,
            [NotNull] ISFSObject data,
            [NotNull] ITaskDelay taskDelay,
            [NotNull] ResponseParser<T> responseParser,
            float timeOut = 15f
        ) {
            _enableLog = enableLog;
            _cmd = cmd;
            _data = data;
            _taskDelay = taskDelay;
            _timeOutMs = (int)(timeOut * 1000);
            _responseParser = responseParser;
        }

        public async Task<T> Start(IServerBridge bridge) {
            if (!bridge.IsConnected) {
                throw new Exception("Not connected");
            }
            if (_tcs != null) {
                return await _tcs.Task;
            }
            try {
                _tcs = new TaskCompletionSource<T>();
                Log(true, _cmd, _data);
                _bridge = bridge;
                _bridge.Send(new ExtensionRequest(_cmd, _data));
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
            // resend message

            // disable
            // if (_tcs == null) {
            //     return;
            // }
            // if (!_tcs.Task.IsCompleted && _bridge != null) {
            //     Log(true, _cmd, _data);
            //     _bridge.Send(new ExtensionRequest(_cmd, _data));
            // }
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

        public void OnExtensionResponse(string cmd, ISFSObject data) {
            if (_cmd != cmd) {
                return;
            }
            if (_tcs == null) {
                // maybe because of time out
                return;
            }
            var requestId = _data.GetInt(SFSDefine.SFSField.Id);
            if (requestId != 0) {
                if (data.ContainsKey(SFSDefine.SFSField.RequestId)) {
                    try {
                        var timestamp = data.GetInt(SFSDefine.SFSField.RequestId);
                        if (requestId != timestamp) {
                            return;
                        }
                    } catch (Exception e) {
                        Debug.LogError(e.Message);
                    }
                }
            }
            Log(false, cmd, data);
            if (ServerUtils.HasError(data)) {
                ResponseObject = null;
                _tcs.TrySetException(new Exception(ServerUtils.GetErrorMessage(data)));
            } else {
                ResponseObject = data;
                _tcs.TrySetResult(_responseParser(cmd, data));
            }
        }

        

        public void OnExtensionResponse(string cmd, int requestId, byte[] data) {
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }

        private void Log(bool send, string cmd, ISFSObject data) {
            if (!_enableLog) {
                return;
            }
            var message = $"{(send ? "SEND" : "RECEIVED")}: {cmd} {data.ToJson()}";
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "LOG: {0}", message);
        }
    }
}