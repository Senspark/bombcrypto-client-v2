namespace BLPvpMode.Engine.Entity {
    public interface IBlockState : IEntityState {
        BlockReason Reason { get; }
        BlockType Type { get; }
        int Health { get; }
        int MaxHealth { get; }
        long Encode();
    }
}