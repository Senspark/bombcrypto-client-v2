using JetBrains.Annotations;

namespace BLPvpMode.Engine.Data {
    public interface IMatchFinishData {
        [NotNull]
        IMatchData MatchData { get; }
    }
}