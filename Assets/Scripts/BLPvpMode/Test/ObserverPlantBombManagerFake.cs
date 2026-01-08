using System.Threading.Tasks;

using BLPvpMode.Manager;

namespace BLPvpMode.Test {
    public class ObserverPlantBombManagerFake : IPlantBombManager {

        public ObserverPlantBombManagerFake() {
            //TODO: 
        }
        
        public Task PlantBomb() {
            throw new System.NotImplementedException();
        }

        public void ProcessUpdate(float delta) {
            // TODO: 
        }

        public void ReceivePacket(IObserverPlantBombPacket packet) {
            throw new System.NotImplementedException();
        }
    }
}