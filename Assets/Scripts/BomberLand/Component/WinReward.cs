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

        public int Value { get; private set; }

        public void SetInfo(RewardSourceType type, int value, bool fullSlot) {
            icon.sprite = resource.GetSprite(type);
            iconGray.sprite = resource.GetSprite(type);
            if (fullSlot) {
                iconGray.gameObject.SetActive(true);
                valueText.text = "Full Slot";
                return;
            }
            iconGray.gameObject.SetActive(false);
            Value = value;
            valueText.text = $"{Value}";
        }

        public void AddValue(int value) {
            Value += value;
            valueText.text = $"{Value}";
        }
        
        public void SetInfo(RewardSourceType type, int value) {
            icon.sprite = resource.GetSprite(type);
            valueText.text = $"{value}";
        }
    }
}