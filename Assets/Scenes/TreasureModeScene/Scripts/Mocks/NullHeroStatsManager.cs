using System.Threading.Tasks;

using Constant;

using Data;

using Services;

namespace Scenes.TreasureModeScene.Scripts.Mocks {
    public class NullHeroStatsManager : IHeroStatsManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public StatData[] GetStats(int heroId) {
            var s = new StatData(StatId.Damage, 1, 1, 1);
            return new[] { s };
        }

        public Task InitializeAsync() {
            return Task.CompletedTask;
        }
    }
}