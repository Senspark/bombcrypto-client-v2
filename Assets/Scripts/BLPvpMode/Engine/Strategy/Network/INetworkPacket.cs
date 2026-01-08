namespace BLPvpMode.Engine.Strategy.Network {
    public interface INetworkPacket {
        long RequestTimestamp { get; }
        int Latency { get; }
        int TimeDelta { get; }
        void Pong(long serverTimestamp, long clientTimestamp);
    }
}