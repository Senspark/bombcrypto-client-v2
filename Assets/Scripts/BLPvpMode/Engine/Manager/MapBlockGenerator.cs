using System.Linq;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public class MapBlockGenerator : IBlockGenerator {
        [NotNull]
        private readonly IBlockHealthManager _blockHealthManager;

        private readonly float _softBlockDensity;

        public MapBlockGenerator(
            [NotNull] IBlockHealthManager blockHealthManager,
            float softBlockDensity
        ) {
            _blockHealthManager = blockHealthManager;
            _softBlockDensity = softBlockDensity;
        }

        public IBlockInfo[] Generate(IMapPattern pattern) {
            var softBlockGenerators = new IBlockGenerator[] {
                new SoftBlockGenerator('b', 1f, _blockHealthManager),
                new SoftBlockGenerator('.', _softBlockDensity, _blockHealthManager),
            };
            var softBlocks = softBlockGenerators
                .SelectMany(it => it.Generate(pattern))
                .ToArray();
            return softBlocks;
        }
    }
}