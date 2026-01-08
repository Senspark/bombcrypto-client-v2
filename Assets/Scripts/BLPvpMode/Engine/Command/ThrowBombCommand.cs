using System;
using System.Threading.Tasks;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Command {
    public class ThrowBombCommand : ICommand {
        [NotNull]
        private readonly IPacketManager _packetManager;

        [NotNull]
        private readonly IMatch _match;

        [NotNull]
        private readonly TaskCompletionSource<object> _tcs;

        private readonly int _slot;

        public int Timestamp { get; }

        public ThrowBombCommand(
            [NotNull] IPacketManager packetManager,
            int timestamp,
            [NotNull] IMatch match,
            [NotNull] TaskCompletionSource<object> tcs,
            int slot
        ) {
            _packetManager = packetManager;
            Timestamp = timestamp;
            _match = match;
            _tcs = tcs;
            _slot = slot;
        }

        public void Handle() {
            try {
                var hero = _match.HeroManager.GetHero(_slot);
                var positionInt = new Vector2Int(
                    Mathf.FloorToInt(hero.Position.x),
                    Mathf.FloorToInt(hero.Position.y));
                var bomb = _match.BombManager.GetBomb(positionInt);
                if (bomb == null) {
                    throw new Exception($"No bomb to throw at [{positionInt.x}, {positionInt.y}]");
                }
                _match.BombManager.ThrowBomb(bomb, hero.Direction, 3, 500);
                _packetManager.Add(() => _tcs.SetResult(null));
            } catch (Exception ex) {
                _packetManager.Add(() => _tcs.SetException(ex));
            }
        }
    }
}