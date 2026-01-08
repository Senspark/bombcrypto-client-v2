using Data;

using Senspark;

namespace Services {
    [Service(nameof(IHeroAbilityManager))]
    public interface IHeroAbilityManager : IService {
        AbilityData[] GetAbilities(int heroId);
    }
}