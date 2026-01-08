using System.Collections.Generic;

using BLPvpMode.Engine.Entity;

namespace BLPvpMode.Engine.Manager {
    public class DefaultBlockHealthManager : IBlockHealthManager {
        private readonly Dictionary<BlockType, int> _data = new() {
            [BlockType.Hard] = 99999, //
            [BlockType.Soft] = 1,
        };

        public int GetHealth(BlockType type) {
            return _data[type];
        }
    }
}