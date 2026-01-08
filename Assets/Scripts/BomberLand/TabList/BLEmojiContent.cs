using System;
using System.Collections.Generic;
using System.Linq;
using Constant;
using Game.Dialog;
using UnityEngine;

namespace Game.UI {
    public class BLEmojiContent : BaseBLContent {
        [SerializeField]
        private BLInventoryEmojiBar emojiBar;

        private List<ItemData> _items;
        private Action<int, (int, long)[]> _sendEquipCallback;

        private bool _modified;
        
        public override void SetData<T>(List<T> list, bool isUpdate) {
            _items = list as List<ItemData>;
            if (_items == null) {
                return;
            }
            itemList.SetOnSelectItem(null);
            itemList.LoadData(_items, isUpdate);
            gameObject.SetActive(true);
            UpdateUI();
            itemList.SetOnSelectItem(ChooseItem);
            emojiBar.SetClickedCallback(UnchooseItem);
            _modified = false;
        }

        public void SetSendEquipCallback(Action<int, (int, long)[]> callback) {
            _sendEquipCallback = callback;
        }

        private void ChooseItem(int index) {
            if (!_items[index].Equipped && !emojiBar.HadEmptySlot()) {
                ReplaceToTheFirstSlot(index);
                return;
            }

            _items[index].ToggleEquipped();
            UpdateUI();
            _modified = true;
        }

        private void UnchooseItem(int itemId) {
            foreach (var item in _items) {
                if (item.ItemId != itemId) {
                    continue;
                }
                item.ToggleEquipped();
                break;
            }
            UpdateUI();
            _modified = true;
        }

        private void ReplaceToTheFirstSlot(int index) {
            UnchooseItem(emojiBar.GetItemId(0));
            ChooseItem(index);
        }

        private void UpdateUI() {
            var equipped = _items.Where(iter => iter.Equipped).ToList();
            emojiBar.gameObject.SetActive(true);
            emojiBar.UpdateEquippedItem(equipped);
            for (var i = 0; i < _items.Count; i++) {
                itemList.SetSelected(i, _items[i].Equipped);
            }
        }

        private void OnDestroy() {
            SendEquip();
        }

        private void OnDisable() {
            SendEquip();
        }

        private void SendEquip() {
            if (!_modified) {
                return;
            }
            _sendEquipCallback?.Invoke(
                (int) InventoryItemType.Emoji,
                (from item in _items where item.Equipped select (item.ItemId, (long)item.ExpirationAfter)).ToArray()
            );
        }
    }
}