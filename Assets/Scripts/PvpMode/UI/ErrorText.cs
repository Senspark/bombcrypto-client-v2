using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace PvpMode.UI {
    public class ErrorText : MonoBehaviour {
        [SerializeField]
        private Text errorText;

        public void SetMessage(string message) {
            errorText.text = message;
        }

        private void Start() {
            var fadeIn = errorText.DOFade(1, 0.4f);
            var fadeOut = errorText.DOFade(0, 0.4f);
            DOTween.Sequence()
                .Append(fadeIn)
                .AppendInterval(3)
                .Append(fadeOut)
                .AppendCallback(() => Destroy(errorText.gameObject));
        }
    }
}