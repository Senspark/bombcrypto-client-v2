using System;
using System.Collections.Generic;
using System.Linq;

using Game.Dialog;

using GameUI;

using UnityEngine;

namespace Game.UI {
    public class BLInventoryEmojiBar : BaseBLItemInformation {
        [SerializeField]
        private BLInventoryEmojiSlot[] slots;

        private Action<int> _clickOnBarCallback;

        public void SetClickedCallback(Action<int> callback) {
            _clickOnBarCallback = callback;
            foreach (var slot in slots) {
                slot.SetClickedCallback(OnSlotClicked);
            }
        }

        public void UpdateEquippedItem(List<ItemData> items) {
            var i = 0;
            for (; i < items.Count && i < slots.Length; i++) {
                slots[i].SetItemToSlot(items[i].ItemId);
            }
            for (; i < slots.Length; i++) {
                slots[i].UnsetItem();
            }
        }
        
        public bool HadEmptySlot() {
            return slots.Any(slot => slot.IsEmpty());
        }

        public int GetItemId(int index) {
            return slots[index].GetItemId();
        }
        
        private void OnSlotClicked(int itemId) {
            _clickOnBarCallback?.Invoke(itemId);
        }
    }
}