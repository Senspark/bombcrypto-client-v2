using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using Senspark;

namespace BLPvpMode.Engine.Manager {
    public class DefaultBlockDropper : IBlockDropper {
        public static IBlockDropper Create(
            [NotNull] IMapInfo info,
            [NotNull] ILogManager logger,
            [NotNull] IRandom random
        ) {
            return new DefaultBlockDropper(new IBlockDropper[] {
                new ItemBlockDropper(
                    blockDropRate: info.ItemBlockDropRate,
                    blockRandomizer: info.ItemBlockRandomizer,
                    logger: logger,
                    random: random
                ),
            });
        }

        [NotNull]
        private readonly IBlockDropper[] _droppers;

        private DefaultBlockDropper(
            [NotNull] IBlockDropper[] droppers
        ) {
            _droppers = droppers;
        }

        public IBlock Drop(IMapManager mapManager, IBlock block) {
            return _droppers.FirstNotNullOfOrNull(it =>
                it.Drop(mapManager, block)
            );
        }
    }
}