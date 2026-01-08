using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Data;

namespace Services {
    public class MockInventoryManager : IInventoryManager {
        private IDictionary<int, Dictionary<int, InventoryItemData>> _data;

        public MockInventoryManager(params InventoryItemData[] data) {
            _data = data.GroupBy(it => it.ItemType)
                .ToDictionary(it => it.Key, items => items.ToDictionary(item => item.ItemId));
        }

        public void Destroy() {
        }

        public Task UnlockChestSlotAsync(int slotId) {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryHeroData>> GetHeroesAsync() {
            throw new NotImplementedException();
        }

        public void Clear() {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryChestData>> GetChestAsync() {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<InventoryItemData>> GetItemsAsync() {
            return Task.FromResult(_data.SelectMany(it => it.Value.Values));
        }

        public Task<IEnumerable<InventoryItemData>> GetItemsAsync(int itemType) {
            IEnumerable<InventoryItemData> items = _data.TryGetValue(itemType, out var value) ? value.Values : Array.Empty<InventoryItemData>();
            return Task.FromResult(items);
        }

        public Task<IEnumerable<InventorySellingItemData>> GetSellingItemsAsync() {
            throw new NotImplementedException();
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
        
        public async Task<int> GetCurrentAvatarTR() {
            return 0;
        }
    }
}