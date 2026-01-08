using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class FusionPercentDisplay : MonoBehaviour {
        [SerializeField]
        private Image arrow;

        [SerializeField]
        private Text percentLbl;

        public void Init(float percent) {
            percentLbl.text = $"{percent}%";
        }
    }
}