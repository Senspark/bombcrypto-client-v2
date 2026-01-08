using UnityEngine;
using UnityEngine.UI;

namespace BomberLand.Button {
    public class XButton : MonoBehaviour {
        [SerializeField]
        private Image frameImg;

        [SerializeField]
        private Text xText;

        [SerializeField]
        private int index;

        [SerializeField]
        private Color activeColor;

        public int Index => index;

        public void SetActive(bool value) {
            frameImg.gameObject.SetActive(value);
            xText.color = value ? activeColor : Color.white;
        }
    }
}