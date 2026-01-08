using Constant;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class UpgradeStatFee : MonoBehaviour {
        [SerializeField]
        private GachaChestProductId itemId;
        
        [SerializeField]
        private Text valueText;

        public GachaChestProductId ProductId => itemId;

        public void SetValue(int value, int currentValue) {
            if (value <= 0) {
                gameObject.SetActive(false);
                return;
            }
            valueText.text = $"x{value}";
            valueText.color = value <= currentValue ? Color.white : Color.red;
            gameObject.SetActive(true);
        }
    }
}