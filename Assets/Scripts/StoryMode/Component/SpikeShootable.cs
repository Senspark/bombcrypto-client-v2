using Engine.Entities;
using Engine.Strategy.Provider;

using UnityEngine;

namespace Engine.Components {
    public class SpikeShootable : DistanceShootable {
        public Enemy Owner { set; private get; }

        public SpikeShootable(Entity entity, Updater updater) : base(entity, updater) {
        }
        
        public void DoShoot() {
            var spikes = new Spike[4];
            for (var i = 0; i < spikes.Length; i++) {
                spikes[i] = GenerateSpike();
            }
            spikes[0].ForceMove(Vector2.up);
            spikes[1].ForceMove(Vector2.down);
            spikes[2].ForceMove(Vector2.left);
            spikes[3].ForceMove(Vector2.right);
        }

        private Spike GenerateSpike() {
            var provider = new PoolableProvider("Prefabs/Entities/Spike");
            var spike = (Spike) provider.CreateInstance(Entity.EntityManager);
            spike.transform.SetParent(Entity.EntityManager.View.transform, false);
            Entity.EntityManager.AddEntity(spike);
            spike.Init(Owner);
            var body = spike.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            spike.transform.localPosition = Entity.transform.localPosition;
            return spike;
        }
    }
}