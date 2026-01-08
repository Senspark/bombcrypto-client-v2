using System.Threading.Tasks;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

namespace BLPvpMode.Manager {
    public class PredefinedMatchFinder : IMatchFinder {
        [NotNull]
        private readonly IMatchInfo[] _infoList;

        public PredefinedMatchFinder(
            IMatchInfo[] infoList
        ) {
            _infoList = infoList;
        }

        public Task<IMatchInfo[]> Find() {
            return Task.FromResult(_infoList);
        }
    }
}