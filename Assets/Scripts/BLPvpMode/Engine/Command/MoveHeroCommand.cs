using System;
using System.Threading.Tasks;

using BLPvpMode.Engine.Data;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Engine.Command {
    public class MoveHeroCommand : ICommand {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IPacketManager _packetManager;

        [NotNull]
        private readonly IMatch _match;

        [NotNull]
        private readonly TaskCompletionSource<IMoveHeroData> _tcs;

        private readonly int _slot;
        private readonly Vector2 _position;

        public int Timestamp { get; }

        public MoveHeroCommand(
            [NotNull] ILogManager logger,
            [NotNull] IPacketManager packetManager,
            int timestamp,
            [NotNull] IMatch match,
            [NotNull] TaskCompletionSource<IMoveHeroData> tcs,
            int slot,
            Vector2 position
        ) {
            _logger = logger;
            _packetManager = packetManager;
            Timestamp = timestamp;
            _match = match;
            _tcs = tcs;
            _slot = slot;
            _position = position;
        }

        public void Handle() {
            try {
                var hero = _match.HeroManager.GetHero(_slot);
                hero.Move(Timestamp, _position);
                var positionInt = new Vector2Int(
                    Mathf.FloorToInt(hero.Position.x),
                    Mathf.FloorToInt(hero.Position.y));
                var block = _match.MapManager.GetBlock(positionInt);
                if (block != null && block.IsItem()) {
                    block.Kill(BlockReason.Consumed);
                    hero.TakeItem(block.Type);
                }
                var position = hero.Position;
                _packetManager.Add(() => _tcs.SetResult(new MoveHeroData(position.x, position.y)));
            } catch (Exception ex) {
                _packetManager.Add(() => _tcs.SetException(ex));
            }
        }
    }
}