using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.StoryModeScene.Scripts {
    public class BLDialogLuckyWheelRewardItem : MonoBehaviour {
        [SerializeField]
        public Image icon;

        [SerializeField]
        public Text quantity;

        [SerializeField]
        private TextMeshProUGUI quantityMeshPro;

        public void UpdateUI(Sprite sprite, string value) {
            icon.sprite = sprite;
            if (quantity != null) {
                quantity.text = value;
            }
            if (quantityMeshPro != null) {
                quantityMeshPro.text = value;
            }
        }
    }
}