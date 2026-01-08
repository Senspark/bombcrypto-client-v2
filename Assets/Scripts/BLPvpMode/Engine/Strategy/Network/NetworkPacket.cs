namespace BLPvpMode.Engine.Strategy.Network {
    public class NetworkPacket : INetworkPacket {
        private long _responseTimestamp;
        private long _clientTimestamp;
        public long RequestTimestamp { get; }
        public int Latency => (int) (_responseTimestamp - RequestTimestamp);
        public int TimeDelta => (int) (_responseTimestamp - _clientTimestamp - Latency / 2);

        public NetworkPacket(long requestTimestamp) {
            RequestTimestamp = requestTimestamp;
        }

        public void Pong(long serverTimestamp, long clientTimestamp) {
            _responseTimestamp = serverTimestamp;
            _clientTimestamp = clientTimestamp;
        }
    }
}