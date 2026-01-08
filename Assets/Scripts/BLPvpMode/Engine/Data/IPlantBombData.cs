using UnityEngine;

namespace BLPvpMode.Engine.Data {
    public interface IPlantBombData {
        int Id { get; }
        Vector2Int Position { get; }
        int PlantTimestamp { get; }
    }
}