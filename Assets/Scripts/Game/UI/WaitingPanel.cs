using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class WaitingPanel : MonoBehaviour {
        [SerializeField]
        private RectTransform waitingRun;

        [SerializeField]
        private Text waitingTxt;

        private void Awake() {
            waitingRun
                .DORotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        private void OnDestroy() {
            waitingRun.DOKill();
        }

        public void ChangeText(string str) {
            if (waitingTxt) {
                waitingTxt.text = str;
            }
        }
    }
}