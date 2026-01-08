using System;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI;
using Senspark;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLAltarFuseItem : MonoBehaviour {
        [SerializeField]
        private Text nameText;
        
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text valueText;

        [SerializeField]
        private Image highLight;

        [SerializeField]
        private BLGachaRes resource;
        
        private int _index;
        private Action<int> _onClickCallback;
        private IProductItemManager _productItemManager;

        public async void SetInfo(int index, IUiCrystalData data, Action<int> callback) {
            _index = index;
            _onClickCallback = callback;
            icon.sprite = await resource.GetSpriteByItemId(data.ItemId);
            valueText.text = $"x{data.Quantity}";
            
            _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
            nameText.text = _productItemManager.GetItem(data.ItemId).Name;
        }

        public void SetSelected(bool value) {
            highLight.gameObject.SetActive(value);
        }

        public void OnClicked() {
            _onClickCallback?.Invoke(_index);
        }
        
        public void SetInvisible(bool value) {
            gameObject.SetActive(value);
        }
    }
}