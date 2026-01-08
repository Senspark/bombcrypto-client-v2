using System;

using DG.Tweening;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace Scenes.ConnectScene.Scripts {
    public class MainLoadingBar : MonoBehaviour {
        [SerializeField]
        private RectTransform container;
        
        [SerializeField]
        private TextMeshProUGUI percentTxt;
        
        [SerializeField]
        private TextMeshProUGUI descTxt;

        [SerializeField]
        private Image progress;

        [SerializeField]
        private float maxProgress = 584;

        [SerializeField]
        private float startY;
        
        [SerializeField]
        private float endY;

        [SerializeField]
        private float moveTime = 0.5f;

        private Tween _tween;

        private void Awake() {
            UpdateProgress(0, string.Empty);
        }

        private void OnDestroy() {
            _tween?.Kill();
        }

        public void UpdateProgress(int value, string desc) {
            percentTxt.text = $"{value} %";
            descTxt.text = desc;
            const float xMin = 0;
            var size = progress.rectTransform.sizeDelta;
            var x = value / 100f * maxProgress;
            if (x > 0 && x < xMin) {
                x = xMin;
            }
            size.x = x;
            progress.rectTransform.sizeDelta = size;
        }

        public void MoveUp(Action onCompleted = null) {
            var from = new Vector2(0, endY);
            if (!gameObject.activeSelf) {
                container.anchoredPosition = from;
                gameObject.SetActive(true);
            }
            _tween?.Kill();
            _tween = container
                .DOAnchorPosY(startY, 0.5f)
                .ChangeStartValue(from)
                .SetDelay(moveTime)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() => onCompleted?.Invoke());
        }

        public void MoveDown(Action onCompleted = null) {
            _tween?.Kill();
            _tween = container
                .DOAnchorPosY(endY, 0.5f)
                .ChangeStartValue(new Vector2(0, startY))
                .SetDelay(moveTime)
                .SetEase(Ease.InOutCubic)
                .OnComplete(() => onCompleted?.Invoke());
        }
    }
}