using System;
using System.Linq;

using UnityEngine;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLSegmentShopSubscription : BLSegmentShop {
        [SerializeField]
        public GameObject frameSubContent;

        [SerializeField]
        public GameObject prefabSlot;

        [SerializeField]
        public GameObject prefabInfo;

        public BLShopSlot[] Items { get; private set; } = null;

        public GameObject ObjInfo { get; private set; } = null;

        public Action<int> OnSelectInfo;

        protected override void Awake() {
            base.Awake();
            if (!prefabInfo || ObjInfo || !frameSubContent) {
                return;
            }
            ObjInfo = Instantiate(prefabInfo, frameSubContent.transform);
            ObjInfo.SetActive(false);
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
            Items = items;
            return objs;
        }

        private void ItemSelect(int idxSlotSelected) {
            if (Items == null) {
                return;
            }
            if (idxSlotSelected < 0) {
                return;
            }
            if (idxSlotSelected >= Items.Length) {
                return;
            }
            // Snap to Current Segment
            // this.ForceSelectMenu?.Invoke();
            for (var idx = 0; idx < Items.Length; idx++) {
                var item = Items[idx];
                item.SetSelected(idx == idxSlotSelected);
            }
            if (frameSubContent) {
                frameSubContent.SetActive(true);
            }
            OnSelectInfo?.Invoke(idxSlotSelected);
        }
        
        public void SelectFirstItem() {
            if (Items == null) {
                return;
            }
            if (Items.Length <= 0) {
                return;
            }
            for (var idx = 0; idx < Items.Length; idx++) {
                if (!Items[idx].gameObject.activeSelf) {
                    continue;
                }
                ItemSelect(idx);
                return;
            }
        }
    }
}