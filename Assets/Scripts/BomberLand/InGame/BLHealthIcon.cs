using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.InGame {
    public class BLHealthIcon : MonoBehaviour {
        [SerializeField]
        private Image iconOn;

        public void ShowOn(bool value) {
            iconOn.gameObject.SetActive(value);
        }
    }
}