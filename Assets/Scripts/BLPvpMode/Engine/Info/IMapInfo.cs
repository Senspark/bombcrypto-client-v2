using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Utility;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Info {
    public interface IMapInfo {
        int PlayTime { get; }
        int Tileset { get; }
        int Width { get; }
        int Height { get; }

        [NotNull]
        Vector2Int[] StartingPositions { get; }

        [NotNull]
        IBlockInfo[] Blocks { get; }

        [NotNull]
        IFallingBlockInfo[] FallingBlocks { get; }

        float ItemBlockDropRate { get; }

        [NotNull]
        IRandomizer<BlockType> ItemBlockRandomizer { get; }
    }
}