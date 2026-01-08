using System;
using System.Linq;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Utility {
    public class WeightedRandomizer<T> : IRandomizer<T> {
        [NotNull]
        private readonly T[] _items;

        [NotNull]
        private readonly float[] _weights;

        private readonly float _sum;

        public WeightedRandomizer(
            [NotNull] T[] items,
            [NotNull] float[] weights
        ) {
            _items = items;
            _weights = weights;
            _sum = weights.Sum();
        }

        public T Random(IRandom random) {
            var r = random.RandomFloat(0, _sum);
            for (var i = 0; i < _weights.Length; ++i) {
                if (r < _weights[i]) {
                    return _items[i];
                }
                r -= _weights[i];
            }
            throw new Exception("Could not random");
        }
    }
}