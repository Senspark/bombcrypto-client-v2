using PvpMode.Manager;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace PvpMode.UI {
    public class BoosterItemShop : MonoBehaviour {
        [SerializeField]
        private BoosterType boosterType;
        
        [SerializeField]
        private Image highlight;

        [SerializeField]
        private Text price;

        [FormerlySerializedAs("_onValueChanged")]
        [SerializeField]
        private UnityEvent<bool> onValueChanged;

        public System.Action<int> OnChooseItem;

        private int _price;
        
        public BoosterType GetBoosterType() {
            return boosterType;
        }
        
        public void SetPrice(int value) {
            _price = value;
            price.text = "" + value;
        }

        public int GetPrice() {
            return _price;
        }
        
        public void OnButtonClicked() {
            SetChoose(true);
            OnChooseItem?.Invoke(transform.GetSiblingIndex());
        }

        public void SetChoose(bool value) {
            highlight.gameObject.SetActive(value);
            onValueChanged.Invoke(value);
        }
    }
}