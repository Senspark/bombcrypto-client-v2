using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Button {
    public class ButtonWithHotKey : MonoBehaviour {
        [SerializeField]
        private Image hotKey;

        private void Start() {
            if (hotKey != null) {
                hotKey.gameObject.SetActive(!Application.isMobilePlatform);
            }
        }
    }
}