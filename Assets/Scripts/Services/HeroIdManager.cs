using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Senspark;

namespace Services {
    public class HeroIdManager : IHeroIdManager {
        private IEarlyConfigManager _earlyConfigManager;
        private Dictionary<int, int> _heroIds;
        private Dictionary<int, int> _itemIds;

        public HeroIdManager(IEarlyConfigManager earlyConfigManager) {
            _earlyConfigManager = earlyConfigManager;
        }

        public void Destroy() {
        }

        public int GetHeroId(int itemId) {
            Initialize();
            return _heroIds.TryGetValue(itemId, out var value)
                ? value
                : throw new Exception($"Could not find item id: {itemId}");
        }

        public int GetItemId(int heroId) {
            Initialize();
            return _itemIds.TryGetValue(heroId, out var value)
                ? value
                : throw new Exception($"Could not find hero id: {heroId}");
        }

        private void Initialize() {
            Initialize(_earlyConfigManager.Heroes.Select(it => (it.Skin, it.ItemId)));
        }

        public void Initialize(IEnumerable<(int HeroId, int ItemId)> data) {
            _heroIds ??= data.ToDictionary(it => it.ItemId, it => it.HeroId);
            _itemIds ??= _heroIds.ToDictionary(it => it.Value, it => it.Key);
        }

        Task<bool> IService.Initialize() {
            return Task.FromResult(true);
        }
    }
}