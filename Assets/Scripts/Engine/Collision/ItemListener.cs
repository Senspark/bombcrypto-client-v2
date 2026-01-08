using Engine.Entities;
using Engine.Manager;

using PvpMode.Entities;

using UnityEngine;

namespace Engine.Collision {
    public class ItemListener : ICollisionListener {
        public void OnCollisionEntered(Entity entity, Entity otherEntity, Vector2 position, IEntityManager manager) {
            switch (entity) {
                case Door door: {
                    if (otherEntity is Player) {
                        door.PlayerEnter();
                    }
                    break;
                }
                case Item item: {
                    if (item.IsActive) {
                        if (otherEntity is Player player) {
                            player.RequestTakeItem(item);
                        }
                    }
                    break;
                }
            }
        }

        public void OnCollisionExited(Entity entity, Entity otherEntity, IEntityManager manager) {
        }
    }
}