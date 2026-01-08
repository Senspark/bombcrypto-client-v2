using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI {
    public class BLAltarFuseContent : BLAltarBaseContent {
        [SerializeField]
        private BLAltarFuseInformation fuse;

        [SerializeField]
        protected BLAltarFuseList itemList;

        private List<IUiCrystalData> _itemFuses;

        private int _chooseItemId = 0;
        
        private void Awake() {
            if (itemList == null) {
                return;
            }
            itemList.SetOnSelectItem(ChooseItem);
            fuse.SetOnFuseCallback(DoFuse);
        }
        
        public void SetData(List<IUiCrystalData> list) {
            _itemFuses = list;
            itemList.LoadData(_itemFuses);
            SetNoItemText(_itemFuses.Count == 0);
            if (_itemFuses.Count > 0) {
                var chooseIndex = FindIndexFromItemId(_chooseItemId);
                itemList.SetSelectItem(chooseIndex);
                fuse.SetActive(true);
            } else {
                fuse.SetActive(false);
            }
        }

        private int FindIndexFromItemId(int itemId) {
            for (var i = 0; i < _itemFuses.Count; i++) {
                if (itemId == _itemFuses[i].ItemId) {
                    return i;
                }
            }
            return 0;
        }
        
        private void ChooseItem(int index) {
            var chooseFuse = _itemFuses[index];
            _chooseItemId = chooseFuse.ItemId;
            fuse.UpdateFuse(chooseFuse);
        }

        private void DoFuse(int itemId, int targetId, int quantity, int costGold, int costGem) {
            OnFuseCallback(itemId, targetId, quantity, costGold, costGem);
        }
        
        public void SetEnableShowMore(Action<bool> callback) {
            itemList.SetEnableShowMore(callback);
        }

        public void UpdatePage() {
            itemList.UpdatePage();
        }
    }
}