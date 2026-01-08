using Animation;

using Engine.Components;

using UnityEngine;

using Engine.Entities;

namespace StoryMode.Entities {
    public class BossDie : Engine.Entities.Entity {
        [SerializeField]
        private EnemyAnimator enemyAnimator;

        [SerializeField]
        private Transform bodyTransform;

        private void Awake() {
            AddEntityComponent<Updater>(new Updater());
        }
        
        private void SetBossScale(EnemyType enemyType) {
            var scale = Boss.GetBossScale(enemyType);
            bodyTransform.localScale = new Vector3(scale, scale, scale);

            var offsetY = Boss.GetBossOffsetY(enemyType);
            var position = bodyTransform.localPosition;
            position.y = offsetY;
            bodyTransform.localPosition = position;
        }

        public void PlayDie(EnemyType type) {
            if (enemyAnimator == null) {
                return;
            }
            SetBossScale(type);
            enemyAnimator.SetEnemyType(type);
            enemyAnimator.PlayDie(() => { Kill(true); });
        }
    }
}