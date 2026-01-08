using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IBlockGenerator {
        [ItemNotNull]
        [NotNull]
        IBlockInfo[] Generate([NotNull] IMapPattern pattern);
    }
}