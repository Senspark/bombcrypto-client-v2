using UnityEngine;

namespace Game.Dialog.BomberLand {
    public class BLRatingElement : MonoBehaviour {
        [SerializeField]
        private GameObject off;

        [SerializeField]
        private GameObject on;

        public void OnValueChanged(bool value) {
            off.SetActive(!value);
            on.SetActive(value);
        }
    }
}