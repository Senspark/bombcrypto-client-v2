using System;
using System.Threading.Tasks;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

using Senspark;

namespace BLPvpMode.Engine.Command {
    public class UseBoosterCommand : ICommand {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IPacketManager _packetManager;

        [NotNull]
        private readonly IMatch _match;

        [NotNull]
        private readonly TaskCompletionSource<object> _tcs;

        private readonly int _slot;
        private readonly Booster _item;

        public int Timestamp { get; }

        public UseBoosterCommand(
            [NotNull] ILogManager logger,
            [NotNull] IPacketManager packetManager,
            int timestamp,
            [NotNull] IMatch match,
            [NotNull] TaskCompletionSource<object> tcs,
            int slot,
            Booster item
        ) {
            _logger = logger;
            _packetManager = packetManager;
            Timestamp = timestamp;
            _match = match;
            _tcs = tcs;
            _slot = slot;
            _item = item;
        }

        public void Handle() {
            try {
                var hero = _match.HeroManager.GetHero(_slot);
                hero.UseBooster(_item);
                _packetManager.Add(() => _tcs.SetResult(null));
            } catch (Exception ex) {
                _packetManager.Add(() => _tcs.SetException(ex));
            }
        }
    }
}