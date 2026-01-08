using BLPvpMode.Engine.Utility;

namespace BLPvpMode.Engine.Entity {
    public class BlockState : IBlockState {
        public static IBlockState Decode(long state) {
            var decoder = new LongBitDecoder(state);
            return new BlockState(
                type: (BlockType) decoder.PopInt(5),
                isAlive: decoder.PopBoolean(),
                health: decoder.PopInt(4),
                maxHealth: decoder.PopInt(4),
                reason: (BlockReason) decoder.PopInt(3)
            );
        }

        public bool IsAlive { get; }
        public BlockReason Reason { get; }
        public BlockType Type { get; }
        public int Health { get; }
        public int MaxHealth { get; }

        public BlockState(
            bool isAlive,
            BlockReason reason,
            BlockType type,
            int health,
            int maxHealth
        ) {
            IsAlive = isAlive;
            Reason = reason;
            Type = type;
            Health = health;
            MaxHealth = maxHealth;
        }

        public long Encode() {
            var encoder = new LongBitEncoder()
                .Push((int) Type, 5)
                .Push(IsAlive)
                .Push(Health, 4)
                .Push(MaxHealth, 4)
                .Push((int) Reason, 3);
            return encoder.Value;
        }
    }
}