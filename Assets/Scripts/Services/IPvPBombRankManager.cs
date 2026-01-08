using System.Threading.Tasks;
using Senspark;

namespace Services {
    [Service(nameof(IPvPBombRankManager))]
    public interface IPvPBombRankManager : IService {
        PvpRankType GetBombRank();
        PvpRankType GetBombRank(int bombRank);
        int GetCurrentPoint();
        int GetDecayPointConfig();
        int GetMinMatchesConfig();
        int GetAmountMatches();
        int GetDecayPointUser();
        Task InitializeAsync();
    }
}