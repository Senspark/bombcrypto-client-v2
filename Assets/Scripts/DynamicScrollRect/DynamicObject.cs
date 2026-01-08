using DynamicScrollRect;

using UnityEngine;

namespace Game.Dialog {
    public class DynamicObject : DynamicScrollObject<DynamicInventoryItem> {
        public override float CurrentHeight { get; set; }
        public override float CurrentWidth { get; set; }

        private DynamicInventoryItem _dynamicItem;

        public DynamicObject PrevObject { set; get; }
        public DynamicObject NextObject { set; get; }
        
        private void Awake() {
            CurrentHeight = GetComponent<RectTransform>().rect.height;
            CurrentWidth = GetComponent<RectTransform>().rect.width;
        }

        public void SetDynamicItem(DynamicInventoryItem item) {
            _dynamicItem = item;
        }

        public override void UpdateScrollObject(DynamicInventoryItem item, int index, int selectId = -1) {
            base.UpdateScrollObject(item, index);
            _dynamicItem.UpdateDynamicInfo(item);
            _dynamicItem.SetHighLight(item.Item.playerData.heroId.Id == selectId);
        }
    }
}