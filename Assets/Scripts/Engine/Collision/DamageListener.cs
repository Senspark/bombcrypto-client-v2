using System.Collections;
using System.Collections.Generic;

using App;

using Engine.Components;
using Engine.Entities;
using Engine.Manager;
using UnityEngine;

namespace Engine.Collision
{
    public class DamageListener : ICollisionListener
    {
        public void OnCollisionEntered(Entity entity, Entity otherEntity, Vector2 position, IEntityManager manager)
        {
            if (manager.LevelManager.GameMode != GameModeType.StoryMode) {
                return;
            }

            var dealer = entity.GetComponent<DamageDealer>();
            var receiver = otherEntity.GetComponent<DamageReceiver>();
            if (dealer == null || receiver == null)
            {
                return;
            }

            // enemy not kill enemy
            if (entity is Enemy && otherEntity is Enemy)
            {
                return;
            }

            // spike not kill enemy
            if (entity is Spike && otherEntity is Enemy) {
                return;
            }

            receiver.TakeDamage(entity);
        }

        public void OnCollisionExited(Entity entity1, Entity entity2, IEntityManager manager)
        {

        }


    }
}
