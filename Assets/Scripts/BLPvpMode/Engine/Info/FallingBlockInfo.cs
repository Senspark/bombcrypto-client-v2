using Newtonsoft.Json;

using UnityEngine;

namespace BLPvpMode.Engine.Info {
    public class FallingBlockInfo : IFallingBlockInfo {
        public int Timestamp { get; }
        public Vector2Int Position { get; }

        public FallingBlockInfo(
            [JsonProperty("timestamp")] int timestamp,
            [JsonProperty("x")] int x,
            [JsonProperty("y")] int y) {
            Timestamp = timestamp;
            Position = new Vector2Int(x, y);
        }
    }
}