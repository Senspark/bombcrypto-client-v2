using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IMatchReadyData {
        [NotNull]
        string MatchId { get; }

        int Slot { get; }
    }
}