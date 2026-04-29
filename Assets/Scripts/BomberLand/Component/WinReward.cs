using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Component {
    public class WinReward : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Image iconGray;

        [SerializeField]
        private Text valueText;

        [SerializeField]
        private RewardResource resource;

        public float Value { get; private set; }

        public void SetInfo(RewardSourceType type, float value, bool fullSlot) {
            icon.sprite = resource.GetSprite(type);
            iconGray.sprite = resource.GetSprite(type);
            if (fullSlot) {
                iconGray.gameObject.SetActive(true);
                valueText.text = "Full Slot";
                return;
            }
            iconGray.gameObject.SetActive(false);
            Value = value;
            valueText.text = Value % 1 == 0 ? $"{Value:0}" : $"{Value:0.##}";
        }

        public void AddValue(float value) {
            Value += value;
            valueText.text = Value % 1 == 0 ? $"{Value:0}" : $"{Value:0.##}";
        }
        
        public void SetInfo(RewardSourceType type, float value) {
            icon.sprite = resource.GetSprite(type);
            valueText.text = value % 1 == 0 ? $"{value:0}" : $"{value:0.##}";
        }
    }
}