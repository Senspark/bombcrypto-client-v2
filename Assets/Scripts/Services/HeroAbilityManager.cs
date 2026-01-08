using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    public class HeroAbilityManager : IHeroAbilityManager {
        private readonly Dictionary<int, HeroAbilityData> _data;

        public HeroAbilityManager(IEnumerable<HeroAbilityData> data, ILogManager logManager) {
            _data = data.ToDictionary(it => it.HeroId);
            logManager.Log(string.Join(" | ",
                _data.Values.Select(it => $"hero id: {it.HeroId}, abilities: {it.Abilities}")));
        }

        public void Destroy() {
        }

        public AbilityData[] GetAbilities(int heroId) {
            var data = _data.TryGetValue(heroId, out var value)
                ? value
                : throw new Exception($"[{nameof(HeroAbilityManager)}] Could not find hero id: {heroId}");
            return data.Abilities;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}