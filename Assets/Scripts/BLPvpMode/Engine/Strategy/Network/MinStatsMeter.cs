using System.Collections.Generic;
using System.Linq;

namespace BLPvpMode.Engine.Strategy.Network {
    public class MinStatsMeter : IStatsMeter {
        private readonly SortedDictionary<int, int> _values = new();
        public int Value => _values.Count == 0 ? 0 : _values.First().Key;

        public void Add(int value) {
            _values[value] = (_values.TryGetValue(value, out var counter) ? counter : 0) + 1;
        }

        public void Remove(int value) {
            --_values[value];
            if (_values[value] == 0) {
                _values.Remove(value);
            }
        }
    }
}