using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Constant;
using Data;
using Game.Dialog;
using Game.UI;
using Senspark;
using Services;
using UnityEngine;

namespace Controller {
    public class BLInventoryEmojiController : MonoBehaviour {
        [SerializeField]
        private BLEmojiContent content;

        private IInventoryManager _inventoryManager;
        public List<InventoryItemData> Items { set; get; }
        private bool _isUpdate;

        private void Awake() {
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
        }

        public void ClearCacheData() {
            Items = null;
            _isUpdate = true;
        }
        
        public async Task SetMaxItems() {
            await content.ItemList.SetMaxItems();
        }
        
        public async Task LoadData() {
            if (Items == null) {
                var result = await _inventoryManager.GetItemsAsync((int) InventoryItemType.Emoji);
                Items = result.ToList();
            }
            var items = new List<ItemData>();
            foreach (var data in Items) {
                items.Add(new ItemData( 
                    (BLItemType) data.ItemType,
                    data.ItemId,
                    data.ItemName,
                    data.Quantity,
                    data.Sellable,
                    Array.Empty<StatData>(),
                    data.ExpirationAfter,
                    data.Equipped,
                    data.IsNew
                ));
            }
            content.SetPageData(items.Count);
            content.SetData(items, _isUpdate);
            _isUpdate = false;
        }
        
        public void SetEquipCallback(Action<int, (int, long)[]> callback) {
            content.SetSendEquipCallback(callback);
        }

    }
}