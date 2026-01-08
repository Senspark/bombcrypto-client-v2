using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Engine.Manager {
    public class ObserveDataFactory : IObserveDataFactory {
        [NotNull]
        private readonly IMatchInfo _matchInfo;

        private int _id;

        public ObserveDataFactory([NotNull] IMatchInfo matchInfo) {
            _matchInfo = matchInfo;
            _id = 0;
        }

        public IMatchObserveData Generate(long timestamp, IMatchStateDelta stateDelta) {
            return new MatchObserveData(
                id: _id++,
                timestamp: timestamp,
                matchId: _matchInfo.Id,
                heroDelta: stateDelta.Hero,
                bombDelta: stateDelta.Bomb,
                blockDelta: stateDelta.Block
            );
        }
    }
}