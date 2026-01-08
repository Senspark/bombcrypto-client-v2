using App;

using Engine.Entities;
using Engine.Manager;

using UnityEngine;

namespace Engine.Collision {
    public class PlayerBombListener : ICollisionListener {
        public void OnCollisionEntered(Entity entity, Entity otherEntity, Vector2 position, IEntityManager manager) { }

        public void OnCollisionExited(Entity entity, Entity otherEntity, IEntityManager manager) {
            if (manager.LevelManager.GameMode != GameModeType.StoryMode &&
                manager.LevelManager.GameMode != GameModeType.PvpMode) {
                return;
            }

            if (otherEntity is Bomb) {
                if (entity is Player) {
                    var player = entity as Player;
                    player.StuckWithBomb = false;
                    if (!player.WalkThrough.ThroughBomb) {
                        player.HadOutOfBomb = true;
                        otherEntity.GetComponent<Bomb>()?.CheckOnBomb();
                    }
                } else if (entity is BasicEnemy) {
                    var bomb = otherEntity as Bomb;
                    if (bomb.IsEnemy) {
                        return;
                    }

                    var enemy = (BasicEnemy) entity;
                    if (enemy.WalkThrough is {ThroughBomb: false}) {
                        enemy.HadOutOfBomb = true;
                        otherEntity.GetComponent<Bomb>()?.CheckOnBomb();
                    }
                }
            }
        }
    }
}