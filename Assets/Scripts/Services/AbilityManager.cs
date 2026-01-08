using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Constant;

using Data;

namespace Services {
    public class AbilityManager : IAbilityManager {
        private readonly Dictionary<string, StatData[]> _data;

        public AbilityManager() : this(new[] {
            ("DMG_PLUS_1", new StatData[] {
                new(StatId.Damage, 0, 0, 1)
            }),
            ("BOMB_PLUS_1", new StatData[] {
                new(StatId.Count, 0, 0, 1)
            }),
            ("SPEED_PLUS_1", new StatData[] {
                new(StatId.Speed, 0, 0, 1)
            }),
            ("RANGE_PLUS_1", new StatData[] {
                new(StatId.Range, 0, 0, 1)
            }),
            ("HP_PLUS_1", new StatData[] {
                new(StatId.Health, 0, 0, 1)
            })
        }) {
        }

        private AbilityManager(IEnumerable<(string Id, StatData[] Stats)> data) {
            _data = data.ToDictionary(it => it.Id, it => it.Stats);
        }

        public StatData[] GetStats(string abilityHash) {
            return _data.TryGetValue(abilityHash, out var value)
                ? value
                : throw new Exception($"Could not find ability hash: {abilityHash}");
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
    }
}