using System.Collections.Generic;
using System.Linq;

using Animation;

using App;

using Engine.Components;

using UnityEngine;

using Engine.Strategy.Reloader;
using Engine.Strategy.Shooter;
using Engine.Strategy.Spawner;
using Engine.Strategy.Weapon;

using Senspark;

namespace Engine.Entities {
    public class Boss : Enemy {
        [SerializeField]
        private GameObject sleepAnimation;

        [SerializeField]
        private Transform bodyTransform;

        [SerializeField]
        private BossCollider bossCollider;

        public BossCollider BossCollider {
            get => bossCollider;
        }

        private Rigidbody2D _myBody;
        private Rigidbody2D _secondBody;
        private EnemyAnimator _enemyAnimator;

        private EnemySpawnable _spawnable;
        private FollowMove _followMove;

        private const float SleepTime = 60;
        private float _sleepElapse;

        protected override void Awake() {
            _myBody = GetComponent<Rigidbody2D>();
            _secondBody = BossCollider.GetComponent<Rigidbody2D>();
            _enemyAnimator = GetComponent<EnemyAnimator>();
            _followMove = GetComponent<FollowMove>();

            // Add Updater
            var updater = new Updater()
                .OnBegin(Init)
                .OnUpdate(OnProcess);
            AddEntityComponent<Updater>(updater);
            
            // Add Health
            Health = new Health(this, healthBar);
            AddEntityComponent<Health>(Health);

            // Add EnemySpawnable
            _spawnable = new EnemySpawnable(this);
            AddEntityComponent<EnemySpawnable>(_spawnable);
            
            // Add Sparkable
            var sparkable = new Sparkable(this);
            AddEntityComponent<Sparkable>(sparkable);
            
            // Add ExtraShootable
            var extraShootable = new ExtraShootable(this, updater, Shootable) {
                DistanceToShoot = 3,
                TimeShootingInterval = 10
            };
            AddEntityComponent<ExtraShootable>(extraShootable);
            
            // Add Shootable
            Shootable = new Shootable(this, _followMove, sparkable);
            AddEntityComponent<Shootable>(Shootable);
            
            base.Awake();
        }

        private void OnProcess(float delta) {
            if (Status == EnemyStatus.Sleeping) {
                UpdateSleepElapse(delta);
                if (_sleepElapse <= 0) {
                    StopSleep();
                }
                return;
            }

            _secondBody.position = _myBody.position;
        }

        private void UpdateSleepElapse(float minus) {
            _sleepElapse -= minus;
            EntityManager.LevelManager.OnSleepBossCountDown(Mathf.CeilToInt(_sleepElapse));
        }

        private void Wakeup() {
            EntityManager.LevelManager.OnBossWakeup();
        }

        private void Init() {
            if (Receiver) {
                Receiver.SetOnTakeDamage(TakeDamage);
            }
            SetupWeapon();
        }

        public void StartSleep() {
            Status = EnemyStatus.Sleeping;
            _sleepElapse = SleepTime;
            _enemyAnimator.PlaySleep();
            sleepAnimation.SetActive(true);
            ActiveAction(false);
        }

        private void StopSleep() {
            Status = EnemyStatus.Moving;
            _enemyAnimator.StopSleep();
            ActiveAction(true);
            sleepAnimation.SetActive(false);
            UpdateSleepElapse(_sleepElapse);
            Wakeup();
        }

        private void ActiveAction(bool value) {
            Shootable.Active = value;
            _spawnable.Active = value;
            _followMove.Active = value;
        }

        protected override void TakeDamage(IEnemyDetails enemy) {
            StopSleep();
            Health.SetCurrentHealth(enemy.Hp);
            if (enemy.Hp > 0) {
                AnimateTakeDamage();
            } else {
                ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.BossDestroy);
                Kill(true);
                EntityManager.LevelManager.EarnGoldFromEnemy(enemy.GoldReceive, transform.position);
            }
        }

