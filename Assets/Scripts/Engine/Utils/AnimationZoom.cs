using System;

using DG.Tweening;

using UnityEngine;

namespace Engine.Utils {
    public class AnimationZoom : MonoBehaviour {
        public float duration;
        public float loopDelay;
        public bool loop;

        private Sequence _seq;

        private void OnDestroy() {
            Stop();
        }

        public void Play() {
            var zoomOut = transform.DOScale(1.2f, duration / 3f);
            var zoomIn = transform.DOScale(0.8f, duration / 3f);
            var reset = transform.DOScale(1f, duration / 3f);
            _seq = DOTween.Sequence()
                .Append(zoomOut)
                .Append(zoomIn)
                .Append(reset)
                .AppendInterval(loopDelay)
                .SetLoops(loop ? -1 : 1);
        }
        
        public void Stop() {
            _seq?.Kill();
            _seq = null;
            transform.localScale = Vector3.one;
        }
    }
}