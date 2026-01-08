using DG.Tweening;

using UnityEngine;

namespace PvpMode.Utils {
    public class Toast : MonoBehaviour
    {
        private void Awake() {
            gameObject.SetActive(false);
        }

        public void Show() {
            DOTween.Sequence()
                .AppendCallback(() => gameObject.SetActive(true))
                .AppendInterval(1)
                .AppendCallback(() => gameObject.SetActive(false));
        }
    }
}
