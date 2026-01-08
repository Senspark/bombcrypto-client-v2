using System.Threading.Tasks;

namespace BLPvpMode.Manager {
    public interface IPlantBombManager {
        Task PlantBomb();

        void ProcessUpdate(float delta);
        void ReceivePacket(IObserverPlantBombPacket packet);
    }
}