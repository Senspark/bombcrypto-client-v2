using System.Collections.Generic;
using System.Threading.Tasks;

using Game.UI.GameData;

using Senspark;

namespace Services {
    [Service(nameof(IGameDataRemoteManager))]
    public interface IGameDataRemoteManager : IService {
        Task SyncRemoteData();
        bool GetData(string eventName);
    }
}