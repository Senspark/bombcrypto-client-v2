using UnityEngine;

using Engine.Manager;
using Engine.Entities;

namespace Engine.Collision
{
    public interface ICollisionListener
    {
        void OnCollisionEntered(Entity entity, Entity otherEntity, Vector2 position, IEntityManager manager);
        void OnCollisionExited(Entity entity, Entity otherEntity, IEntityManager manager);
    }
}