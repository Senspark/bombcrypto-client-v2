using System;

using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace StoryMode.Component {
    public class ImageBlink : MonoBehaviour {
        [SerializeField]
        private Image image;

        private void Start() {
            image.DOFade(0.0f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDestroy() {
            image.DOKill();
        }
    }
}
