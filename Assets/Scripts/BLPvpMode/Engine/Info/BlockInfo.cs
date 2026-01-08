using BLPvpMode.Engine.Entity;

using Newtonsoft.Json;

using UnityEngine;

namespace BLPvpMode.Engine.Info {
    public class BlockInfo : IBlockInfo {
        public BlockType BlockType { get; }
        public Vector2Int Position { get; }
        public int Health { get; }

        public BlockInfo(
            [JsonProperty("block_type")] int blockType,
            [JsonProperty("x")] int x,
            [JsonProperty("y")] int y,
            [JsonProperty("health")] int health) {
            BlockType = (BlockType) blockType;
            Position = new Vector2Int(x, y);
            Health = health;
        }
    }
}