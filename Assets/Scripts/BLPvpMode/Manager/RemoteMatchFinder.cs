using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Manager {
    public class RemoteMatchFinder : IMatchFinder {
        [NotNull]
        private readonly IPvpJoinManager _joinManager;

        private readonly Engine.Info.PvpMode _mode;

        [CanBeNull]
        private readonly string _matchId;

        public RemoteMatchFinder(
            [NotNull] IPvpJoinManager joinManager,
            Engine.Info.PvpMode mode,
            [CanBeNull] string matchId
        ) {
            _joinManager = joinManager;
            _mode = mode;
            _matchId = matchId;
        }

        public async Task<IMatchInfo[]> Find() {
            var results = await _joinManager.FindMatch(_mode, _matchId);
            return results
                .Select(item => item.MatchInfo)
                .ToArray();
        }
    }
}