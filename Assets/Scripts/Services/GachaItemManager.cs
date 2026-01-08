using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Constant;

using Data;

namespace Services {
    public class GachaItemManager : IGachaItemManager {
        private readonly Dictionary<GachaChestProductId, GachaItemData> _data;

        public GachaItemManager(GachaItemData[] data) {
            _data = data.ToDictionary(it => it.ProductId);
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public GachaItemData GetItem(GachaChestProductId productId) {
            return _data.TryGetValue(productId, out var data)
                ? data
                : throw new Exception($"Could not find product id: {productId}");
        }
    }
}