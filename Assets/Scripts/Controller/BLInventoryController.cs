using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics;
using App;
using Constant;

using Cysharp.Threading.Tasks;

using Data;
using Game.Dialog;
using Game.Manager;
using Game.UI;

using Scenes.InventoryScene.Scripts;

using Senspark;
using Services;
using Services.Server;
using Services.Server.Exceptions;

using Share.Scripts.Dialog;

using UnityEngine;

namespace Controller {
    public class BLInventoryController : MonoBehaviour {
        [SerializeField]
        private BLItemType type;

        [SerializeField]
        private BLItemContent content;

        public BLItemType ItemType => type;

        private IAbilityManager _abilityManager;
        private IInventoryManager _inventoryManager;
        private IMarketplace _marketplace;
        private IAnalytics _analytics;
        private IProductItemManager _productItemManager;
        private Action<BLItemType> _onItemSellChanged;

        public List<InventoryItemData> Items { get; set; }
        public List<InventorySellingItemData> SellingItems { get; set;}

        private bool _isUpdate;

        private void Awake() {
            _abilityManager = ServiceLocator.Instance.Resolve<IAbilityManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
        }

        public void ClearCacheData() {
            Items = null;
            SellingItems = null;
            _isUpdate = true;
            content.ClearCacheData();
        }
        
        public async Task SetMaxItems() {
            await content.ItemList.SetMaxItems();
        }
        
        public async Task LoadData(Canvas canvasDialog) {
            if (Items == null) {
                var result = await _inventoryManager.GetItemsAsync(
                    (int) ItemData.ConvertToProductType(type));
                Items = result.ToList();
            }
            var items = new List<ItemData>();
            foreach (var iter in Items) {
                items.Add(ConvertFrom(iter));
            }
            content.SetPageData(items.Count);
            content.SetData(items, _isUpdate);
            _isUpdate = false;
        }

        public void LoadDataByItemList(Canvas canvasDialog, IEnumerable<InventoryItemData> result) {
            var items = result.Select(ConvertFrom).ToList();
            content.SetPageData(items.Count);
            content.SetData(items, _isUpdate);
            _isUpdate = false;
            Items = (List<InventoryItemData>) result;
        }

        public async Task LoadDataOnSell(Canvas canvasDialog, Action<BLItemType> onItemSellChanged) {
            _onItemSellChanged = onItemSellChanged;
            if (SellingItems == null) {
                var result = await _inventoryManager.GetSellingItemsAsync();
                SellingItems = result.ToList();
            }
            var items = new List<ItemData>();
            foreach (var iter in SellingItems) {
                items.Add(ConvertFrom(iter));
            }
            content.SetPageData(items.Count);
            content.SetData(items, _isUpdate);
            _isUpdate = false;
            var informationOnSell = content.ItemInformation.GetComponent<BLInventoryOnSellInformation>();
            if (!informationOnSell) {
                return;
            }
            informationOnSell.SetOnCancel(
                async (ItemData itemData) => {
                    _onItemSellChanged?.Invoke(itemData.ItemType);
                    CancelSellingProduct(canvasDialog, itemData);
                });
            informationOnSell.SetOnEdit((ItemData itemData) => {
                var dialog = DialogItemEdit.Create().ContinueWith(dialog => {
                    dialog.SetInfo(itemData);
                    dialog.Show(canvasDialog);
                    dialog.SetOnEditPrice((oldUnitPrice, newUnitPrice, oldQuantity, newQuantity) => {
                        dialog.Hide();
                        _onItemSellChanged?.Invoke(itemData.ItemType);
                        EditSellingProduct(canvasDialog, itemData.ItemId, (int)itemData.ItemType,  oldUnitPrice, newUnitPrice, oldQuantity,
                            newQuantity, (int)(itemData.ExpirationAfter));
                    });
                });
            });
        }
        
        private async void EditSellingProduct(Canvas canvasDialog, int itemId, int itemType, float oldUnitPrice, float newUnitPrice,
            int oldQuantity, int newQuantity, int expirationAfter
        ) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            try {
                await _marketplace.EditItemMarket(itemId, itemType, oldUnitPrice, newUnitPrice, newQuantity, oldQuantity, expirationAfter);
                // After EditPrice -> Refresh Data
                ClearCacheData();
                await LoadDataOnSell(canvasDialog, _onItemSellChanged);
            } catch (Exception e) {
                if (e is ErrorCodeException) {
                    DialogError.ShowError(canvasDialog, e.Message);
                } else {
                    DialogOK.ShowError(canvasDialog, e.Message);
                }
            } finally {
                waiting.End();
            }
        }

        private async void CancelSellingProduct(Canvas canvasDialog, ItemData itemData) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            try {
                await _marketplace.CancelItemMarket(itemData.ItemId, itemData.Price, (int)itemData.ItemType, (int)itemData.ExpirationAfter);
                _analytics.MarketPlace_TrackProduct(itemData.ItemName, itemData.UnitPrice, itemData.Quantity,
                    MarketPlaceResult.CancelSell);
                // After CancelSelling -> Refresh Data
                ClearCacheData();
                await LoadDataOnSell(canvasDialog, _onItemSellChanged);
            } catch (Exception e) {
                if (e is ErrorCodeException) {
                    DialogError.ShowError(canvasDialog, e.Message);
                } else {
                    DialogOK.ShowError(canvasDialog, e.Message);
                }
            } finally {
                waiting.End();
            }
            
        }

        private ItemData ConvertFrom(InventoryItemData data) {
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
                type,
                data.ItemId,
                data.ItemName,
                data.Quantity,
                data.Sellable,
                resultStats.Values.ToArray(),
                data.ExpirationAfter,
                data.Equipped,
                data.IsNew,
                data.Used,
                data.Expire,
                data.CreateDate,
                data.InventoryItemType
            );
        }

        private ItemData ConvertFrom(InventorySellingItemData data) {
            var itemData = _productItemManager.GetItem(data.ItemId);
            return new ItemData(
                ItemData.ConvertToBLItemType((InventoryItemType) itemData.ItemType),
                data.ItemId,
                data.Price.Value,
                data.Id,
                itemData.Description,
                data.Quantity,
                false,
                data.UnitPrice,
                data.ExpirationAfter
            );
        }
    }
}