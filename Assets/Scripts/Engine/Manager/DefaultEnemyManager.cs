using System.Collections.Generic;
using System.Linq;

using App;

using Cysharp.Threading.Tasks;

using Engine.Components;
using Engine.Entities;

using UnityEngine;

namespace Engine.Manager {
    public class DefaultEnemyManager : IEnemyManager {
        private readonly IEntityManager EntityManager;

        private readonly IBossSkillDetails _bossSkillDetails;

        private readonly List<Enemy> _enemies = new List<Enemy>();

        public int Count {
            get {
                return _enemies.Count(iter => iter.IsAlive);
            }
        }

        public DefaultEnemyManager(
            IEntityManager entityManager,
            IBossSkillDetails bossSkillDetails
        ) {
            EntityManager = entityManager;
            _bossSkillDetails = bossSkillDetails;
        }

        public void RemoveEnemy(Enemy enemy) {
            _enemies.Remove(enemy);
            EntityManager.LevelManager.OnRemoveEnemy(enemy.EnemyType);
        }

        public Enemy GetEnemyById(int id) {
            for (var i = 0; i < _enemies.Count; i++) {
                if (_enemies[i].Id == id) {
                    return _enemies[i];
                }
            }
            return null;
        }

        public IBossSkillDetails GetBossSkillDetails() {
            return _bossSkillDetails;
        }

        public async UniTask<Enemy> CreateEnemy(IEnemyDetails enemyData, Vector2Int location) {
            var enemyType = (EnemyType)(enemyData.Skin - 1);
            var isBoss = IsBoss(enemyType);

            if (enemyData.PosSpawn.x > 0 && enemyData.PosSpawn.y > 0) {
                // Nếu có PosSpawn thì force spawn theo pos mới
                location = enemyData.PosSpawn;
            } else 
            {
                //Nếu là boss thì vị trí của boss là giữa map
                if (isBoss) {
                    location = EntityManager.MapManager.TakeLocationsForBoss();
                }
            }
            var entityType = isBoss ? EntityType.Boss : EntityType.Enemy;
            var enemy = await EntityManager.LevelManager.CreateEntity(entityType, location,
                enemyData.Follow) as Enemy;

            if (!enemy) {
                return enemy;
            }
            var countdown = enemy.GetComponent<CountDown>();
            if (countdown) {
                countdown.EnemiesCountDownCallback = () => {
                    RemoveEnemy(enemy);
                    EntityManager.LevelManager.CheckEnemiesClear();
                };
            }

            SetEnemyProperties(enemy, enemyData);
            enemy.StartSpeed();

            _enemies.Add(enemy);
            EntityManager.LevelManager.OnAddEnemy(enemy.EnemyType);

            return enemy;
        }

        public void CreateEnemies(IEnemyDetails[] enemies) {
            var num = enemies.Length;
            if (num > 0) {
                var locations = EntityManager.MapManager.TakeEmptyLocations(num);
                for (var i = 0; i < locations.Count && i < enemies.Length; i++) {
                    CreateEnemy(enemies[i], locations[i]);
                }
            }
        }

        public void CreateEnemies(IEnemyDetails[] enemies, Vector2Int location) {
            var num = enemies.Length;
            if (num > 0) {
                for (var i = 0; i < num; i++) {
                    CreateEnemy(enemies[i], location);
                }
            }
        }

