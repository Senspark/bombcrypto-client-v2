using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Constant;

using Data;

using Senspark;

namespace Services {
    public class HeroStatsManager : IHeroStatsManager {
        private Dictionary<int, HeroStatData> _data;
        private readonly IEarlyConfigManager _earlyConfigManager;
        private bool _initialized;
        private readonly ILogManager _logManager;

        public HeroStatsManager(ILogManager logManager, IEarlyConfigManager earlyConfigManager) {
            _earlyConfigManager = earlyConfigManager;
            _logManager = logManager;
        }

        public void Destroy() {
        }

        public StatData[] GetStats(int heroId) {
            Initialize();
            var data = _data.TryGetValue(heroId, out var value)
                ? value
                : throw new Exception($"[{nameof(HeroStatsManager)}] Could not find hero id: {heroId}");
            return data.Stats;
        }

        private void Initialize() {
            if (_initialized) {
                return;
            }
            _initialized = true;
            _data = _earlyConfigManager.Heroes.Select(it => new HeroStatData(
                it.ItemId,
                new[] {
                    new StatData(StatId.Count, 0, it.MaxBomb, it.Bomb, it.MaxUpgradeBomb),
                    new StatData(StatId.Range, 0, it.MaxRange, it.BombRange, it.MaxUpgradeRange),
                    new StatData(StatId.Speed, 0, it.MaxSpeed, it.Speed, it.MaxUpgradeSpeed),
                    new StatData(StatId.Health, 0, it.MaxHealth, it.Health, it.MaxUpgradeHealth),
                    new StatData(StatId.Damage, 0, it.MaxDamage, it.Damage, it.MaxUpgradeDamage),
                }
            )).ToDictionary(it => it.HeroId);
        }

        Task<bool> IService.Initialize() {
            return Task.FromResult(true);
        }

        public async Task InitializeAsync() {
            await _earlyConfigManager.InitializeAsync();
            Initialize();
        }
    }
}