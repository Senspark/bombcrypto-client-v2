using System;

using Engine.Entities;

using UnityEngine;

namespace Engine.Components {
    public class AnimationEvent : MonoBehaviour {
        
        [SerializeField]
        private Entity entity;

        private Shootable shootable;
        private EnemySpawnable spawnable;
        private SpikeShootable spikeShootable;
        private ExtraShootable extraShootable;

        private void Start() {
            shootable = entity.GetEntityComponent<Shootable>();
            spawnable = entity.GetEntityComponent<EnemySpawnable>();
            spikeShootable = entity.GetEntityComponent<SpikeShootable>();
            extraShootable = entity.GetEntityComponent<ExtraShootable>();
        }

        public void OnShootEvent() {
            shootable?.DoShoot();
        }
        
        public void OnSpawnEvent() {
            spawnable?.DoSpawn();
        }

        public void OnDistanceShootEvent() {
            spikeShootable?.DoShoot();
            extraShootable?.DoShoot();
        }
    }
}
