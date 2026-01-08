using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public interface IMapGenerator {
        [NotNull]
        IMapInfo Generate();
    }
}