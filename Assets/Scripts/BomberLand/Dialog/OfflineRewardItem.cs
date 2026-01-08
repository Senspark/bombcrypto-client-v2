using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class OfflineRewardItem : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text quantity;

        public void UpdateUI(Sprite sprite, int value) {
            icon.sprite = sprite;
            quantity.text = $"+{value}";
        }
    }
}