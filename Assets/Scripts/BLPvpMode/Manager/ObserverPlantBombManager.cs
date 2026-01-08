using System;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Manager {
    public class ObserverPlantBombManager : IPlantBombManager {
        private readonly ITimeManager _timeManager;
        private readonly int _slot;
        private readonly Action<int, Vector2Int> _onPlanted;

        public ObserverPlantBombManager(
            ITimeManager timeManager,
            int slot,
            Action<int, Vector2Int> onPlanted
        ) {
            _timeManager = timeManager;
            _slot = slot;
            _onPlanted = onPlanted;
        }

        public Task PlantBomb() {
            // No op.
            return Task.CompletedTask;
        }

        public void ProcessUpdate(float delta) {
            // No-op.
        }

        public void ReceivePacket(IObserverPlantBombPacket packet) {
            Assert.IsTrue(_slot == packet.Slot);
            _onPlanted(packet.BombId, packet.Position);
        }
    }
}