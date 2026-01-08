using System;
using System.Threading.Tasks;

using App;

using Engine.Entities;

using Game.UI;

using UnityEngine;

using Object = UnityEngine.Object;

namespace Services {
    public class DefaultPveModeManager : IPveModeManager {
        private readonly NetworkType _networkType;

        public DefaultPveModeManager(NetworkType networkType) {
            _networkType = networkType;
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public string GetEntityPath(EntityType entityType) {
            return entityType switch {
                EntityType.normalBlock => "Prefabs/Blocks/BlockNormal",
                EntityType.jailHouse => "Prefabs/Blocks/BlockJail",
                EntityType.woodenChest => "Prefabs/Blocks/BlockWooden",
                EntityType.silverChest => "Prefabs/Blocks/BlockSilver",
                EntityType.goldenChest => "Prefabs/Blocks/BlockGolden",
                EntityType.diamondChest => "Prefabs/Blocks/BlockDiamond",
                EntityType.legendChest => "Prefabs/Blocks/BlockLegend",
                EntityType.Door => "Prefabs/Items/Door",
                EntityType.Enemy => "Prefabs/Enemies/Enemy",
                EntityType.Boss => "Prefabs/Enemies/Boss",
                EntityType.keyChest => "Prefabs/Blocks/BlockKey",
                EntityType.Item => "Prefabs/Items/Item",
                EntityType.BcoinDiamondChest => "Prefabs/Blocks/BlockDiamond",
                EntityType.BossChest => _networkType switch {
                    NetworkType.Binance => "Prefabs/Blocks/BlockBossBirthDay",
                    _=> "Prefabs/Blocks/BlockBoss"
                },
                EntityType.Prison => "Prefabs/Items/Prison",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        
        public LevelView CreateLevelView(GameModeType mode, int tileIndex, Transform parent) {
            var path = GeneratePathLevelView(mode, tileIndex);
            return Object.Instantiate(Resources.Load<LevelView>(path), parent);
        }

        private string GeneratePathLevelView(GameModeType mode, int tileIndex) {
            return _networkType switch {
                //DevHoang: Add new airdrop
                NetworkType.Binance => GetBinancePvePath(mode),
                NetworkType.Ton => GetTonPvePath(),
                NetworkType.Polygon => GetPolygonPvePath(tileIndex),
                NetworkType.Solana => GetSolanaPvePath(),
                NetworkType.Ronin => GetSolanaPvePath(),
                NetworkType.Base => GetSolanaPvePath(),
                NetworkType.Viction => GetSolanaPvePath(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static string GetBinancePvePath(GameModeType mode) {
            return mode switch {
                GameModeType.TreasureHunt => "Prefabs/Levels/Binance/LevelViewPve",
                GameModeType.TreasureHuntV2 => "Prefabs/Levels/Binance/LevelViewPveAmazon",
                _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
            };
        }

        private static string GetTonPvePath() {
            return "Prefabs/Levels/LevelViewPveTon";
        }
        
        private static string GetSolanaPvePath() {
            return "Prefabs/Levels/LevelViewPveSolana";
        }
        
        private static string GetPolygonPvePath(int tileIndex) {
            return tileIndex switch {
                0 => "Prefabs/Levels/Polygon/LevelViewPve",
                1 => "Prefabs/Levels/Polygon/LevelViewPve-chess",
                2 => "Prefabs/Levels/Polygon/LevelViewPve-ice",
                3 => "Prefabs/Levels/Polygon/LevelViewPve-sahara",
                4 => "Prefabs/Levels/Polygon/LevelViewPve-candy",
                _ => throw new ArgumentOutOfRangeException(nameof(tileIndex), tileIndex, null)
            };
        }
    }
}