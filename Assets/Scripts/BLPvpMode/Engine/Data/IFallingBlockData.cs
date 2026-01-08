using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IFallingBlockData {
        [NotNull]
        string MatchId { get; }

        [NotNull]
        IFallingBlockInfo[] Blocks { get; }
    }
}