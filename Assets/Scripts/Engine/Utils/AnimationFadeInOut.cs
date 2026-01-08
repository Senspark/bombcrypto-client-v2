using System;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class AnimationFadeInOut : MonoBehaviour {
    [SerializeField]
    private Image image;
    
    [SerializeField]
    private float duration = 2;

    [SerializeField]
    private float loopDelay = 1;

    [SerializeField]
    private bool loop = true;

    private Sequence _seq;
    
    private void Start() {
        Play();
    }

    private void OnDestroy() {
        _seq.Kill();
    }

    private void Play() {
        var fadeOut = image.DOFade(0, duration / 3f);
        var fadeIn = image.DOFade(1f, duration / 3f);
        _seq = DOTween.Sequence()
            .Append(fadeOut)
            .Append(fadeIn)
            .AppendInterval(loopDelay)
            .SetLoops(loop ? -1 : 1);
    }
}
