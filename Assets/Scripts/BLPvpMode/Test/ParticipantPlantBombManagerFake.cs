using System;
using System.Threading.Tasks;

using BLPvpMode.Manager;

using UnityEngine;

namespace BLPvpMode.Test {
    public class ParticipantPlantBombManagerFake : IPlantBombManager {
        private readonly Action<int, Vector2Int, int, int> _onPlanted;

        public ParticipantPlantBombManagerFake(
            Action<int, Vector2Int, int, int> onPlanted
        ) {
            _onPlanted = onPlanted;
        }

        public Task PlantBomb() {
            _onPlanted(0, Vector2Int.zero, 0, 0);
            return Task.CompletedTask;
        }

        public void ProcessUpdate(float delta) {
            //throw new System.NotImplementedException();
        }

        public void ReceivePacket(IObserverPlantBombPacket packet) {
            throw new System.NotImplementedException();
        }
    }
}