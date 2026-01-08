using JetBrains.Annotations;

using Sfs2X.Entities;
using Sfs2X.Entities.Data;

namespace BLPvpMode.Manager.Api {
    public interface IServerListener {
        void OnConnection();
        void OnConnectionError([NotNull] string message);
        void OnConnectionRetry();
        void OnConnectionResume();
        void OnConnectionLost([NotNull] string reason);
        void OnLogin();
        void OnLoginError(int code, [NotNull] string message);
        void OnUdpInit(bool success);
        void OnPingPong(int lagValue);
        void OnRoomVariableUpdate(SFSRoom room);
        void OnJoinRoom(SFSRoom room);
        void OnExtensionResponse([NotNull] string cmd, [NotNull] ISFSObject value);
        void OnExtensionResponse(string cmd, int requestId, byte[] data);
        void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage);
    }
}