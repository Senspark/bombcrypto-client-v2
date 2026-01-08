using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Data;

using Senspark;

namespace Services {
    public class HeroColorManager : IHeroColorManager {
        private Dictionary<int, int> _data;
        private readonly ILogManager _logManager;

        public HeroColorManager(ILogManager logManager) {
            _logManager = logManager;
        }

        public void Destroy() {
        }

        public int GetColor(int heroId) {
            return _data.TryGetValue(heroId, out var value)
                ? value
                : throw new Exception($"Could not find hero id: {heroId}");
        }

        public void Initialize(IEnumerable<HeroColorData> data) {
            _data = data.ToDictionary(it => it.HeroId, it => it.HeroColor);
            _logManager.Log($"hero ids: {string.Join(", ", _data.Keys)}");
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}