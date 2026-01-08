using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Data;

using Senspark;

using Newtonsoft.Json;

namespace Services {
    public class ProductItemManager : IProductItemManager {
        private static ProductItemKind GetItemKind(string str) {
            return str switch {
                "MVP" => ProductItemKind.Mvp,
                "NORMAL" => ProductItemKind.Normal,
                "PREMIUM" => ProductItemKind.Premium,
                _ => ProductItemKind.Unknown
            };
        }

        private static ProductTagShop GetTagShop(int tag) {
            return tag switch {
                1 => ProductTagShop.New,
                2 => ProductTagShop.Limited,
                _ => ProductTagShop.Unknown
            };
        }

        private Dictionary<int, ProductItemData> _data;
        private Dictionary<int, string> _descriptions;
        private readonly IEarlyConfigManager _earlyConfigManager;
        private ILogManager _logManager;

        public ProductItemManager(IEarlyConfigManager earlyConfigManager) {
            _earlyConfigManager = earlyConfigManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task<string> GetDescriptionAsync(int itemId) {
            await InitializeAsync();
            return GetDescription(itemId);
        }

        public string GetDescription(int itemId) {
            LoadData();
            return _descriptions.TryGetValue(itemId, out var description)
                ? description
                : throw new Exception($"Could not find item id: {itemId}");
        }

        public ProductItemData GetItem(int itemId) {
            LoadData();
            return _data.TryGetValue(itemId, out var data)
                ? data
                : throw new Exception($"Could not find item id: {itemId}");
        }

        public async Task<ProductItemData> GetItemAsync(int itemId) {
            await InitializeAsync();
            return GetItem(itemId);
        }

        public async Task InitializeAsync() {
            if (_data != null) {
                return;
            }
            await _earlyConfigManager.InitializeAsync();
            LoadData();
        }

        private void LoadData() {
            _data ??= _earlyConfigManager.Items.ToDictionary(
                it => it.ItemId,
                it => new ProductItemData {
                    Abilities = JsonConvert.DeserializeObject<string[]>(it.Abilities),
                    Name = it.Name,
                    Description = it.Description,
                    ItemId = it.ItemId,
                    ItemType = it.ItemType,
                    ItemKind = GetItemKind(it.Kind),
                    ItemKindStr = it.Kind,
                    TagShop = GetTagShop(it.TagShop)
                }
            );
            var isLog = _descriptions == null;
            _descriptions ??= _earlyConfigManager.Items.ToDictionary(it => it.ItemId, it => it.Description);
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
            if (isLog) {
                _logManager.Log($"descriptions: {string.Join(", ", _descriptions.Values)}");
                _logManager.Log($"type: {string.Join(", ", _data.Values.Select(it => it.ItemType))}");    
            }
        }
    }
}