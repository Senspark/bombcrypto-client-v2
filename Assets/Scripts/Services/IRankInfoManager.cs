using System.Threading.Tasks;
using App;
using Senspark;

namespace Services {
    [Service(nameof(IRankInfoManager))]
    public interface IRankInfoManager : IService {
        int BombRank { get; }
        int CurrentPoint { get; }
        int DecayPointConfig { get; }
        int MinMatchesConfig { get; }
        int AmountMatches { get; }
        int DecayPointUser { get; }
        GameModeType LastPlayMode { get; }
        Task InitializeAsync();
        Task ReloadData();
        void UpdateLastPlayMode(GameModeType mode);
    }
}