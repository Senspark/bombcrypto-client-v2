using Engine.Entities;

using StoryMode.Entities;

using UnityEngine;

namespace Engine.Components {
    public class BossDropper : KillTrigger {
        [SerializeField]
        private BossDie prefabBoss;

        [SerializeField]
        private BossDie prefabExplode;

        public override void Trigger() {
            CreateAnimationExplode();
        }

        private void CreateAnimationExplode() {
            var entity = GetComponent<Entity>();
            var parent = transform.parent;
            var ghost = Instantiate(prefabBoss, parent);
            var explode = Instantiate(prefabExplode, parent);

            var position = transform.localPosition;
            explode.transform.localPosition = position;
            ghost.transform.localPosition = position;
            var boss = GetComponent<Boss>();
            ghost.PlayDie(boss.EnemyType);
            entity.EntityManager.AddEntity(ghost);
            entity.EntityManager.AddEntity(explode);
        }
    }
}