using Sfs2X.Core;

namespace BLPvpMode.Manager.Api {
    public interface ISmartFoxDispatcher {
        void OnConnection(BaseEvent evt);
        void OnConnectionRetry(BaseEvent evt);
        void OnConnectionResume(BaseEvent evt);
        void OnConnectionLost(BaseEvent evt);
        void OnLogin(BaseEvent evt);
        void OnLoginError(BaseEvent evt);
        void OnUdpInit(BaseEvent evt);
        void OnPingPong(BaseEvent evt);
        void OnRoomVariableUpdate(BaseEvent evt);
        void OnRoomJoin(BaseEvent evt);
        void OnExtensionResponse(BaseEvent evt);
    }
}