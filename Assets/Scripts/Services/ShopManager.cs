using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using Castle.Core.Internal;

using Constant;

using Data;

using Newtonsoft.Json;

using Utils;

namespace Services {
    public class ShopManager : IShopManager {
        private class CostumePrice {
            [JsonProperty("package")]
            public string Package;

            [JsonProperty("reward_type")]
            public string RewardType;

            [JsonProperty("price")]
            public int Price;

            [JsonProperty("duration")]
            public long Duration;
        }

        private class GetCostumeData {
            [JsonProperty("item_id")]
            public int ItemId;

            [JsonProperty("prices")]
            public CostumePrice[] Prices;
        }

        private readonly Dictionary<int, CostumeData[]> _costumes = new();
        private readonly IServerRequester _serverRequester;

        public ShopManager(IServerRequester serverRequester) {
            _serverRequester = serverRequester;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task BuyCostumeAsync(int itemId, string itemPackage, int quantity) {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(
                await _serverRequester.BuyCostumeItem(itemId, itemPackage, quantity)
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
        }

        public CostumeData[] GetCostumes(int itemType) {
            return _costumes.TryGetValue(itemType, out var value) ? value : Array.Empty<CostumeData>();
        }
        
        public void RemoveItemShop(int itemId, int itemType) {
            var allItemByTypes =_costumes.TryGetValue(itemType, out var value) ? value : Array.Empty<CostumeData>();
            _costumes[itemType] = allItemByTypes.FindAll(item => item.ItemId != itemId);
        }

        public async Task<CostumeData[]> GetCostumesAsync(int itemType) {
            if (_costumes.TryGetValue(itemType, out var value)) {
                return value;
            }
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<GetCostumeData[]>>(
                await _serverRequester.GetCostumeShop(itemType)
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }

            _costumes[itemType] = result.Data.Select(it => new CostumeData {
                ItemId = it.ItemId,
                Prices = it.Prices.Select(price => new CostumeData.PriceData {
                    Package = price.Package,
                    RewardType = price.RewardType,
                    Price = price.Price,
                    Duration = price.Duration
                }).ToArray()
            }).ToArray();
            return GetCostumes(itemType);
        }
    }
}