using Engine.Entities;

using System;

using App;

using BLPvpMode.Engine.Entity;

using Random = UnityEngine.Random;

namespace Engine.Manager {
    public enum TileType {
        Background,
        Left,
        LeftBottom,
        Right,
        RightBottom,
        BorderLeft1,
        BorderLeft2,
        BorderRight1,
        BorderRight2,
        BorderTop1,
        BorderTop2,
        BorderBottom1,
        BorderBottom2,
        BorderLeftTop,
        BorderRightTop,
        BorderLeftBottom,
        BorderRightBottom,
        OutSideLeft,
        OutSideLeftBottom,
        OutSideRight,
        OutSideRightBottom,
        Wall,
        Brick,
        Bomb,
        BombExplosion,
        Item,
        normalBlock,
        jailHouse,
        woodenChest,
        silverChest,
        goldenChest,
        diamondChest,
        legendChest,
        keyChest,
        BcoinDiamondChest,
        BossChest,
        NftChest,
        PatternLeftBottom,
        PatternLeftTop,
        PatternRightBottom,
        PatternRightTop
    }

    public class MapInfo {
        public GameModeType GameModeType = GameModeType.PvpMode;
        public int Col = 35;

        public int Row = 17;

        // ExtendMapData == null: mặc định load sort-block xen kẽ chẵn, lẽ
        public ExtendMapData ExtendData { set; get; }
    }

    public static class MapHelper {
        public static EntityType ConvertToEntityType(BlockType pvpBlockType) {
            return pvpBlockType switch {
                BlockType.Hard => EntityType.Wall,
                BlockType.Soft => EntityType.Brick,
                // Fix tutorial
                // Fix nếu có item ban đầu thì quy ước là Brick, để sau khi nổ hiện item
                BlockType.BombUp => EntityType.Brick,
                BlockType.FireUp => EntityType.Brick,
                BlockType.Boots => EntityType.Brick,
                _ => EntityType.Unknown
            };
        }

        public static int ConvertTileTypeToID(TileType tileType, bool rand) {
            return tileType switch {
                TileType.OutSideLeft => 0,
                TileType.Left => 1,
                TileType.BorderLeftTop => 2,
                TileType.BorderTop1 => 3,
                TileType.BorderTop2 => 4,
                TileType.BorderRightTop => 5,
                TileType.Right => 6,
                TileType.OutSideRight => 7,
                TileType.BorderLeft1 => 10,

                TileType.BorderRight1 => 13,
                TileType.BorderLeft2 => 18,
                TileType.Brick => 19,
                TileType.BorderRight2 => 21,
                TileType.OutSideLeftBottom => 24,
                TileType.LeftBottom => 25,
                TileType.BorderLeftBottom => 26,
                TileType.BorderBottom1 => 27,
                TileType.BorderBottom2 => 28,
                TileType.BorderRightBottom => 29,
                TileType.RightBottom => 30,
                TileType.OutSideRightBottom => 31,

                TileType.normalBlock => 19,
                TileType.jailHouse => 32,
                TileType.woodenChest => 33,
                TileType.silverChest => 34,
                TileType.goldenChest => 35,
                TileType.diamondChest => 36,
                TileType.legendChest => 37,
                TileType.keyChest => 38,
                TileType.BcoinDiamondChest => 39,
                TileType.BossChest => 14,
                TileType.NftChest => 32,

                TileType.PatternLeftBottom => 16,
                TileType.PatternLeftTop => 8,
                TileType.PatternRightBottom => 17,
                TileType.PatternRightTop => 9,

                // Random
                TileType.Background => rand ? (Random.Range(0, 2) == 0 ? 8 : 11) : 11,
                TileType.Wall => rand ? (Random.Range(0, 2) == 0 ? 9 : 12) : 12,
                _ => throw new ArgumentOutOfRangeException(nameof(tileType), tileType, null)
            };
        }

        public static TileType ConvertIdToTileType(int tileId) {
            switch (tileId) {
                case 11:
                    return TileType.Background;
                case 19:
                    return TileType.Brick;
                case 32:
                    return TileType.jailHouse;
                case 33:
                    return TileType.woodenChest;
                case 34:
                    return TileType.silverChest;
                case 35:
                    return TileType.goldenChest;
                case 36:
                    return TileType.diamondChest;
                case 37:
                    return TileType.legendChest;
                case 38:
                    return TileType.keyChest;
                case 39:
                    return TileType.BcoinDiamondChest;

                default:
                    return TileType.Wall;
            }
        }

        public static TileType ConvertEntityTypeToTileType(EntityType entityType) {
            switch (entityType) {
                case EntityType.normalBlock: return TileType.normalBlock;
                case EntityType.jailHouse: return TileType.jailHouse;
                case EntityType.woodenChest: return TileType.woodenChest;
                case EntityType.silverChest: return TileType.silverChest;
                case EntityType.goldenChest: return TileType.goldenChest;
                case EntityType.diamondChest: return TileType.diamondChest;
                case EntityType.legendChest: return TileType.legendChest;
                case EntityType.Brick: return TileType.Brick;
                case EntityType.keyChest: return TileType.keyChest;
                case EntityType.BcoinDiamondChest: return TileType.BcoinDiamondChest;
                case EntityType.BossChest: return TileType.BossChest;
                case EntityType.NftChest: return TileType.NftChest;
                default:
                    return TileType.Brick;
            }
        }
    }

    public class DefaultMapManager {
        public const int R_COL = 35;
        public const int R_ROW = 17;
        private const int C_COL = R_COL + 4; // (border left 2 + right 2)
        private const int C_ROW = R_ROW + 2; // (border top 1 + bottm 1)
        private const float TILE_SIZE = 1;
    }
}