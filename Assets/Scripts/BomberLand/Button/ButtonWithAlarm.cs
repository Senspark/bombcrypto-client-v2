using UnityEngine;

namespace BomberLand.Button {
    public class ButtonWithAlarm : MonoBehaviour {
        [SerializeField]
        private Sprite greySprite;

        [SerializeField]
        private Sprite normalSprite;

        [SerializeField]
        private UnityEngine.UI.Button button;

        public bool Interactable {
            get => button.interactable;
            set => button.interactable = value;
        }
        
        public void SetGrey(bool value) {
            if (value) {
                button.image.sprite = greySprite;
            } else {
                button.image.sprite = normalSprite;
            }
        }

        public void ShowAlarm(Canvas canvas) {
            ToastText.Create().Show(canvas);
        }
    }
}