using Animation;

using Engine.Entities;

using UnityEngine;

namespace Engine.Components {
    public class DistanceShootable : EntityComponentV2 {
        public int DistanceToShoot { set; private get; } = 3;
        public int TimeShootingInterval { set; private get; } = 1;
        private float TimeProcess { set; get; } = 0;

        public bool Enable { set; private get; } = false;

        protected Entity Entity;
        
        public DistanceShootable(Entity entity, Updater updater) {
            Entity = entity;
            updater
                .OnBegin(()=> TimeProcess = TimeShootingInterval)
                .OnUpdate(OnProcess);
        }

        private void OnProcess(float delta) {
            if (!Entity.IsAlive) {
                return;
            }

            if (!Enable) {
                return;
            }

            if (TimeProcess < TimeShootingInterval) {
                TimeProcess += delta;
                return;
            }
            var player = Entity.EntityManager.PlayerManager.GetPlayer();
            if (player != null && player.IsAlive) {
                var mapManager = Entity.EntityManager.MapManager;
                var tileLocation = mapManager.GetTileLocation(Entity.transform.localPosition);
                var playerLocation = mapManager.GetTileLocation(player.GetPosition());

                var dx = tileLocation.x - playerLocation.x;
                var dy = tileLocation.y - playerLocation.y;

                if ((Mathf.Abs(dy) <= DistanceToShoot) &&
                    (Mathf.Abs(dx) <= DistanceToShoot)) {
                    Shoot();
                    TimeProcess = 0;
                }
            }
        }

        protected virtual void Shoot() {
            if (!Entity.IsAlive) {
                return;
            }
            Entity.GetComponent<EnemyAnimator>().PlayDistanceShoot();
        }
    }
}