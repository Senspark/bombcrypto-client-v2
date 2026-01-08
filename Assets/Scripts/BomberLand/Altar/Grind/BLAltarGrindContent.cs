using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Game.UI {
    public class BLAltarGrindContent : BLAltarBaseContent {
        [SerializeField]
        private BLAltarGrindInformation grind;

        [SerializeField]
        protected BLAltarGrindList itemList;

        private List<TRHeroData> _itemGrinds;

        private int _chooseItemId = -1;

        private void Awake() {
            if (itemList == null) {
                return;
            }
            itemList.SetOnSelectItem(ChooseItem);
            grind.SetOnGrindCallback(DoGrind);
        }

        public void SetData(List<TRHeroData> list) {
            _itemGrinds = list;
            itemList.LoadData(_itemGrinds);
            SetNoItemText(_itemGrinds.Count == 0);
            if (_itemGrinds.Count > 0) {
                var chooseIndex = FindIndexFromItemId(_chooseItemId);
                itemList.SetSelectItem(chooseIndex);
                grind.SetActive(true);
            } else {
                grind.SetActive(false);
            }
        }

        private int FindIndexFromItemId(int itemId) {
            for (var i = 0; i < _itemGrinds.Count; i++) {
                if (itemId == _itemGrinds[i].ItemId) {
                    return i;
                }
            }
            return 0;
        }
        
        private void ChooseItem(int index) {
            var chooseGrind = _itemGrinds[index];
            _chooseItemId = chooseGrind.ItemId;
            grind.UpdateGrind(chooseGrind);
        }

        private void DoGrind(int itemId, int quantity, int cost, int status) {
            OnGrindCallback(itemId, quantity, cost, status);
        }
        
        public void SetEnableShowMore(Action<bool> callback) {
            itemList.SetEnableShowMore(callback);
        }

        public void UpdatePage() {
            itemList.UpdatePage();
        }
    }
}