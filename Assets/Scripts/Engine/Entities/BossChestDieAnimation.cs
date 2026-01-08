using DG.Tweening;

using Engine.Entities;
using Engine.Manager;
using Engine.Strategy.Provider;

using UnityEngine;

public class BossChestDieAnimation : Entity {
    [SerializeField]
    private Animator animator;

    private static readonly int Die = Animator.StringToHash("Die");

    public static BossChestDieAnimation Create(IEntityManager entityManager, Entity entityPrefab, Transform parent, Vector3 position) {
        var provider = new ExhaustedProvider(entityPrefab);
        var instance = (BossChestDieAnimation) provider.CreateInstance(entityManager);
        Transform transform1;
        (transform1 = instance.transform).SetParent(parent, false);
        transform1.position = position;
        entityManager.AddEntity(instance);
        return instance;
    }

    private void Awake() {
        PlayDieAnimation();
    }

    public void PlayDieAnimation() {
        animator.SetTrigger(Die);
        DOTween.Sequence()
            .AppendInterval(2)
            .OnComplete(() => Kill(true));
    }
}
