using System;

using App;

using CustomSmartFox;

using JetBrains.Annotations;

using Senspark;

using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api {
    public class SmartFoxDispatcherSol : ISmartFoxDispatcher {
        [NotNull]
        private readonly DispatchEventDelegate _dispatchEvent;

        [NotNull]
        private readonly IExtResponseEncoder _encoder;

        public SmartFoxDispatcherSol(DispatchEventDelegate dispatchEvent, IExtResponseEncoder encoder) {
            _dispatchEvent = dispatchEvent;
            _encoder = encoder;
        }

        public void OnConnection(BaseEvent evt) {
            var success = (bool)evt.Params["success"];
            if (success) {
                _dispatchEvent(listener => listener.OnConnection());
            } else {
                var message = (string)evt.Params["errorMessage"];
                _dispatchEvent(listener => listener.OnConnectionError(message));
            }
        }

        public void OnConnectionRetry(BaseEvent evt) {
            _dispatchEvent(listener => listener.OnConnectionRetry());
        }

        public void OnConnectionResume(BaseEvent evt) {
            _dispatchEvent(listener => listener.OnConnectionResume());
        }

        public void OnConnectionLost(BaseEvent evt) {
            var reason = (string)evt.Params["reason"];
            _dispatchEvent(listener => listener.OnConnectionLost(reason));
        }

        public void OnLogin(BaseEvent evt) {
            _dispatchEvent(listener => listener.OnLogin());
        }

        public void OnLoginError(BaseEvent evt) {
            var code = (short)evt.Params["errorCode"];
            var message = (string)evt.Params["errorMessage"];
            _dispatchEvent(listener => listener.OnLoginError(code, message));
        }

        public void OnUdpInit(BaseEvent evt) {
            var success = (bool)evt.Params["success"];
            _dispatchEvent(listener => listener.OnUdpInit(success));
        }

        public void OnPingPong(BaseEvent evt) {
            var lagValue = (int)evt.Params["lagValue"];
            _dispatchEvent(listener => listener.OnPingPong(lagValue));
        }

        public void OnRoomVariableUpdate(BaseEvent evt) {
            var value = (SFSRoom)evt.Params["room"];
            _dispatchEvent(listener => listener.OnRoomVariableUpdate(value));
        }

        public void OnRoomJoin(BaseEvent evt) {
            var value = (SFSRoom)evt.Params["room"];
            _dispatchEvent(listener => listener.OnJoinRoom(value));
        }

        public void OnExtensionResponse(BaseEvent evt) {
            var cmd = (string)evt.Params["cmd"];
            var value = (ISFSObject)evt.Params["params"];
            byte[] data = null;

            //Do server chủ động gửi về nên ko có request id
            if (!value.ContainsKey(SFSDefine.SFSField.NewRequestId)) {
                if (cmd != SFSDefine.SFSCommand.USER_LOGIN) {
                    if (value.ContainsKey(SFSDefine.SFSField.Data)) {
                        data = value.GetByteArray(SFSDefine.SFSField.Data).Bytes;
                        var (outData, json) = _encoder.DecodeDataToSfsObject(data);
                        _dispatchEvent(listener => listener.OnExtensionResponse(cmd, outData));
                    } else {
                        var outData = new SFSObject();
                        _dispatchEvent(listener => listener.OnExtensionResponse(cmd, outData));
                    }
                    return;
                }
            }

            var errorCode = 0;
            var requestId = value.GetInt(SFSDefine.SFSField.NewRequestId);

            if (value.ContainsKey(SFSDefine.SFSField.ErrorCode)) {
                errorCode = value.GetInt(SFSDefine.SFSField.ErrorCode);
            }
            if (value.ContainsKey(SFSDefine.SFSField.Data)) {
                data = value.GetByteArray(SFSDefine.SFSField.Data).Bytes;
            }
            if (errorCode > 0) {
                string errorMessage = ServerUtils.GetErrorMessage(value);
                _dispatchEvent(listener => listener.OnExtensionError(cmd, requestId, errorCode, errorMessage));
            } else {
                _dispatchEvent(listener => listener.OnExtensionResponse(cmd, requestId, data));
            }
        }
    }
}