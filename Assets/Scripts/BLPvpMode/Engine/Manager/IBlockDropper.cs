using BLPvpMode.Engine.Entity;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IBlockDropper {
        [CanBeNull]
        IBlock Drop([NotNull] IMapManager mapManager, [NotNull] IBlock block);
    }
}