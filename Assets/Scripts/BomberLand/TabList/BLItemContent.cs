using System.Collections.Generic;
using Game.Dialog;
using UnityEngine;

using Utils;

namespace Game.UI {
    public class BLItemContent : BaseBLContent {
        [SerializeField]
        private BaseBLItemInformation item;
        public BaseBLItemInformation ItemInformation => item;

        private List<ItemData> _items = new List<ItemData>();

        private void Awake() {
            itemList.SetOnSelectItem(ChooseItem);
            item.OnShowDialogCallback = ShowDialogItem;
            item.OnShowDialogOrderCallback = ShowDialogOrder;
            item.OnOrderErrorCallback = ShowOrderError;
        }
        
        public void ClearCacheData() {
            _items.Clear();
        }

        public override void SetData<T>(List<T> list, bool isUpdate) {
            if (list == null) {
                return;
            }
            
            foreach (var iter in list) {
                _items.Add(iter as ItemData);
            }
            // Hide ItemInformation
            item.gameObject.SetActive(false);
            itemList.LoadData(_items, isUpdate);
        }
        
        public void UpdateData<T>(List<T> list) {
            foreach (var iter in list) {
                _items.Add(iter as ItemData);
            }
            itemList.UpdateData(list);
        }

        public void RefreshMinPrice() {
            itemList.RefreshMinPrice().Forget();
        }

        public int GetInputAmount() {
            return item.GetInputAmount();
        }

        private void ChooseItem(int index) {
            item.gameObject.SetActive(true);
            item.UpdateItem(_items[index]);
        }

        private void ShowDialogItem(ItemData itemData) {
            OnShowDialogItem?.Invoke(itemData);
        }
        private void ShowDialogOrder(OrderDataRequest orderDataRequest) {
            OnShowDialogOrder?.Invoke(orderDataRequest);
        }
        private void ShowOrderError() {
            OnOrderErrorCallback?.Invoke();
        }
    }
}