using BLPvpMode.Engine.Entity;

namespace BLPvpMode.Engine.Manager {
    public interface IBlockHealthManager {
        int GetHealth(BlockType type);
    }
}