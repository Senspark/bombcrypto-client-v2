using System.Linq;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Strategy.FallingBlock {
    public class MultiFallingBlockGenerator : IFallingBlockGenerator {
        [NotNull]
        private readonly IFallingBlockGenerator[] _generators;

        public MultiFallingBlockGenerator(
            [NotNull] IFallingBlockGenerator[] generators
        ) {
            _generators = generators;
        }

        public IFallingBlockInfo[] Generate(int width, int height, int playTime) {
            return _generators
                .SelectMany(it => it.Generate(width, height, playTime))
                .OrderBy(it => it.Timestamp)
                .ToArray();
        }
    }
}