using System;

using DG.Tweening;

using UnityEngine;

namespace Game.UI {
    public class AniUpDown : MonoBehaviour {
        [SerializeField]
        private Transform hand;

        [SerializeField]
        private Vector2 movement;

        [SerializeField]
        private float duration = 0.5f;

        private bool _started;

        private void Start() {
        }

        private void OnEnable() {
            if (!_started) {
                ApplyAni(hand.localPosition);
            }
        }

        private void OnDisable() {
            hand.DOKill();
            _started = false;
        }

        private void ApplyAni(Vector2 localPosition) {
            if (!_started) {
                _started = true;
            }
            hand.DOKill();
            hand.DOLocalMove(localPosition + movement, duration)
                .ChangeStartValue(localPosition)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}