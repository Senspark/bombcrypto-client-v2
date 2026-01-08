using System.Collections.Generic;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Map {
    internal class ExpandResult : IExpandResult {
        public Dictionary<Vector2Int, int> DamagedPositions { get; }
        public List<IBomb> ExplodedBombs { get; }

        public ExpandResult(
            [NotNull] Dictionary<Vector2Int, int> damagedPositions,
            [ItemNotNull] [NotNull] List<IBomb> explodedBombs
        ) {
            DamagedPositions = damagedPositions;
            ExplodedBombs = explodedBombs;
        }
    }

    public class InstantExpandStrategy : IExpandStrategy {
        public IExpandResult Expand(
            IBombManager bombManager,
            IMapManager mapManager,
            IBomb bomb
        ) {
            var bombSet = new HashSet<IBomb> { bomb };
            var bombs = new List<IBomb> { bomb };
            var damagedPositions = new Dictionary<Vector2Int, int>();
            void ProcessPosition(Vector2Int position, int damage) {
                var affectedBomb = bombManager.GetBomb(position);
                if (affectedBomb != null && !bombSet.Contains(affectedBomb)) {
                    bombs.Add(affectedBomb);
                    bombSet.Add(affectedBomb);
                }
                damagedPositions[position] =
                    damagedPositions.TryGetValue(position, out var value)
                        ? Mathf.Max(damage, value)
                        : damage;
            }
            var index = 0;
            while (index < bombs.Count) {
                var item = bombs[index++];
                var position = new Vector2Int(
                    Mathf.FloorToInt(item.Position.x),
                    Mathf.FloorToInt(item.Position.y));
                var ranges = item.State.ExplodeRanges;
                var damage = item.Damage;
                ProcessPosition(position, damage);
                for (var xx = position.x - ranges[Direction.Left]; xx < position.x; ++xx) {
                    ProcessPosition(new Vector2Int(xx, position.y), damage);
                }
                for (var xx = position.x + ranges[Direction.Right]; xx > position.x; --xx) {
                    ProcessPosition(new Vector2Int(xx, position.y), damage);
                }
                for (var yy = position.y + ranges[Direction.Up]; yy > position.y; --yy) {
                    ProcessPosition(new Vector2Int(position.x, yy), damage);
                }
                for (var yy = position.y - ranges[Direction.Down]; yy < position.y; ++yy) {
                    ProcessPosition(new Vector2Int(position.x, yy), damage);
                }
            }
            return new ExpandResult(damagedPositions, bombs);
        }
    }
}