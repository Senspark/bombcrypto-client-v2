using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IMatchStartData {
        [NotNull]
        IMatchData Match { get; }

        [NotNull]
        IMapInfo Map { get; }
    }
}