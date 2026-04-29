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
        private readonly int _wagerMode;
        private readonly int _wagerTier;
        private readonly int _wagerToken;

        public RemoteMatchFinder(
            [NotNull] IPvpJoinManager joinManager,
            Engine.Info.PvpMode mode,
            [CanBeNull] string matchId,
            int wagerMode = 0,
            int wagerTier = 0,
            int wagerToken = 0
        ) {
            _joinManager = joinManager;
            _mode = mode;
            _matchId = matchId;
            _wagerMode = wagerMode;
            _wagerTier = wagerTier;
            _wagerToken = wagerToken;
        }

        public async Task<IMatchInfo[]> Find() {
            var results = await _joinManager.FindMatch(_mode, _matchId, _wagerMode, _wagerTier, _wagerToken);
            return results
                .Select(item => item.MatchInfo)
                .ToArray();
        }
    }
}