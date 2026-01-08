using UnityEngine;

namespace BLPvpMode.Engine.Info {
    public interface IFallingBlockInfo {
        int Timestamp { get; }
        Vector2Int Position { get; }
    }
}