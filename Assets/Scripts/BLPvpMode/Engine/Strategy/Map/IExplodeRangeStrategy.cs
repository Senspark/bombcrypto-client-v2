using BLPvpMode.Engine.Manager;

using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Map {
    public interface IExplodeRangeStrategy {
        int GetExplodeRange(
            IMapManager manager,
            Vector2Int position,
            int range,
            bool piercing,
            Direction direction
        );
    }
}