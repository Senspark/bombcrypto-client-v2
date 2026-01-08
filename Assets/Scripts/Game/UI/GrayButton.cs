using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class GrayButton : MonoBehaviour {
        [SerializeField]
        private Sprite grayColor;
        
        [SerializeField]
        private Color darkColor = new(0.7f, 0.7f, 0.7f, 1f);

        private Sprite _originColor;
        private Image _img;
        private Button _btn;

        private void Awake() {
            _img = GetComponent<Image>();
            _btn = GetComponent<Button>();
            _originColor = _img.sprite;
        }

        public void SetActive(bool enable) {
            gameObject.SetActive(enable);
        }

        public void SetEnable(bool enable) {
            _btn.enabled = enable;
        }

        public void SetColor(ButtonColor buttonColor) {
            if (!_img) {
                return;
            }
            if (buttonColor == ButtonColor.Bright) {
                _img.sprite = _originColor;
                _img.color = Color.white;
            } else if (buttonColor == ButtonColor.Dark) {
                _img.sprite = _originColor;
                _img.color = darkColor;
            } else {
                _img.sprite = grayColor;
                _img.color = Color.white;
            }
        }

        public enum ButtonColor {
            Bright,
            Dark,
            Gray
        }
    }
}