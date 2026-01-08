using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using Game.UI.Custom;

using UnityEngine;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLSegmentShopWithItem : BLSegmentShop {
        [SerializeField]
        public GameObject frameSubContent;

        [SerializeField]
        public GameObject prefabSlot;

        [SerializeField]
        public GameObject prefabInfo;

        [SerializeField]
        public CustomContentSizeFitter contentSizeFitter;
        
        [SerializeField]
        private GameObject noItemText;
        
        private BLShopSlot[] _items = null;

        private GameObject _objInfo = null;
        public GameObject ObjInfo => _objInfo;

        public BLShopSlot[] Items => _items;

        public Action<int> OnSelectInfo;
        public Action OnShowDialogInfo { set; private get; }

        protected override void Awake() {
            base.Awake();
            if (!prefabInfo || _objInfo || !frameSubContent) {
                return;
            }
            _objInfo = Instantiate(prefabInfo, frameSubContent.transform);
            _objInfo.SetActive(false);
        }

        public void AutoLayoutVertical() {
            if (contentSizeFitter != null) {
                contentSizeFitter.AutoLayoutVertical();
            }
        }
        
        public GameObject[] InitSlot(int numSlot) {
            var items = GetComponentsInChildren<BLShopSlot>();
            if (items.Length <= 0) {
                return null;
            }
            if (items.Length <= numSlot) {
                var it = items.Last();
                for (var i = items.Length; i < numSlot; i++) {
                    Instantiate(it, it.transform.parent);
                }
                items = GetComponentsInChildren<BLShopSlot>();
            }
            Debug.Assert(items.Length >= numSlot);
            var objs = new GameObject[numSlot];
            for (var idx = 0; idx < items.Length; idx++) {
                var item = items[idx];
                item.Index = idx;
                if (idx >= numSlot) {
                    item.SetIsEmpty(true);
                    item.gameObject.SetActive(false);
                    continue;
                }
                item.SetIsEmpty(false);
                item.OnClickItem = () => { ItemSelect(item.Index); };
                objs[idx] = item.CreateContentByPrefab(prefabSlot);
                objs[idx].SetActive(true);
            }
            _items = items;
            if (noItemText) {
                noItemText.SetActive(numSlot == 0);
            }
            return objs;
        }

        /*
         * Call when item onClick
         */
        private void ItemSelect(int idxSlotSelected) {
            if (_items == null) {
                return;
            }
            if (idxSlotSelected < 0) {
                return;
            }
            if (idxSlotSelected >= _items.Length) {
                return;
            }
            // Snap to Current Segment
            // this.ForceSelectMenu?.Invoke();
            for (var idx = 0; idx < _items.Length; idx++) {
                var item = _items[idx];
                item.SetSelected(idx == idxSlotSelected);
            }
            if (frameSubContent) {
                frameSubContent.SetActive(true);
            }
            OnSelectInfo?.Invoke(idxSlotSelected);
        }

        public void UnSelectAllItem() {
            if (_items == null) {
                return;
            }
            foreach (var item in _items) {
                item.SetSelected(false);
            }
        }

        public void SelectFirstItem() {
            if (_items == null) {
                return;
            }
            if (_items.Length <= 0) {
                return;
            }
            for (var idx = 0; idx < _items.Length; idx++) {
                if (!_items[idx].gameObject.activeSelf) {
                    continue;
                }
                ItemSelect(idx);
                return;
            }
        }

        public void HideItemAt(int idx) {
            _items[idx].gameObject.SetActive(false);
        }

        public void OnInfoButtonClicked() {
            OnShowDialogInfo?.Invoke();
        }
    }
}