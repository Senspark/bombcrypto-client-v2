using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using BLPvpMode.Engine.Config;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Strategy.FallingBlock;
using BLPvpMode.Engine.Utility;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    internal static class StringExtensions {
        [NotNull]
        public static string StripMargin([NotNull] this string s) {
            return Regex.Replace(s, @"[ \t]+\|", string.Empty);
        }
    }

    public class DefaultMapGenerator : IMapGenerator {
        [NotNull]
        private readonly IMapConfig _config;

        private readonly float _itemBlockDensity;

        [NotNull]
        private readonly BlockType[] _itemBlockTypes;

        [NotNull]
        private readonly float[] _itemBlockDropRates;

        [NotNull]
        private readonly IBlockGenerator _blockGenerator;

        public DefaultMapGenerator(
            [NotNull] IMapConfig config,
            float itemBlockDensity,
            BlockType[] itemBlockTypes,
            float[] itemBlockDropRates,
            [NotNull] IBlockGenerator blockGenerator
        ) {
            _config = config;
            _itemBlockDensity = itemBlockDensity;
            _itemBlockTypes = itemBlockTypes;
            _itemBlockDropRates = itemBlockDropRates;
            _blockGenerator = blockGenerator;
        }

        public IMapInfo Generate() {
            var tileset = _config.TilesetList[Random.Range(0, _config.TilesetList.Length)];
            var pattern = new StringMapPattern(@"
                |0_b...........b_3
                |_x.x.x.x.x.x.x.x_
                |b...............b
                |.x.x.x.x.x.x.x.x.
                |.................
                |.x.x.x.x.x.x.x.x.
                |.................
                |.x.x.x.x.x.x.x.x.
                |.................
                |.x.x.x.x.x.x.x.x.
                |b...............b
                |_x.x.x.x.x.x.x.x_
                |2_b...........b_1
            |".StripMargin());
            var positionGenerator = new DefaultPositionGenerator("0123");
            var blocks = _blockGenerator.Generate(pattern);
            var fallingBlockPattern =
                _config.FallingBlockPatternList[Random.Range(0, _config.FallingBlockPatternList.Length)];
            var fallingBlockGenerator = fallingBlockPattern.ToGenerator();
            return new MapInfo(
                playTime: _config.PlayTime,
                tileset: tileset,
                width: pattern.Width,
                height: pattern.Height,
                startingPositions: positionGenerator
                    .Generate(pattern)
                    .Select(it => new Dictionary<string, int> {
                        ["first"] = it.x, //
                        ["second"] = it.y,
                    })
                    .ToArray(),
                blocks: blocks,
                fallingBlocks: fallingBlockGenerator.Generate(pattern.Width, pattern.Height, _config.PlayTime),
                itemBlockDropRate: _itemBlockDensity,
                itemBlockRandomizer: new WeightedRandomizer<BlockType>(_itemBlockTypes, _itemBlockDropRates)
            );
        }
    }
}