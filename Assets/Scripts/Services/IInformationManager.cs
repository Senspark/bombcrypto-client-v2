using System.Threading.Tasks;

using Game.UI.Information;

using Senspark;

namespace App {
    [Service(nameof(IInformationManager))]
    public interface IInformationManager : IService {
        Task SyncRemoteData();
        InformationData[] GetTokenData();
        InformationData GetTokenData(ITokenReward reward);
    }
}