namespace BLPvpMode.Engine.Strategy.Network {
    public class MeanStatsMeter : IStatsMeter {
        private int _sum = 0;
        private int _size = 0;

        public int Value => _size == 0 ? 0 : _sum / _size;

        public void Add(int value) {
            _sum += value;
            ++_size;
        }

        public void Remove(int value) {
            _sum -= value;
            --_size;
        }
    }
}