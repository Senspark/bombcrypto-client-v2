using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data;
using Game.Dialog;
using Game.UI;
using Senspark;
using Services;
using UnityEngine;

namespace Controller {
    public class BLInventoryAvatarController : MonoBehaviour {
        [SerializeField]
        private BLItemType type;

        [SerializeField]
        private BLAvatarContent content;

        public BLItemType ItemType => type;

        private IAbilityManager _abilityManager;
        private IInventoryManager _inventoryManager;
        public List<InventoryItemData> Items { get; set; }

        private bool _isUpdate;
        
        private void Awake() {
            _abilityManager = ServiceLocator.Instance.Resolve<IAbilityManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
        }

        public void ClearCacheData() {
            Items = null;
            _isUpdate = true;
        }
        
        public async Task SetMaxItems() {
            await content.ItemList.SetMaxItems();
        }
        
        public void LoadDataByItemList(Canvas canvasDialog, IEnumerable<InventoryItemData> result) {
            var items = result.Select(ConvertFrom).ToList();
            content.SetPageData(items.Count);
            content.SetData(items, _isUpdate);
            _isUpdate = false;
            Items = (List<InventoryItemData>) result;
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
        
        public void SetEquipCallback(Action<int, (int, long)[]> callback) {
            content.SetSendEquipCallback(callback);
        }
    }
}