        private void AnimateTakeDamage() {
            var follow = GetComponent<FollowMove>();
            var currentAction = _enemyAnimator.CurrentAction;
            if (currentAction is AnimationAction.Preparing or AnimationAction.Shoot) {
                UpdateAbilityWithHealth();
                return;
            }

            Movable.ForceStop();
            Status = EnemyStatus.TakeDamage;

            var audioDamage = GetAudioTakeDamage(EnemyType);
            if (audioDamage != Audio.None) {
                ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(audioDamage);
            }

            _enemyAnimator.PlayTakeDamage(() => {
                _enemyAnimator.PlayMoving(FaceDirection.Down);
                Status = EnemyStatus.Moving;
                if (IsAlive) {
                    UpdateAbilityWithHealth();
                }
            });
        }

        private void UpdateAbilityWithHealth() {
            var bossSkill = EntityManager.EnemyManager.GetBossSkillDetails();

            if (EnemyType == EnemyType.BigRockyLord) {
                SpawnOneEnemy();
            } else if (EnemyType == EnemyType.BeetlesKing) {
                ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.MosquitoQuickRun);
                BoostSpeedMe(bossSkill);
            } else if (EnemyType == EnemyType.LordPirates) {
                BoostSpeedBlader();
            }

            if (EnemyType != EnemyType.CandyKing) {
                return;
            }
            var percent = Health.GetPercentHealth();
            if (!(percent < bossSkill.PercentHealthBoost)) {
                return;
            }
            Movable.Speed = bossSkill.SpeedBoost;
            Shootable.Weapon.ChangeCooldown(bossSkill.CoolDownShootBoost);
        }

        private async void SpawnOneEnemy() {
            if (Status == EnemyStatus.Spawning) {
                return;
            }
            Status = EnemyStatus.Spawning;
            _spawnable.Weapon.Spawn();
            await App.WebGLTaskDelay.Instance.Delay((int) (2000));
            Status = EnemyStatus.Moving;
        }

        private void BoostSpeedMe(IBossSkillDetails bossSkill) {
            BoostSpeed(bossSkill.SpeedBoost, bossSkill.SpeedBoostTime);
        }

        private void BoostSpeedBlader() {
            var enemies = EntityManager.FindEntities<Enemy>();
            foreach (var enemy in enemies.Where(enemy => enemy.IsAlive)
                         .Where(enemy => enemy.EnemyType == EnemyType.BladerPirates)) {
                enemy.BoostSpeed(Movable.Speed * 3, 3);
            }
        }

        private void SetupWeapon() {
            if (Shootable != null) {
                var shootWeapon = new ShootWeapon(WeaponType.Bomb,
                    new AutoReloader(0),
                    new BombShooter(),
                    Shootable);

                Shootable.SetWeapon(shootWeapon);
                Shootable.SetShootLocation(GetShootPosition);

                Shootable.Weapon.StartLoader();
            }

            if (_spawnable == null) {
                return;
            }
            var spawnWeapon = new EnemyWeapon(WeaponType.Enemy,
                new AutoReloader(0),
                new EnemySpawner(this),
                _spawnable);
            _spawnable.SetWeapon(spawnWeapon);
            _spawnable.SetSpawnLocation(GetSpawnLocation);

            _spawnable.Weapon.StopLoader();
        }

        public void OnAfterShoot() {
            if (_spawnable.Weapon.IsLoaderActive()) {
                _spawnable.Weapon.StartLoader();
            } else {
                Shootable.Weapon.StartLoader();
            }
        }

        public void OnAfterSpawn() {
            if (Shootable.Weapon.IsLoaderActive()) {
                Shootable.Weapon.StartLoader();
            } else {
                _spawnable.Weapon.StartLoader();
            }
        }

        private Vector2 GetShootPosition() {
            // float offset = 1.5f;
            // Vector3 offsetLocation = Vector3.zero;
            // var direction = Movable.Velocity;
            // if (direction.x > 0) offsetLocation.x += offset;
            // else if (direction.x < 0) offsetLocation.x -= offset;
            // else if (direction.y > 0) offsetLocation.y += offset;
            // else if (direction.y < 0) offsetLocation.y -= offset;

            // var tileLocation = EntityManager.MapManager.GetTileLocation(transform.localPosition);
            // return EntityManager.MapManager.GetTilePosition(tileLocation) + offsetLocation;

            return transform.localPosition;
        }

        private Vector2Int GetSpawnLocation() {
            return EntityManager.MapManager.GetTileLocation(transform.localPosition);
        }

        public void SetBossScale(EnemyType enemyType) {
            var scale = GetBossScale(enemyType);
            bodyTransform.localScale = new Vector3(scale, scale, scale);

            var offsetY = GetBossOffsetY(enemyType);
            var position = bodyTransform.localPosition;
            position.y = offsetY;
            bodyTransform.localPosition = position;

            var circleCollider = BossCollider.GetComponent<CircleCollider2D>();
            circleCollider.radius = GetBossColliderRadius(enemyType);
        }

        public static float GetBossScale(EnemyType enemyType) {
            var scaleList = new Dictionary<EnemyType, float>() {
                {EnemyType.BigTank, 1},
                {EnemyType.CandyKing, 0.95f},
                {EnemyType.BigRockyLord, 1.4f},
                {EnemyType.BeetlesKing, 1f},
                {EnemyType.DeceptionsHeadQuater, 0.9f},
                {EnemyType.LordPirates, 1f},
                {EnemyType.DumplingsMaster, 1.4f},
                {EnemyType.PumpkinLord, 1.4f},
                {EnemyType.JesterKing, 1.4f}
            };
            return scaleList[enemyType];
        }
        
        private static float GetBossColliderRadius(EnemyType enemyType) {
            var scaleList = new Dictionary<EnemyType, float>() {
                {EnemyType.BigTank, 1.4f},
                {EnemyType.CandyKing, 1.2f},
                {EnemyType.BigRockyLord, 1.3f},
                {EnemyType.BeetlesKing, 1.2f},
                {EnemyType.DeceptionsHeadQuater, 1.4f},
                {EnemyType.LordPirates, 1.4f},
                {EnemyType.DumplingsMaster, 1.2f},
                {EnemyType.PumpkinLord, 1.2f},
                {EnemyType.JesterKing, 1.0f},
            };
            return scaleList[enemyType];
        }
        

        public static float GetBossOffsetY(EnemyType enemyType) {
            var scaleList = new Dictionary<EnemyType, float>() {
                {EnemyType.BigTank, 0.05f},
                {EnemyType.CandyKing, 0.05f},
                {EnemyType.BigRockyLord, 0.05f},
                {EnemyType.BeetlesKing, 0.05f},
                {EnemyType.DeceptionsHeadQuater, 0.1f},
                {EnemyType.LordPirates, 0.2f},
                {EnemyType.DumplingsMaster, 0f},
                {EnemyType.PumpkinLord, 0f},
                {EnemyType.JesterKing, 0f}
            };
            return scaleList[enemyType];
        }

        private Audio GetAudioTakeDamage(EnemyType enemyType) {
            switch (enemyType) {
                case EnemyType.BigRockyLord:
                    return Audio.MonsterTakeDamage;
                case EnemyType.DeceptionsHeadQuater:
                    return Audio.RobotTakeDamage;
                case EnemyType.LordPirates:
                    return Audio.PirateTakeDamage;
                case EnemyType.DumplingsMaster:
                    return Audio.ChefTakeDamage;
                case EnemyType.PumpkinLord:
                    return Audio.ChefTakeDamage;
                case EnemyType.JesterKing:
                    return Audio.ChefTakeDamage;
            }
            return Audio.None;
        }

        public override void SetActive(bool value) {
            base.SetActive(value);
            ActiveAction(value);
        }
    }
}