        private void SetEnemyProperties(Enemy enemy, IEnemyDetails enemyData) {
            enemy.EnemyType = (EnemyType)(enemyData.Skin - 1);
            enemy.Animator.SetEnemyType(enemy.EnemyType);

            if (enemy.dropper != null) {
                enemy.dropper.SetEnemySprite(enemy.EnemyType);
                switch (enemy.EnemyType) {
                    case EnemyType.AutoBots:
                        enemy.dropper.SetTurnIntoBomb(21);
                        break;
                    case EnemyType.BabyPumpkin:
                        enemy.SetAutoBlinkBeforeExplode(5, 3);
                        enemy.dropper.SetTurnIntoBomb(0, 0);
                        break;

                    /// Hủy chức năng tự động biến hình từ CraftyCat sang GhostCraftyCat trong 10s
                    // case EnemyType.CraftyCat:
                    //     enemy.SetAutoChangeToAnother(10, EnemyType.GhostCraftyCat);
                    //     break;
                }
            }

            enemy.Id = enemyData.Id;
            enemy.Movable.Speed = enemyData.Speed;
            enemy.Health.MaxHealth = enemyData.MaxHp;
            enemy.Health.SetCurrentHealth(enemyData.Hp);
            enemy.Damage = enemyData.Damage;

            enemy.Health.SetShowHealthBarWhenFull(true);

            var isBoss = IsBoss(enemy.EnemyType);
            if (isBoss) {
                enemy.WalkThrough.ThroughBrick = true;
                var boss = (Boss)enemy;
                boss.SetBossScale(enemy.EnemyType);
                EntityManager.AddEntity(boss.BossCollider);

                if (enemy.Shootable != null) {
                    enemy.Shootable.BossId = enemyData.Id;
                    enemy.Shootable.Damage = enemyData.Damage;
                    enemy.Shootable.BombSkin = GetBossBombSkin(enemy.EnemyType);
                    enemy.Shootable.ExplosionSkin = GetBossExplosionSkin(enemy.EnemyType);
                    enemy.Shootable.ExplosionLength = enemyData.BombRange;
                    enemy.Shootable.OptionShoot = GetOptionShoot(enemy.EnemyType);
                    enemy.Shootable.ChangeCooldown(_bossSkillDetails.CoolDownShoot);
                }

                var spawnable = enemy.Spawnable as EnemySpawnable;
                spawnable?.ChangeCooldown(_bossSkillDetails.CoolDownSpawn);

                var extraShootable = enemy.GetEntityComponent<ExtraShootable>();
                if (enemy.EnemyType == EnemyType.JesterKing) {
                    extraShootable.Enable = true;
                }

                boss.StartSleep();

            } else {
                enemy.WalkThrough.ThroughBrick = enemyData.ThroughBrick;
                enemy.Animator.StopSleep();
            }

            // BabyPumpkin will start to follow hero when distance <= 5 and never stop follow 
            if (enemy.EnemyType == EnemyType.BabyPumpkin) {
                var follow = enemy.GetComponent<FollowMove>();
                if (follow == null) {
                    follow = enemy.gameObject.AddComponent<FollowMove>();
                }
                follow.DistanceToStartFollow = 5;
                follow.DistanceToStopFollow = 1000000;
            }

            //// Tạm thời Design quyết định bỏ chức năng bắn gai ở con nhím....
            // // Hedgehog Crazy has Spike Shootable
            // var spikeShootable = enemy.GetEntityComponent<SpikeShootable>();
            // if (enemy.EnemyType == EnemyType.HedgehogCrazy) {
            //     spikeShootable.Owner = enemy;
            //     spikeShootable.Enable = true;
            // }
        }

        public static bool IsBoss(EnemyType enemyType) {
            var bossList = new List<EnemyType>() {
                EnemyType.BigTank,
                EnemyType.CandyKing,
                EnemyType.BigRockyLord,
                EnemyType.BeetlesKing,
                EnemyType.DeceptionsHeadQuater,
                EnemyType.LordPirates,
                EnemyType.DumplingsMaster,
                EnemyType.PumpkinLord,
                EnemyType.JesterKing
            };
            return (bossList.Contains((enemyType)));
        }

        private static int GetOptionShoot(EnemyType enemyType) {
            var shootList = new Dictionary<EnemyType, int>() {
                { EnemyType.BigTank, 1 },
                { EnemyType.CandyKing, 2 },
                { EnemyType.BigRockyLord, 3 },
                { EnemyType.BeetlesKing, 4 },
                { EnemyType.DeceptionsHeadQuater, 5 },
                { EnemyType.LordPirates, 6 },
                { EnemyType.DumplingsMaster, 7 },
                { EnemyType.PumpkinLord, 8 },
                { EnemyType.JesterKing, 9 }
            };
            return (shootList[enemyType]);
        }

        private static int GetBossBombSkin(EnemyType enemyType) {
            var skinBombList = new Dictionary<EnemyType, int>() {
                { EnemyType.BigTank, 22 },
                { EnemyType.CandyKing, 23 },
                { EnemyType.BigRockyLord, 24 },
                { EnemyType.BeetlesKing, 25 },
                { EnemyType.DeceptionsHeadQuater, 26 },
                { EnemyType.LordPirates, 27 },
                { EnemyType.DumplingsMaster, 28 },
                { EnemyType.PumpkinLord, 29 },
                { EnemyType.JesterKing, 29 }
            };
            return skinBombList[enemyType];
        }

        private static int GetBossExplosionSkin(EnemyType enemyType) {
            return 0;
        }
    }
}