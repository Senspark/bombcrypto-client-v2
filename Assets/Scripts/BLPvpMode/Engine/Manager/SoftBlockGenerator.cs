using System.Linq;

using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

using Random = UnityEngine.Random;

namespace BLPvpMode.Engine.Manager {
    public class SoftBlockGenerator : IBlockGenerator {
        private readonly char _char;
        private readonly float _density;

        [NotNull]
        private readonly IBlockHealthManager _blockHealthManager;

        public SoftBlockGenerator(
            char ch,
            float density,
            [NotNull] IBlockHealthManager blockHealthManager
        ) {
            _char = ch;
            _density = density;
            _blockHealthManager = blockHealthManager;
        }

        public IBlockInfo[] Generate(IMapPattern pattern) {
            var positions = pattern.Find((_, c) => c == _char);
            var chosenPositions = positions
                .OrderBy(it => Random.Range(0f, 1f))
                .Take((int) (positions.Count * _density));
            return chosenPositions.Select(position => {
                const BlockType type = BlockType.Soft;
                return (IBlockInfo) new BlockInfo(
                    blockType: (int) type,
                    x: position.x,
                    y: position.y,
                    health: _blockHealthManager.GetHealth(type)
                );
            }).ToArray();
        }
    }
}