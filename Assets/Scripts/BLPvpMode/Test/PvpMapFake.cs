using System.Collections.Generic;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Utility;

using SuperTiled2Unity;

using UnityEngine;

namespace BLPvpMode.Test {
    public class MapFake : IMapInfo {
        public int PlayTime => 120000;
        public int Tileset { get; }
        public int Width { get; }
        public int Height { get; }
        public Vector2Int[] StartingPositions { get; }
        public IBlockInfo[] Blocks { get; }
        public IFallingBlockInfo[] FallingBlocks { get; }
        public float ItemBlockDropRate { get; }
        public IRandomizer<BlockType> ItemBlockRandomizer { get; }
        public SuperMapData SuperMapData { set; get; }

        public MapFake(PvpMapDetailMod pvpMapDetail) {
            StartingPositions = new Vector2Int[pvpMapDetail.StartingPositions.Length];
            for (var idx = 0; idx < pvpMapDetail.StartingPositions.Length; idx++) {
                var pos = pvpMapDetail.StartingPositions[idx];
                StartingPositions[idx] = new Vector2Int(pos[0], pos[1]);
            }
            Tileset = pvpMapDetail.Map.TileSet;
            Width = pvpMapDetail.Map.Col;
            Height = pvpMapDetail.Map.Row;
            var listBlock = new List<IBlockInfo>();
            foreach (var block in pvpMapDetail.Map.Blocks) {
                if (block.X < 0 || block.X >= Width || block.Y < 0 || block.Y >= Height) {
                    continue;
                }
                listBlock.Add(new PvpBlockMod(new Vector2Int(block.X, block.Y), block));
            }
            Blocks = listBlock.ToArray();
            SuperMapData = null;
        }
    }
}