using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.DailyGift {
    public class BLDailyGiftReward : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text quantity;

        public void UpdateUI(Sprite sprite, int amount) {
            if (sprite) {
                icon.sprite = sprite;
            }
            quantity.text = $"{amount}";
            quantity.gameObject.SetActive(amount > 0);
        }
    }
}