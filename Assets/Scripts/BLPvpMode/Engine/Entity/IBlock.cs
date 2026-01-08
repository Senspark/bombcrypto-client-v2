using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public enum BlockReason {
        Null,

        /** Destroyed by bombs. */
        Exploded,

        /** Removed by falling blocks. */
        Removed,

        /** Consumed by hero. */
        Consumed,

        /** Spawn by map. */
        Spawn,

        /** Dropped by destroying a block. */
        Dropped,

        /** Falling block. */
        Falling,
    }

    public enum BlockType {
        Null,
        Hard,
        Soft,

        // Items.
        BombUp,
        FireUp,
        Boots,

        // Effects.
        Kick,
        Shield,
        Skull,

        // Rewards,
        GoldX1,
        GoldX5,
        BronzeChest,
        SilverChest,
        GoldChest,
        PlatinumChest,
    }

    public interface IBlock : IEntity {
        [NotNull]
        IBlockState State { get; }

        BlockReason Reason { get; }

        BlockType Type { get; }
        Vector2Int Position { get; }
        void ApplyState(IBlockState state);
        void TakeDamage(int amount);
        void Kill(BlockReason reason);
    }

    public static class BlockExtensions {
        public static bool IsBlock(this IBlock item) {
            return item.Type is >= BlockType.Hard and <= BlockType.Soft;
        }

        public static bool IsItem(this IBlock item) {
            return item.Type is >= BlockType.BombUp and <= BlockType.PlatinumChest;
        }

        public static bool IsChest(this IBlock item) {
            return item.Type is >= BlockType.BronzeChest and <= BlockType.PlatinumChest;
        }
    }
}