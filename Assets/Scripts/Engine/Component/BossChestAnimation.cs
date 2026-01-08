using Engine.Entities;

using UnityEngine;

namespace Engine.Components {
    public class BossChestAnimation : MonoBehaviour {
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private Health health;

        [SerializeField]
        private Entity entityDiePrefab;
        
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Hit = Animator.StringToHash("Hit");
        private Entity _entity;

        private void Awake() {
            _entity = GetComponent<Entity>();
            health.SetOnTakeDamage(OnTakeDamage);
        }

        public void PlayIdleAnimation() {
            animator.SetTrigger(Idle);
        }

        public void PlayHitAnimation() {
            animator.SetTrigger(Hit);
        }

        private void OnTakeDamage(float currentHealth, DamageFrom _) {
            if (currentHealth > 0) {
                PlayHitAnimation();
            } else {
                var trans = transform;
                BossChestDieAnimation.Create(_entity.EntityManager, entityDiePrefab, trans.parent, trans.position);
            }
        }
    }
}