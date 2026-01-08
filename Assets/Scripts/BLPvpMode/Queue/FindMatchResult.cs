using BLPvpMode.Engine.Info;

using Data;

namespace BLPvpMode.Queue {
    public interface IFindMatchResult {
        IMatchInfo MatchInfo { get; }
        string Hash { get; }
    }

    public class FindMatchResult : IFindMatchResult {
        public IMatchInfo MatchInfo { get; }
        public string Hash { get; }

        public FindMatchResult(string parameters) {
            var response = PvpMatchResponse.Parse(parameters);
            MatchInfo = response.Info;
            Hash = response.Hash;
        }
    }
}