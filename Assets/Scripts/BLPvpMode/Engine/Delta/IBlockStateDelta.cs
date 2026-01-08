using UnityEngine;

namespace BLPvpMode.Engine.Delta {
    public interface IBlockStateDelta {
        Vector2Int Position { get; }
        long State { get; }
        long LastState { get; }
    }
}