using System.Threading.Tasks;
using CustomSmartFox;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;

namespace BLPvpMode.Manager.Api.Handlers {
    public class PingHandler : IServerHandlerVoid {
        private readonly IExtResponseEncoder _encoder;
        private readonly int _requestId;
        private readonly long _timestamp;
        private readonly bool _useUdp;

        public PingHandler(
            IExtResponseEncoder encoder,
            int requestId,
            long timestamp,
            bool useUdp
        ) {
            _encoder = encoder;
            _requestId = requestId;
            _timestamp = timestamp;
            _useUdp = useUdp;
        }

        public Task Start(IServerBridge bridge) {
            var data = new SFSObject().Apply(it => {
                it.PutInt("request_id", _requestId);
                it.PutLong("timestamp", _timestamp);
            });
            var sfsData = new SFSObject();
            sfsData.PutUtfString(SFSDefine.SFSField.Data, _encoder.EncodeData(data.ToJson()));;
            bridge.Send(new ExtensionRequest(
                SFSDefine.SFSCommand.PVP_PING_PONG,
                sfsData,
                bridge.LastJoinedRoom,
                _useUdp
            ));
            return Task.FromResult<object>(null);
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
        }

        public void OnExtensionError(string cmd, int requestId, int errorCode, string errorMessage) {
        }
    }
}