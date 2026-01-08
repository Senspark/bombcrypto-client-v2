using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLGacha {
    public class BLGachaMultiChestReward : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text quantity;

        [SerializeField]
        private Text description;

        [SerializeField]
        private GameObject objDescription;

        [SerializeField]
        private GameObject selected;

        [SerializeField]
        private BLGachaRes resource;
        
        [SerializeField]
        public GameObject premiumFrame;

        protected void OnDestroy() {
            Destroy(objDescription);
        }

        public async void Initialize(GachaChestItemData data, string des) {
            icon.sprite = await resource.GetSpriteByItemId(data.ProductId);
            quantity.text = $"{data.Value}";
            description.text = des;
            var wText = description.preferredWidth + 40;
            if (wText > 400) {
                wText = 400;
            }
            var rtF = objDescription.GetComponent<RectTransform>();
            rtF.sizeDelta = new Vector2(wText, rtF.sizeDelta.y);
            if (premiumFrame != null) {
                premiumFrame.SetActive(data.ItemKind == ProductItemKind.Premium);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            UiShowDescription(true);
        }
        
        public void OnPointerExit(PointerEventData eventData) {
            UiShowDescription(false);
        }

        public void UiShowDescription(bool isShow) {
            selected.SetActive(isShow);
            objDescription.SetActive(isShow);
            if (!isShow) {
                return;
            }
            // Move objDescription to parent
            var tfParent = transform.parent.parent.parent;
            if (objDescription.transform.parent != tfParent) {
                if (!objDescription.GetComponent<LayoutElement>()) {
                    objDescription.AddComponent<LayoutElement>();
                }
                var lastPos = objDescription.transform.position;
                objDescription.GetComponent<LayoutElement>().ignoreLayout = true;
                objDescription.transform.SetParent(tfParent, true);
                objDescription.transform.SetSiblingIndex(tfParent.childCount);
                objDescription.transform.position = lastPos;
            }
        }
    }
}