using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class AirDropEventButton : MonoBehaviour {
        [SerializeField]
        private Text text;
        
        [SerializeField]
        private Image background;

        public void SetActive(bool active) {
            if (active) {
                text.color = new Color32(119, 226, 0, 255);
                background.color = new Color32(217, 255, 208, 255);
            } else {
                text.color = background.color = Color.white;
            }
        }
    }
}