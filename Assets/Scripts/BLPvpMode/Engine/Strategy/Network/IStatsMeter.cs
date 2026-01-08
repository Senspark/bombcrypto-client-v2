namespace BLPvpMode.Engine.Strategy.Network {
    public interface IStatsMeter {
        int Value { get; }
        void Add(int value);
        void Remove(int value);
    }
}