using System.Collections.Generic;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;

using Engine.Entities;

using Newtonsoft.Json;

using PvpMode.Services;

using UnityEngine;

using Random = UnityEngine.Random;

namespace BLPvpMode.Test {
    public class PvpBlockStateMod : IBlockState {
        public bool IsAlive => true;

        public BlockReason Reason => BlockReason.Null;

        public BlockType Type {
            get {
                return _itemType switch {
                    (int) ItemType.BombUp => BlockType.BombUp,
                    (int) ItemType.FireUp => BlockType.FireUp,
                    (int) ItemType.Boots => BlockType.Boots,
                    -1 => (BlockType) _blockType,
                    _ => BlockType.Null
                };
            }
        }

        public int Health => 1;
        public int MaxHealth => 1;

        public int Slot => 0;
        public int Id => 0;
        public int Range => 0;
        public int Damage => 0;
        public bool Piercing => false;
        public int ExplodeDuration => 3000;
        public int PlantTimestamp => 0;

        public Dictionary<Direction, int> ExplodeRanges => null;

        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        [JsonProperty("blockType")]
        private int _blockType = (int) BlockType.Soft;

        [JsonProperty("itemType")]
        private int _itemType = -1;

        public void SetItemType(int itemType) {
            _itemType = itemType;
        }

        public long Encode() {
            throw new System.NotImplementedException();
        }
    }

    public class PvpBlockMod : IBlockInfo {
        public BlockType BlockType { get; }
        public Vector2Int Position { get; }
        public int Health { get; }

        public PvpBlockMod(Vector2Int position, IBlockState state) {
            Position = position;
            BlockType = state.Type;
            Health = state.Health;
        }
    }

    public class PvpMapMod {
        [JsonProperty("col")]
        public int Col { get; set; }

        [JsonProperty("row")]
        public int Row { get; set; }

        [JsonProperty("tileset")]
        public int TileSet { get; set; }

        [JsonProperty("blocks")]
        public PvpBlockStateMod[] Blocks { get; set; }

        [JsonProperty("isGenItem")]
        public bool IsGenItem { get; set; } = false;

        public void TryGenItem() {
            if (!IsGenItem) {
                return;
            }
            foreach (var block in Blocks) {
                if (block.Type != BlockType.Soft) {
                    continue;
                }
                if (block.X <= 4 && block.Y >= Row - 4) {
                    continue;
                }
                var ran = Random.Range(0, 100);
                switch (ran) {
                    case >= 40:
                        continue;
                    case < 10:
                        block.SetItemType((int) ItemType.FireUp);
                        break;
                    case < 25:
                        block.SetItemType((int) ItemType.BombUp);
                        break;
                    default:
                        block.SetItemType((int) ItemType.Boots);
                        break;
                }
            }
        }
    }

    public class PvpMapDetailMod {
        [JsonProperty("heroes")]
        public PvPHeroDetailMod[] Heroes { get; set; }

        [JsonProperty("map")]
        public PvpMapMod Map { get; set; }

        [JsonProperty("startingPositions")]
        public int[][] StartingPositions { get; set; }

        public void SetEmptyMap(int col, int row) {
            Map.Col = col;
            Map.Row = row;
            Map.Blocks = new PvpBlockStateMod[] { };
            StartingPositions = new int[][] { new int[] { 0, row - 1 }, new int[] { col - 1, 0 } };
        }
    }
}