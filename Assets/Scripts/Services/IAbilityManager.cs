using Data;

using Senspark;

namespace Services {
    [Service(nameof(IAbilityManager))]
    public interface IAbilityManager : IService {
        StatData[] GetStats(string abilityHash);
    }
}