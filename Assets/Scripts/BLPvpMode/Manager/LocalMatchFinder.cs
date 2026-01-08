using System.Linq;
using System.Threading.Tasks;

using BLPvpMode.Engine.Info;
using BLPvpMode.Manager.Builders;

using JetBrains.Annotations;

namespace BLPvpMode.Manager {
    public class LocalMatchFinder : IMatchFinder {
        [NotNull]
        private readonly int[] _slots;

        private readonly Engine.Info.PvpMode _mode;
        private readonly int _round;

        [NotNull]
        private readonly IMatchUserInfoBuilder[] _builders;

        public LocalMatchFinder(
            [NotNull] int[] slots,
            Engine.Info.PvpMode mode,
            int round,
            [NotNull] IMatchUserInfoBuilder[] builders
        ) {
            _slots = slots;
            _mode = mode;
            _round = round;
            _builders = builders;
        }

        public Task<IMatchInfo[]> Find() {
            return Task.FromResult(_slots
                .Select(slot =>
                    new MatchInfoBuilder {
                        Mode = _mode, //
                        Round = _round,
                        Slot = slot,
                        UserBuilders = _builders,
                    }.Build())
                .ToArray());
        }
    }
}