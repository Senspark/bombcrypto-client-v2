using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IFreeTRHeroManager))]
    public interface IFreeTRHeroManager : IService {
        Task<int> ChooseAsync(FreeTRHeroData hero);
        FreeTRHeroData[] GetHeroes();
        // void Initialize(FreeTRHeroData[] heroes, bool isChose);
        bool IsChose();
    }
}