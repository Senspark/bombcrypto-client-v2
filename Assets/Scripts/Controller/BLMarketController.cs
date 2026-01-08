using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using App;
using Data;
using Game.Dialog;
using Game.UI;
using Senspark;
using Services;
using Services.Server;
using UnityEngine;

namespace Controller {
    public class BLMarketController : MonoBehaviour {
        [SerializeField]
        private BLItemType type;

        [SerializeField]
        private BLItemContent content;

        [SerializeField]
        private BLBarSort barSort;

        private IAbilityManager _abilityManager;
        private IMarketplace _marketplace;

        private void Awake() {
            _abilityManager = ServiceLocator.Instance.Resolve<IAbilityManager>();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
        }

        public void ClearCacheData() {
            content.ClearCacheData();
        }
        
        public async Task SetMaxItems() {
            await content.ItemList.SetMaxItems();
        }
        
        public async Task LoadData() {
            var result = await _marketplace.GetProductAsync(
                (int) ItemData.ConvertToProductType(type),
                0,
                content.ItemList.MaxItems,
                (int) barSort.CurrentSort);
            var items = new List<ItemData>();
            foreach (var iter in result.Products) {
                items.Add(ConvertFrom(iter));
            }
            content.SetPageData(result.Quantity);
            content.SetData(items, true);
        }
        
        public async Task UpdatePageData() {
            var result = await  _marketplace.GetProductAsync(
                (int) ItemData.ConvertToProductType(type),
                content.ItemList.CurPage * content.ItemList.MaxItems,
                content.ItemList.MaxItems,
                (int) barSort.CurrentSort);
            var items = new List<ItemData>();
            foreach (var iter in result.Products) {
                items.Add(ConvertFrom(iter));
            }
            content.ItemList.UpdatePage();
            content.UpdateData(items);
        }
        
        public void RefreshMinPrice() {
            content.RefreshMinPrice();
        }

        public int GetInputAmount() {
            return content.GetInputAmount();
        }

        private ItemData ConvertFrom([NotNull] ProductData data) {
            var resultStats = new Dictionary<int, StatData>();
            foreach (var ability in data.Abilities) {
                var stats = _abilityManager.GetStats(ability);
                foreach (var statData in stats) {
                    var statId = statData.StatId;
                    if (resultStats.ContainsKey(statId)) {
                        resultStats[statId].PlusAssign(statData);
                    } else {
                        resultStats[statId] = new StatData(statId, 0, 0, statData.Value);
                    }
                }
            }

            return new ItemData(
                data,
                data.SellTime,
                type,
                data.ItemId,
                data.ProductName,
                data.Description,
                data.Quantity,
                data.Price.Value,
                (BlockRewardType) data.Price.Type,
                data.ProductId,
                data.ProductType,
                resultStats.Values.ToArray(),
                data.ExpirationAfter
            );
        }
    }
}