using System;
using Constant;
using Data;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Dialog.BomberLand.BLGacha {
    public class BLGachaChestReward : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
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
        private GachaChestProductId productId;
        
        [SerializeField]
        public GameObject premiumFrame;

        private Action _onSelect;

        [CanBeNull]
        private Sequence _fadeSequence = null;

        public int ProductId => (int) productId;
        
        private bool _usingOnPointEnter = true;

        protected void OnDestroy() {
            Destroy(objDescription);
        }

        public async void Initialize(GachaChestItemData data, string des, Action onSelect) {
            productId = data.ProductId;
            icon.sprite = await resource.GetSpriteByItemId(data.ProductId);
            quantity.text = $"{data.Value}";
            description.text = des;
            var wText = description.preferredWidth + 40;
            if (wText > 400) {
                wText = 400;
            }
            var rtF = objDescription.GetComponent<RectTransform>();
            rtF.sizeDelta = new Vector2(wText, rtF.sizeDelta.y);
            _onSelect = () => { onSelect?.Invoke(); };
            if (premiumFrame != null) {
                premiumFrame.SetActive(data.ItemKind == ProductItemKind.Premium);
            }
        }

        public async void Initialize(GachaChestShopItemData data, Action onSelect) {
            productId = (GachaChestProductId)data.ProductId;
            icon.sprite = await resource.GetSpriteByItemId(data.ProductId);
            quantity.gameObject.SetActive(false);
            description.text = data.ProductDescription;
            var wText = description.preferredWidth + 40;
            if (wText > 400) {
                wText = 400;
            }
            var rtF = objDescription.GetComponent<RectTransform>();
            rtF.sizeDelta = new Vector2(wText, rtF.sizeDelta.y);
            _onSelect = () => { onSelect?.Invoke(); };
        }
        
        public void TurnOfOnPointEnter() {
            _usingOnPointEnter = false;
        }

        public void OnTriggerSelect() {
            UiShowDescription(false);
            _onSelect?.Invoke();
        }

        public void SetSelected(bool state) {
            selected.SetActive(state);
        }
        
        public void OnPointerEnter(PointerEventData eventData) {
            if (_usingOnPointEnter) {
                UiShowDescription(true);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData) {
            if (_usingOnPointEnter) {
                UiShowDescription(false);
            }
        }

        public void UiShowDescription(bool isShow) {
            selected.SetActive(isShow);
            objDescription.SetActive(isShow);
            if (!isShow) {
                return;
            }
            // Move objDescription to parent
            var tfParent = transform.parent.parent;
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