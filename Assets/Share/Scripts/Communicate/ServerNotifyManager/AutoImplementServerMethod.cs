using System.Threading.Tasks;

using BLPvpMode.Manager.Api;

using Sfs2X.Entities;

namespace Share.Scripts.Communicate {
    public abstract class AutoImplementServerMethod {
        protected abstract IServerListener Instance();
        
        public void OnConnection() {
            if (Instance() != null)
                Instance().OnConnection();
        }

        public void OnConnectionError(string message) {
            if (Instance() != null)
                Instance().OnConnectionError(message);
        }
        public void OnConnectionRetry() {
            if (Instance() != null)
                Instance().OnConnectionRetry();
        }
        public void OnConnectionResume() {
            if (Instance() != null)
                Instance().OnConnectionResume();
        }
        public void OnConnectionLost(string reason) {
            if (Instance() != null)
                Instance().OnConnectionLost(reason);
        }
        public void OnLogin() {
            if (Instance() != null)
                Instance().OnLogin();
        }
        public void OnLoginError(int code, string message) {
            if (Instance() != null)
                Instance().OnLoginError(code, message);
        }
        public void OnUdpInit(bool success) {
            if (Instance() != null)
                Instance().OnUdpInit(success);
        }
        public void OnPingPong(int lagValue) {
            if (Instance() != null)
                Instance().OnPingPong(lagValue);
        }
        public void OnRoomVariableUpdate(SFSRoom room) {
            if (Instance() != null)
                Instance().OnRoomVariableUpdate(room);
        }
        public void OnJoinRoom(SFSRoom room) {
            if (Instance() != null)
                Instance().OnJoinRoom(room);
        }
    }
}