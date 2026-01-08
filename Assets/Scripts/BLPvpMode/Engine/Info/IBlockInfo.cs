using BLPvpMode.Engine.Entity;

using UnityEngine;

namespace BLPvpMode.Engine.Info {
    public interface IBlockInfo {
        BlockType BlockType { get; }
        Vector2Int Position { get; }
        int Health { get; }
    }
}