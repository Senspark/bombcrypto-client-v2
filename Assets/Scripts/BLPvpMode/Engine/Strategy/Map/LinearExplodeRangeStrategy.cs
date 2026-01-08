using System;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Manager;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Map {
    public class LinearExplodeRangeStrategy : IExplodeRangeStrategy {
        public int GetExplodeRange(
            IMapManager manager,
            Vector2Int position,
            int range,
            bool piercing,
            Direction direction
        ) {
            (bool, bool) ProcessPosition(int xx, int yy) {
                var block = manager.GetBlock(new Vector2Int(xx, yy));
                if (block != null) {
                    if (block.Type == BlockType.Hard) {
                        return (false, false);
                    }
                    if (block.IsBlock()) {
                        return (true, piercing);
                    }
                }
                return (true, true);
            }
            var (xStep, yStep) = direction switch {
                Direction.Left => (-1, 0),
                Direction.Right => (+1, 0),
                Direction.Up => (0, +1),
                Direction.Down => (0, -1),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
            var currentRange = 0;
            while (currentRange < range) {
                var xx = position.x + xStep * (currentRange + 1);
                var yy = position.y + yStep * (currentRange + 1);
                if (0 <= xx && xx < manager.Width &&
                    0 <= yy && yy < manager.Height) {
                    var (shouldIncreaseRange, shouldProcess) = ProcessPosition(xx, yy);
                    if (shouldIncreaseRange) {
                        ++currentRange;
                    }
                    if (shouldProcess) {
                        continue;
                    }
                }
                break;
            }
            return currentRange;
        }
    }
}