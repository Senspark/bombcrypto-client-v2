using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IHeroStatsManager))]
    public interface IHeroStatsManager : IService {
        StatData[] GetStats(int heroId);
        Task InitializeAsync();
    }
}