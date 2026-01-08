using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Inventory {
    public class BLInventoryStat : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text valueText;

        [SerializeField]
        private Sprite[] sprites;
    
        public void SetInfo(int statId, int value) {
            icon.sprite = sprites[statId];
            valueText.text = $"+{value}";
        }
    }
}