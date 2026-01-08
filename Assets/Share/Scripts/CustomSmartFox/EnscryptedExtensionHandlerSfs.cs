using System;
using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CustomSmartFox {
    public class EncryptedExtensionHandlerSfs : IServerHandler<ISFSObject> {
        private readonly IExtResponseEncoder _encoder;
        private readonly bool _enableLog;
        private readonly string _cmd;
        private readonly int _requestId;
        private readonly string _data;

        [CanBeNull]
        private UniTaskCompletionSource<ISFSObject> _tcs;

        [CanBeNull]
        private IServerBridge _bridge;

        public EncryptedExtensionHandlerSfs(
            IExtResponseEncoder encoder,
            int requestId,
            bool enableLog,
            string cmd,
            string data
        ) {
            _encoder = encoder;
            _enableLog = enableLog;
            _cmd = cmd;
            _requestId = requestId;
            _data = data;
        }
        
        

        public async Task<ISFSObject> Start(IServerBridge bridge) {
            if (!bridge.IsConnected) {
                throw new Exception("Not connected");
            }
            if (_tcs != null) {
                await _tcs.Task;
            }
            try {
                _tcs = new UniTaskCompletionSource<ISFSObject>();
                Log(true, _data);
                _bridge = bridge;
                var sfsData = new SFSObject();
                sfsData.PutUtfString(SFSDefine.SFSField.Data, _data != null ? _encoder.EncodeData(_data) : string.Empty);
                sfsData.PutInt(SFSDefine.SFSField.NewRequestId, _requestId);
                _bridge.Send(new ExtensionRequest(_cmd, sfsData));
                var result = await _tcs.Task;
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
            if (_cmd != cmd || _requestId != requestId || _tcs == null) {
                return;
            }

            if (data.Length == 0) {
                Log(false, "NO-DATA");
                _tcs.TrySetResult(default);
                return;
            }

            try {
                var (outData, json) = _encoder.DecodeDataToSfsObject(data);
                Log(false, json);
                _tcs.TrySetResult(outData);
            } catch (Exception e) {
                _tcs.TrySetException(e);
            }
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
            if (_cmd != cmd || _requestId != requestId || _tcs == null) {
                return;
            }
            // TODO: error code sẽ được dùng để lấy Message cho đúng
            Log(false, $"ERROR({errorCode}) {errorMessage}");
            _tcs.TrySetException(new ExtensionException(errorCode, errorMessage));
        }

        private void Log(bool send, string data) {
            if (!_enableLog) {
                return;
            }
            var message = $"{(send ? "SEND" : "RECEIVED")}: {_cmd}({_requestId}) {data}";
            Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "LOG: {0}", message);
        }
    }
}