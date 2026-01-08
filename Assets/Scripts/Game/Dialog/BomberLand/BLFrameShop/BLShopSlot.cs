using System;

using App;

using Senspark;

using UnityEngine;

namespace Game.Dialog.BomberLand.BLFrameShop {
    public class BLShopSlot : MonoBehaviour {
    
        [SerializeField]
        private GameObject selected;
    
        [SerializeField]
        private GameObject content;

        [SerializeField]
        private GameObject premiumFrame;
        
        [NonSerialized]
        public int Index = -1;
    
        public Action OnClickItem;

        private GameObject _createContent = null;

        protected void Awake() {
            selected.SetActive(false);
            content.SetActive(true);
        }

        public void OnClick() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if(_createContent == null) {
                return;
            }
            if(!content.activeSelf) {
                return;
            }
            if(selected.activeSelf) {
                return;
            }
            OnClickItem?.Invoke();
        }

        public void SetIsEmpty(bool b) {
            content.SetActive(!b);
        }

        public void SetSelected(bool b) {
            if (selected) {
                selected.SetActive(b);
            }
        }

        public void SetPremium(bool value) {
            premiumFrame.SetActive(value);
        }
        
        public GameObject CreateContentByPrefab(GameObject prefab) {
            if (_createContent) {
                return _createContent;
            }
            if (content.transform.childCount > 0) {
                return null;
            }
            _createContent = Instantiate(prefab, content.transform);
            return _createContent;
        }
    }
}
