namespace BLPvpMode.Manager {
    public interface INetworkStats {
        int GetLatency(int slot);
        int GetTimeDelta(int slot);
    }
}