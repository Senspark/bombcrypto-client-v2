using Game.UI;

using UnityEngine;
using UnityEngine.EventSystems;

namespace BLPvpMode.UI {
    public class EquipmentUI : MonoBehaviour {
        [SerializeField]
        private bool showTip = true;

        [SerializeField]
        private BLEquipmentItem[] equipmentItems;

        private void Awake() {

            var pointerEnter = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener(OnPointerEnter);

            var pointerExit = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener(OnPointerExit);


            if (!showTip) {
                return;
            }
            for (var i = 0; i < equipmentItems.Length; i++) {
                var equipmentItem = equipmentItems[i];
                var ev = equipmentItem.GetComponent<EventTrigger>();
                if (ev == null) {
                    ev = equipmentItem.gameObject.AddComponent<EventTrigger>();
                }
                ev.triggers.Add(pointerEnter);
                ev.triggers.Add(pointerExit);
            }
        }

        private void OnPointerEnter(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerEnter);
        }

        private void OnPointerExit(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerExit);
        }
        
        private void OnPointerDown(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerDown);
        }
        
        private void OnPointerUp(BaseEventData data) {
            OnPointerEvent(data, EventTriggerType.PointerUp);
        }
        
        private void OnPointerEvent(BaseEventData data, EventTriggerType eventType) {
            var pointerEventData = (PointerEventData) data;
            if (pointerEventData == null) {
                return;
            }
            var equipmentItem = eventType switch {
                EventTriggerType.PointerEnter => pointerEventData.pointerEnter.GetComponent<BLEquipmentItem>(),
                EventTriggerType.PointerExit => pointerEventData.pointerEnter.GetComponent<BLEquipmentItem>(),
                _ => null
            };
            if (equipmentItem == null) {
                return;
            }
            switch (eventType) {
                case EventTriggerType.PointerEnter:
                    for (var i = 0; i < equipmentItems.Length; i++) {
                        var item = equipmentItems[i];
                        if (item != equipmentItem) {
                            item.HideTip();
                        }
                    }
                    equipmentItem.ShowTip();
                    break;
                case EventTriggerType.PointerExit:
                    equipmentItem.HideTip();
                    break;
            }
        }
    }
}