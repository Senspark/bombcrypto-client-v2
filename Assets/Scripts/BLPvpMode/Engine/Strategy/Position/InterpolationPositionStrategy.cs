using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Strategy.Position {
    public class InterpolationPositionStrategy : IPositionStrategy {
        private readonly int _maxInterpolationTimeframe;
        private readonly int _minInterpolationSnapshots;
        private readonly int _maxExtrapolationTimeframe;
        private readonly List<(int, Vector2)> _entries;

        public InterpolationPositionStrategy(
            int maxInterpolationTimeframe,
            int minInterpolationSnapshots,
            int maxExtrapolationTimeframe
        ) {
            _maxInterpolationTimeframe = maxInterpolationTimeframe;
            _minInterpolationSnapshots = minInterpolationSnapshots;
            _maxExtrapolationTimeframe = maxExtrapolationTimeframe;
            _entries = new List<(int, Vector2)>();
        }

        public Vector2 GetPosition(int timestamp) {
            var (canInterpolate, interpolatedPosition) = InterpolatePosition(timestamp);
            if (canInterpolate) {
                return interpolatedPosition;
            }
            var (canExtrapolate, extrapolatedPosition) = ExtrapolatePosition(timestamp);
            return extrapolatedPosition;
        }

        private (bool, Vector2) InterpolatePosition(int timestamp) {
            Assert.IsTrue(_entries.Count > 0, "Cannot interpolate position");
            for (var i = 0; i < _entries.Count; ++i) {
                var (entryTimestamp, position) = _entries[i];
                if (entryTimestamp > timestamp) {
                    continue;
                }
                if (i == 0) {
                    return (entryTimestamp == timestamp, position);
                }
                var (prevEntryTimestamp, prevPosition) = _entries[i - 1];
                if (position == prevPosition) {
                    return (true, position);
                }
                var t = 1f * (timestamp - entryTimestamp) / (prevEntryTimestamp - entryTimestamp);
                var interpolatedX = Lerp(position.x, prevPosition.x, t);
                var interpolatedY = Lerp(position.y, prevPosition.y, t);
                return (true, new Vector2(interpolatedX, interpolatedY));
            }
            var (_, firstPosition) = _entries[0];
            return (false, firstPosition);
        }

        private (bool, Vector2) ExtrapolatePosition(int timestamp) {
            Assert.IsTrue(_entries.Count > 0, "Cannot extrapolate position");
            var (firstTimestamp, firstPosition) = _entries[0];
            if (_entries.Count == 1) {
                return (false, firstPosition);
            }
            var delta = timestamp - firstTimestamp;
            Assert.IsTrue(delta > 0, "Cannot extrapolate when can interpolate");
            delta = Mathf.Min(delta, _maxExtrapolationTimeframe);
            var (secondTimestamp, secondPosition) = _entries[1];
            var t = 1f * delta / (firstTimestamp - secondTimestamp);
            var extrapolatedX = Lerp(secondPosition.x, firstPosition.x, t);
            var extrapolatedY = Lerp(secondPosition.y, firstPosition.y, t);
            return (true, new Vector2(extrapolatedX, extrapolatedY));
        }

        public void AddPosition(int timestamp, Vector2 position) {
            _entries.Add((timestamp, position));
            _entries.Sort((lhs, rhs) => rhs.Item1.CompareTo(lhs.Item1));
            while (_entries.Count > _minInterpolationSnapshots &&
                   _entries[0].Item1 - _entries[^1].Item1 > _maxInterpolationTimeframe) {
                _entries.RemoveAt(_entries.Count - 1);
            }
        }

        private float Lerp(float a, float b, float t) {
            return a + (b - a) * t;
        }
    }
}