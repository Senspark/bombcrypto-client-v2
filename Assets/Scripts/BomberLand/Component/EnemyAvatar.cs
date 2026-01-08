using Engine.Entities;

using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Component {
    public class EnemyAvatar : MonoBehaviour {
        [SerializeField]
        private Image icon;

        [SerializeField]
        private Text quantityText;

        private int _quantity;

        public void ChangeImage(EnemyType enemyType) {
            var spriteSheetName = $"Enemies/{enemyType}/Front/front_00";
            icon.sprite = Resources.Load<Sprite>(spriteSheetName);
        }

        public void IncreaseQuantity() {
            _quantity += 1;
            quantityText.text = $"{_quantity}";
        }

        public void DecreaseQuantity() {
            _quantity -= 1;
            quantityText.text = $"{_quantity}";
        }

        public void UpdateQuantity(int value) {
            _quantity = value;
            quantityText.text = $"{_quantity}";
        }
    }
}