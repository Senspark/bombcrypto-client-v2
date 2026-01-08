using App;

using Engine.Components;
using Engine.Entities;
using Engine.Manager;

using UnityEngine;

namespace Engine.Collision {
    public class EncounterListener : ICollisionListener {
        public void OnCollisionEntered(Entity entity, Entity otherEntity, Vector2 position, IEntityManager manager) {
            if (entity is Player) {
                var walkThrough = entity.GetEntityComponent<WalkThrough>();
                if (walkThrough != null) {
                    walkThrough.HitObstacle(otherEntity);
                } else {
                    var movable = entity.GetEntityComponent<Movable>();
                    if (movable != null) {
                        movable.ForceStop();
                    }
                }
            }

            if (manager.LevelManager.GameMode != GameModeType.StoryMode &&
                manager.LevelManager.GameMode != GameModeType.PvpMode) {
                return;
            }

            if (entity is Enemy && otherEntity is Enemy) {
                return;
            }

            if (entity is Player && otherEntity is Enemy) {
                if (entity.Immortal) {
                    return;
                }
            }

            if (entity is Enemy {EnemyType: EnemyType.BabyPumpkin} && otherEntity is Player) {
                var enemy = (Enemy) entity;
                if (!enemy.IsFallingDown) {
                    entity.Kill(true);
                }
                return;
            }

            if (entity is Enemy || entity is Player) {
                if (otherEntity is Item) {
                    return;
                }

                if (otherEntity is Fire) {
                    return;
                }

                var walkThrough = entity.GetEntityComponent<WalkThrough>();
                if (walkThrough != null) {
                    walkThrough.HitObstacle(otherEntity);
                } else {
                    var movable = entity.GetEntityComponent<Movable>();
                    if (movable != null) {
                        movable.ForceStop();
                    }
                }
            }

            if (entity is BossCollider bossCollider && otherEntity is BasicEnemy {
                    EnemyType: EnemyType.BeerEaiter
                } basicEnemy) {
                bossCollider.boss.BoostSpeed(4, 3);
                basicEnemy.ForceKill();
            }

            if (entity is BasicEnemy {EnemyType: EnemyType.BeerEaiter} beerEater &&
                otherEntity is BossCollider collider) {
                collider.boss.BoostSpeed(4, 3);
                beerEater.ForceKill();
            }

            if (entity is BossCollider && otherEntity is Bomb) {
                var bomb = otherEntity as Bomb;
                if (!bomb.IsEnemy) {
                    bomb.DestroyMe();
                }
            }

            if (entity is Bomb) {
                if (otherEntity is Border) {
                    var movable = entity.GetEntityComponent<Movable>();
                    if (movable != null) {
                        movable.ForceStop();
                    }
                } else if (otherEntity is Bomb) {
                    var bomb1 = entity as Bomb;
                    var bomb2 = otherEntity as Bomb;
                    if (bomb1.GroupId != "" && bomb2.GroupId != "") {
                        if (bomb1.GroupId == bomb2.GroupId) {
                            var movable1 = entity.GetEntityComponent<Movable>();
                            var movable2 = otherEntity.GetEntityComponent<Movable>();
                            if (movable1.VelocityPhysics == Vector2.zero) {
                                movable2.ForceStop();
                            }
                        }
                    }
                } else if (otherEntity is Player) {
                    var bomb = (Bomb) entity;
                    if (bomb.IsEnemy && !bomb.IsThroughHero) {
                        bomb.StartExplode(bomb.transform.localPosition);
                    }
                }
            }

            if (entity is Spike spike) {
                if (otherEntity is Spike otherSpike) {
                    if (spike.Owner == otherSpike.Owner) {
                        return;
                    }
                } else if (spike.Owner == otherEntity) {
                    return;
                } else if (otherEntity is Player) // đụng player không biến mất mà sẽ mất sau khi take damage
                {
                    return;
                }

                // Biến mất khi đụng bất kỳ ngoài các trường hợp trên 
                spike.Kill(false);
            }
        }

        public void OnCollisionExited(Entity entity, Entity otherEntity, IEntityManager manager) {
        }
    }
}