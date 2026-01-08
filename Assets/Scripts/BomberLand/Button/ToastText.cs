using DG.Tweening;

using TMPro;

using UnityEngine;

namespace BomberLand.Button {
    public class ToastText : MonoBehaviour {
        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private Transform banner;
        
        [SerializeField]
        private TextMeshProUGUI textMessage;

        private const float Distance = 200;
        
        public static ToastText Create() {
            var prefab = Resources.Load<ToastText>("Prefabs/UI/ToastText");
            return Instantiate(prefab);
        }
        
        public ToastText SetText(string message) {
            textMessage.text = message;
            return this;
        }

        public void Show(Canvas canvas) {
            canvasGroup.alpha = 0;
            banner.SetParent(canvas.transform, false);
            var position = banner.position;
            position.y -= Distance;
            banner.position = position;
            
            FadeInAndOut();        
        }

        public void FadeInAndOut() {
            var moveUp = banner.DOMoveY(Distance, 1).SetRelative(true);
            var fadeIn = canvasGroup.DOFade(1, 1);
            var fadeOut = canvasGroup.DOFade(0, 1);

            var moveAndFade = DOTween.Sequence()
                .Append(moveUp)
                .Join(fadeIn);

            DOTween.Sequence()
                .Append(moveAndFade)
                .AppendInterval(0.3f)
                .Append(fadeOut)
                .OnComplete(() => { Destroy(gameObject); });
        }

    }
}
