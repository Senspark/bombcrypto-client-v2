using System;
using System.Collections.Generic;
using System.Linq;

using App;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Utility;

using Newtonsoft.Json;

using UnityEngine;

namespace BLPvpMode.Engine.Info {
    public class MapInfo : IMapInfo {
        public int PlayTime { get; }
        public int Tileset { get; }
        public int Width { get; }
        public int Height { get; }
        public Vector2Int[] StartingPositions { get; }
        public IBlockInfo[] Blocks { get; }
        public IFallingBlockInfo[] FallingBlocks { get; }
        public float ItemBlockDropRate { get; }
        public IRandomizer<BlockType> ItemBlockRandomizer { get; }

        public MapInfo(
            [JsonProperty("play_time")] int playTime,
            [JsonProperty("tileset")] int tileset,
            [JsonProperty("width")] int width,
            [JsonProperty("height")] int height,
            [JsonProperty("starting_positions")] Dictionary<string, int>[] startingPositions,
            [JsonProperty("blocks", ItemConverterType = typeof(ConcreteTypeConverter<BlockInfo>))]
            IBlockInfo[] blocks,
            IFallingBlockInfo[] fallingBlocks = null,
            float itemBlockDropRate = 0f,
            IRandomizer<BlockType> itemBlockRandomizer = null
        ) {
            PlayTime = playTime;
            Tileset = tileset;
            Width = width;
            Height = height;
            StartingPositions = startingPositions
                .Select(item => new Vector2Int(item["first"], item["second"]))
                .ToArray();
            Blocks = blocks;
            FallingBlocks = fallingBlocks ?? Array.Empty<IFallingBlockInfo>();
            ItemBlockDropRate = itemBlockDropRate;
            ItemBlockRandomizer = itemBlockRandomizer ?? new WeightedRandomizer<BlockType>(
                Array.Empty<BlockType>(),
                Array.Empty<float>()
            );
        }
    }
}