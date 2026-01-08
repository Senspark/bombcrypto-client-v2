using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace Engine.Utils {
    public class ImageAnimationFadeIn : MonoBehaviour {
        [SerializeField]
        private float delayBegin = 0;

        [SerializeField]
        private float duration = 2;

        private Image _image;

        private void Awake() {
            _image = GetComponent<Image>();
            var color = _image.color;
            color.a = 0;
            _image.color = color;
        }

        private void Start() {
            StartFadeIn();
        }

        private void StartFadeIn() {
            _image.DOFade(1, duration).SetDelay(delayBegin);
        }
    }
}