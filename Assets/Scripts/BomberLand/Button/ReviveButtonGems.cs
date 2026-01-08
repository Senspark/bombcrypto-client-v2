using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Button {
    public class ReviveButtonGems : MonoBehaviour {
        [SerializeField]
        private Text valueText;

        public void SetInfo(int value) {
            valueText.text = $"{value}";
        }
    }
}