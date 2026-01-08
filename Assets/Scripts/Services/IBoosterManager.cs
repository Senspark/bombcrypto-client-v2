using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(Services) + nameof(IBoosterManager))]
    public interface IBoosterManager : IService {
        Task<BoosterData[]> GetPvEBoostersAsync();
        Task<BoosterData[]> GetPvPBoostersAsync();
    }
}