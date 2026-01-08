using System.Collections.Generic;
using System.Threading.Tasks;

using Services;

namespace Scenes.TreasureModeScene.Scripts.Mocks
{
    public class NullHeroIdManager : IHeroIdManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public int GetHeroId(int itemId) {
            return 0;
        }

        public int GetItemId(int heroId) {
            return 0;
        }

        public void Initialize(IEnumerable<(int HeroId, int ItemId)> data) {
        }
    }
}