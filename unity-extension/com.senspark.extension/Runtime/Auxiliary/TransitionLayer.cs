using System;

using Cysharp.Threading.Tasks;

using DG.Tweening;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.UI;

namespace Senspark {
    [AddComponentMenu("Senspark/Transition Layer")]
    public class TransitionLayer : MonoBehaviour {
        private static TransitionLayer _sharedInstance;

        [MustUseReturnValue]
        private static TransitionLayer GetInstance() {
            if (_sharedInstance == null) {
                var key = $"Senspark/Prefabs/Auxiliary/{nameof(TransitionLayer)}";
                var prefab = Resources.Load<TransitionLayer>(key);
                _sharedInstance = Instantiate(prefab);
                DontDestroyOnLoad(_sharedInstance.gameObject);
            }
            return _sharedInstance;
        }

        public static async UniTask Fade(float duration, [NotNull] Func<UniTask> action) {
            try {
                await FadeIn(duration);
                await action();
            } finally {
                await FadeOut(duration);
            }
        }

        public static async UniTask<T> Fade<T>(float duration, [NotNull] Func<UniTask<T>> action) {
            try {
                await FadeIn(duration);
                return await action();
            } finally {
                await FadeOut(duration);
            }
        }

        public static async UniTask FadeIn(float duration) {
            var instance = GetInstance();
            await instance.FadeInInternal(duration);
        }

        public static async UniTask FadeOut(float duration) {
            var instance = GetInstance();
            await instance.FadeOutInternal(duration);
        }

        [SerializeField]
        private Image _image;

        private Tween _tween;

        private void Awake() {
            var color = _image.color;
            color.a = 0;
            _image.color = color;
            _image.gameObject.SetActive(false);
        }

        private void OnDestroy() {
            _tween?.Kill();
            _tween = null;
        }

        private UniTask FadeInInternal(float duration) {
            var tcs = new UniTaskCompletionSource<object>();
            _tween?.Kill();
            _image.gameObject.SetActive(true);
            _tween = DOTween.Sequence()
                .Append(_image.DOFade(1, duration))
                .AppendCallback(() => { //
                    tcs.TrySetResult(null);
                });
            return tcs.Task;
        }

        private UniTask FadeOutInternal(float duration) {
            var tcs = new UniTaskCompletionSource<object>();
            _tween?.Kill();
            _tween = DOTween.Sequence()
                .Append(_image.DOFade(0, duration))
                .AppendCallback(() => {
                    _image.gameObject.SetActive(false);
                    tcs.TrySetResult(null);
                });
            return tcs.Task;
        }
    }
}