using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(IAirDropManager))]
    public interface IAirDropManager : IService {
        Task SyncRemoteData();
        AirDropEventData GetData(string code);
    }
}