using System;
using System.Threading.Tasks;

using Data;

namespace Services {
    public class EmptyHeroAbilityManager : IHeroAbilityManager {
        public void Destroy() {
        }

        public AbilityData[] GetAbilities(int heroId) {
            return Array.Empty<AbilityData>();
        }
        
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}