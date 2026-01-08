using System;

using DG.Tweening;

using Engine.Components;
using Engine.Entities;
using Engine.Manager;
using Engine.Strategy.Provider;

using UnityEngine;

public class Thunder : Entity {
    [SerializeField]
    private SpriteRenderer spriteRenderer;
    
    [SerializeField]
    private Animator animator;

    [SerializeField]
    private float moveDuration;

    [SerializeField]
    private float animationDuration;

    [SerializeField]
    private float height;

    private static readonly int AnimationTrigger = Animator.StringToHash("Animation");
    private Transform _follow;
    private Sequence _sequence;
    
    public static Thunder Create(IEntityManager entityManager, Transform parent) {
        var provider = new PoolableProvider("Prefabs/Entities/Thunder");
        var instance = (Thunder) provider.CreateInstance(entityManager);
        instance.transform.SetParent(parent, false);
        entityManager.AddEntity(instance);

        return instance;
    }

    private void OnDisable() {
        _sequence?.Kill();
    }

    public void StartAnimation(Action onThunder, Action afterThunder) {
        transform.localPosition += new Vector3(0, height, 0);
        transform.localScale = Vector3.zero;
        
        _sequence = DOTween.Sequence();
        _sequence
            .Append(transform.DOScale(Vector3.one, moveDuration).SetEase(Ease.OutElastic))
            .AppendCallback(() => {
                animator.SetTrigger(AnimationTrigger);
                onThunder?.Invoke();
            })
            .AppendInterval(animationDuration)
            .AppendCallback(() => afterThunder?.Invoke())
            .Append(transform.DOScale(Vector3.zero, moveDuration).SetEase(Ease.InElastic))
            .OnComplete(() => Kill(true));
    }
}