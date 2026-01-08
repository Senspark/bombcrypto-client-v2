using System;
using System.Collections.Generic;
using System.Linq;
using Constant;
using Game.Dialog;
using Senspark;
using Services;
using UnityEngine;

namespace Game.UI {
    public class BLAvatarContent : BaseBLContent {
        [SerializeField]
        private BaseBLItemInformation item;
        public BaseBLItemInformation ItemInformation => item;

        private List<ItemData> _items;
        private Action<int, (int, long)[]> _sendEquipCallback;
        private ItemData _curItem;

        private void Awake() {
            itemList.SetOnSelectItem(ChooseItem);
            item.OnShowDialogCallback = EquipAvatar;
        }

        public override void SetData<T>(List<T> list, bool isUpdate) {
            _items = list as List<ItemData>;
            if(_items == null) {
                return;
            }
            _items = _items.OrderBy(e => e.CreateDate).ToList();
            // Hide ItemInformation
            item.gameObject.SetActive(false);
            itemList.LoadData(_items, isUpdate);
        }

        private void ChooseItem(int index) {
            item.gameObject.SetActive(true);
            item.UpdateItem(_items[index]);
        }

        private void EquipAvatar(ItemData itemData) {
            UpdateLocalData(itemData);
            SendEquip(itemData);
        }
        
        public void SetSendEquipCallback(Action<int, (int, long)[]> callback) {
            _sendEquipCallback = callback;
        }
        
        private void SendEquip(ItemData itemData) {
            _sendEquipCallback?.Invoke(
                (int) InventoryItemType.AvatarTR,
                new[] { (itemData.ItemId, (long)itemData.ExpirationAfter) }
            );
        }

        private void UpdateLocalData(ItemData itemData) {
            foreach (var iter in _items) {
                if (iter.Equipped) iter.ToggleEquipped();
            }
            itemData.DisableIsNew();
            itemData.ToggleEquipped();
            itemList.UpdateLocalData(_items);
            
            var avatarTRManager = ServiceLocator.Instance.Resolve<IAvatarTRManager>();
            avatarTRManager.SetCurrentAvatarId(itemData.ItemId);
        }
    }
}