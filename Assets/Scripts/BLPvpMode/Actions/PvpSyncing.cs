using System.Threading.Tasks;

using BLPvpMode.Manager;

using Engine.Components;

using PvpMode.Entities;
using PvpMode.Services;

using UnityEngine;

namespace Actions {
    public enum FactionType {
        Ally,
        Enemy,
    }

    public class PvpSyncing {
        public ICommandManager CommandManager { get; }

        public int Slot { get; }
        public bool IsBot { get; }

        private PlayerPvp Player { get; }

        public bool IsAlive => Player.IsAlive;

        public PvpSyncing(
            PlayerPvp player,
            ICommandManager commandManager,
            int slot,
            bool isBot
        ) {
            Player = player;
            CommandManager = commandManager;
            Slot = slot;
            IsBot = isBot;
        }

        public void UpdateLocalPosition(long timestamp, Vector2 position, Direction direction) {
            CommandManager.ReceivePacket(new MovePacket {
                Timestamp = timestamp, //
                Position = position, //
                Direction = direction,
            });
        }

        public async Task PlantBomb() {
            await CommandManager.PlantBomb();
        }

        public void UpdateMovementLatency(int latency) {
            // Max acceptable ping difference.
            const int maxAcceptablePingDelta = 1000;
            var offset = Mathf.Min(latency / 2 + CommandManager.TimeToSendBundleMove + 10, maxAcceptablePingDelta);
            CommandManager.PositionInterpolationOffset = offset;
        }

        public void SyncMove(float delta) {
            // Process position.
            if (Player.IsInJail) {
                return;
            }
            var movable = Player.Movable;
            CommandManager.Position = movable.Position;
            CommandManager.PositionPredict = movable.PositionPredict ?? movable.Position;
        }

        public void SyncPos() {
            var movable = Player.Movable;
            ((PvpMovable) movable).SyncPos(CommandManager.Position, CommandManager.Direction);
        }
    }
}