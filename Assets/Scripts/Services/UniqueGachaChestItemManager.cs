using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Data;

using Game.Dialog.BomberLand.BLFrameShop;

namespace Services {
    public class UniqueGachaChestItemManager : IGachaChestItemManager {
        private GachaChestShopItemData[] _data;

        private readonly Dictionary<ChestShopType, int[]> _items = new();

        public void Destroy() {
        }

        public void SetItems(ChestShopType chestType, int[] items) {
            _items[chestType] = items;
        }

        public int[] GetItems(ChestShopType chestType) {
            return _items.TryGetValue(chestType, out var item) ? item : Array.Empty<int>();
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}