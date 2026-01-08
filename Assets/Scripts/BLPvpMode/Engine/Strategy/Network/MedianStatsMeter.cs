using System.Collections.Generic;

namespace BLPvpMode.Engine.Strategy.Network {
    public class MedianStatsMeter : IStatsMeter {
        private bool _dirty = false;
        private readonly List<int> _values = new();

        public int Value {
            get {
                if (_dirty) {
                    _dirty = false;
                    _values.Sort();
                }
                return _values.Count == 0 ? 0 : _values[_values.Count / 2];
            }
        }

        public void Add(int value) {
            _values.Add(value);
            _dirty = true;
        }

        public void Remove(int value) {
            _values.Remove(value);
            _dirty = true;
        }
    }
}