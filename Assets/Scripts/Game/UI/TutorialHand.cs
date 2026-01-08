using System;

using DG.Tweening;

using UnityEngine;

namespace Game.UI {
    public class TutorialHand : MonoBehaviour {
        [SerializeField]
        private RectTransform hand;

        [SerializeField]
        private Vector2 movement;

        [SerializeField]
        private float duration = 0.5f;

        private bool _started = false;

        private void Start() { }

        private void OnEnable() {
            if (!_started) {
                SetNewPosition(hand.anchoredPosition);
            }
        }

        private void OnDisable() {
            // hand.DOPause();
            // _started = false;
        }

        private void OnDestroy() {
            hand.DOKill();
        }

        private void SetNewPosition(Vector2 anchoredPosition) {
            _started = true;
            // hand.DOKill();
            hand.DOAnchorPos(anchoredPosition + movement, duration)
                .ChangeStartValue(anchoredPosition)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}