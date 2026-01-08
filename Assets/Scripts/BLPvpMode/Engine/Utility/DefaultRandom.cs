using System;

namespace BLPvpMode.Engine.Utility {
    public class DefaultRandom : IRandom {
        private readonly Random _random;

        public DefaultRandom(long seed) {
            _random = new Random((int) seed);
        }

        public int RandomInt(int minInclusive, int maxExclusive) {
            return _random.Next(minInclusive, maxExclusive);
        }

        public float RandomFloat(float minInclusive, float maxExclusive) {
            return minInclusive + (float) _random.NextDouble() * (maxExclusive - minInclusive);
        }
    }
}