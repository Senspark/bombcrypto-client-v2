using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Utility;

using JetBrains.Annotations;

using Senspark;

namespace BLPvpMode.Engine.Manager {
    public class ItemBlockDropper : IBlockDropper {
        private readonly float _blockDropRate;

        [NotNull]
        private readonly IRandomizer<BlockType> _blockRandomizer;

        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IRandom _random;

        public ItemBlockDropper(
            float blockDropRate,
            [NotNull] IRandomizer<BlockType> blockRandomizer,
            [NotNull] ILogManager logger,
            [NotNull] IRandom random
        ) {
            _blockDropRate = blockDropRate;
            _blockRandomizer = blockRandomizer;
            _logger = logger;
            _random = random;
        }

        public IBlock Drop(IMapManager mapManager, IBlock block) {
            if (_random.RandomFloat(0, 1) >= _blockDropRate) {
                return null;
            }
            var type = _blockRandomizer.Random(_random);
            return new Block(
                position: block.Position,
                initialState: new BlockState(
                    isAlive: true,
                    reason: BlockReason.Dropped,
                    type: type,
                    health: 1,
                    maxHealth: 1
                ),
                logger: _logger,
                mapManager: mapManager
            );
        }
    }
}