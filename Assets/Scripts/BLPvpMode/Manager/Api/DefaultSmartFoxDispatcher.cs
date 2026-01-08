using System;
using App;
using CustomSmartFox;
using JetBrains.Annotations;
using Sfs2X.Core;

namespace BLPvpMode.Manager.Api {
    public delegate void DispatchEventDelegate(Action<IServerListener> callback);

    public class DefaultSmartFoxDispatcher: ISmartFoxDispatcher {
        [NotNull]
        private readonly ISmartFoxDispatcher _dispatcherBridge;

        public DefaultSmartFoxDispatcher(DispatchEventDelegate dispatchEvent, IExtResponseEncoder encoder) {
                _dispatcherBridge = new SmartFoxDispatcherSol(dispatchEvent, encoder);
        }

        public void OnConnection(BaseEvent evt) {
            _dispatcherBridge.OnConnection(evt);
        }

        public void OnConnectionRetry(BaseEvent evt) {
            _dispatcherBridge.OnConnectionRetry(evt);
        }

        public void OnConnectionResume(BaseEvent evt) {
            _dispatcherBridge.OnConnectionResume(evt);
        }

        public void OnConnectionLost(BaseEvent evt) {
            _dispatcherBridge.OnConnectionLost(evt);
        }

        public void OnLogin(BaseEvent evt) {
            _dispatcherBridge.OnLogin(evt);
        }

        public void OnLoginError(BaseEvent evt) {
            _dispatcherBridge.OnLoginError(evt);
        }

        public void OnUdpInit(BaseEvent evt) {
            _dispatcherBridge.OnUdpInit(evt);
        }

        public void OnPingPong(BaseEvent evt) {
            _dispatcherBridge.OnPingPong(evt);
        }

        public void OnRoomVariableUpdate(BaseEvent evt) {
            _dispatcherBridge.OnRoomVariableUpdate(evt);
        }

        public void OnRoomJoin(BaseEvent evt) {
            _dispatcherBridge.OnRoomJoin(evt);
        }

        public void OnExtensionResponse(BaseEvent evt) {
            _dispatcherBridge.OnExtensionResponse(evt);
        }
    }
}