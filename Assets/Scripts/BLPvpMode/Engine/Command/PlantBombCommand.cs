using System;
using System.Threading.Tasks;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Engine.Command {
    public class PlantBombCommand : ICommand {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IPacketManager _packetManager;

        [NotNull]
        private readonly IMatch _match;

        [NotNull]
        private readonly TaskCompletionSource<IPlantBombData> _tcs;

        private readonly int _slot;

        public int Timestamp { get; }

        public PlantBombCommand(
            [NotNull] ILogManager logger,
            [NotNull] IPacketManager packetManager,
            int timestamp,
            [NotNull] IMatch match,
            [NotNull] TaskCompletionSource<IPlantBombData> tcs,
            int slot
        ) {
            _logger = logger;
            _packetManager = packetManager;
            Timestamp = timestamp;
            _match = match;
            _tcs = tcs;
            _slot = slot;
        }

        public void Handle() {
            try {
                var hero = _match.HeroManager.GetHero(_slot);
                var bomb = hero.PlantBomb(Timestamp, true);
                var id = bomb.Id;
                var positionInt = new Vector2Int(
                    Mathf.FloorToInt(bomb.Position.x),
                    Mathf.FloorToInt(bomb.Position.y));
                var plantTimestamp = bomb.PlantTimestamp;
                _packetManager.Add(() =>
                    _tcs.SetResult(new PlantBombData(id, positionInt.x, positionInt.y, plantTimestamp)));
            } catch (Exception ex) {
                _packetManager.Add(() => _tcs.SetException(ex));
            }
        }
    }
}