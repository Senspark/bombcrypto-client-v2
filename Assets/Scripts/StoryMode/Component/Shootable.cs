using System;
using System.Collections.Generic;

using Animation;

using App;

using Engine.Entities;
using Engine.Manager;
using Engine.Strategy.Provider;
using Engine.Strategy.Weapon;

using Senspark;

using UnityEngine;

using Vector2 = UnityEngine.Vector2;

namespace Engine.Components {
    using ShootLocationCallback = Func<Vector2>;

    public class Shootable : EntityComponentV2 {
        private ShootLocationCallback _shootLocationCallback;
        public Vector2 ShootLocation => _shootLocationCallback?.Invoke() ?? Vector2.zero;
        public IWeapon Weapon { get; private set; }

        public int BossId { set; get; }
        public float Damage { set; get; } = 1;
        public int BombSkin { set; get; }
        public int ExplosionSkin { set; get; }
        public int ExplosionLength { set; get; } = 4;
        private float TimeToExplode { set; get; } = 3.0f;

        private Vector2 _vectorShoot;

        public float OptionShoot { set; get; } = 1;
        // 1: skill 1: 1 thẳng (3 bomb) - skill 2: 2 xéo + 1 thẳng. (4s)
        // 2: skill 1: 2 xéo + 1 dọc + 2 ngang (5 bomb) (10s, hp<30% - 7s) - skill 2: 4 enemies (skin -1). (20s) - hp < 30% => speed x 2
        // 3: skill 1: 1 ememy (skin -1) khi bị mất máu. - skill 2: 4 bomb 4 hướng dọc,ngang (10s)
        // 4: skill 1: speed x 4 when immortal khi bị mất máu. - skill 2: tất bomb -> enemy (skin - 3).
        // 5: skill 1: nổ như bomb
        // 6: skill 1: thả 10 bom từ trên xuống
        // 7: skill 1: thả 10 beer eater

        private bool _turnSkill = true;
        private int _bombId = 0;

        private Boss _boss;
        private FollowMove _followMove;
        private Sparkable _sparkable;
        private EnemyAnimator _enemyAnimator;

        public bool Active { private get; set; } = true;

        private PoolableProvider _providerBomb;

        public IEntityManager EntityManager => _boss.EntityManager;
        
        public Shootable(Boss boss, FollowMove followMove, Sparkable sparkable) {
            _boss = boss;
            _followMove = followMove;
            _sparkable = sparkable;
            _enemyAnimator = _boss.GetComponent<EnemyAnimator>();
            
            _providerBomb = new PoolableProvider("Prefabs/Entities/BombCount");
            _boss.GetEntityComponent<Updater>()
                .OnUpdate(delta => { Weapon?.Update(delta); });
        }

        public void ChangeCooldown(float cooldown) {
            Weapon.ChangeCooldown(cooldown);
        }

        public bool Shoot() {
            if (!Active) {
                return false;
            }

            if (!_boss.IsAlive) {
                return false;
            }

            var player = EntityManager.PlayerManager.GetPlayer();
            if (player == null || !player.IsAlive) {
                return false;
            }
            _vectorShoot = ChangDirectionToShootPlayer();
            if (_boss.EnemyType == EnemyType.DeceptionsHeadQuater) {
                PrepareToFire();
            } else if (_boss.EnemyType == EnemyType.BigTank) {
                BigTankShoot();
            } else {
                _enemyAnimator.PlayShoot(DoShoot);
            }
            return true;
        }

        private async void PrepareToFire() {
            _boss.SetStatus(EnemyStatus.Shooting);
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.RobotShoot);
            _enemyAnimator.PlayPrepare(() => { _enemyAnimator.PlayShoot(DoShoot); });
        }

        public void DoShoot() {
            switch (OptionShoot) {
                case 1:
                    ShootOne();
                    break;
                case 2:
                    ShootTwo();
                    break;
                case 3:
                    ShootThree();
                    break;
                case 4:
                    ShootFour();
                    break;
                case 5:
                    ShootFive();
                    break;
                case 6:
                    ShootSix();
                    break;
                case 7:
                    ShootSeven();
                    break;
                case 9:
                    ShootNine();
                    break;
            }
        }

        private Bomb GenerateBomb(int bombId = 0) {
            var bomb = Weapon.Shoot() as Bomb;
            if (!bomb) {
                return bomb;
            }
            var body = bomb.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            bomb.SetOrderLayer(_boss.EnemyType == EnemyType.BigTank ? 1001 : 500);
            var bossId = new HeroId(BossId, HeroAccountType.Trial);
            bomb.Init(bombId, bossId, BombSkin, ExplosionSkin, Damage, 0, ExplosionLength, TimeToExplode, false,
                OnAfterBombExplosion, true, _boss.EnemyType == EnemyType.LordPirates);
            bomb.SetCountDownEnable(false);
            return bomb;
        }

        private Fire GenerateFire() {
            var fire = (Fire) EntityManager.MapManager.CreateEntity(EntityType.Fire);
            fire.Type = EntityType.Fire;
            fire.transform.SetParent(EntityManager.View.transform, false);
            EntityManager.AddEntity(fire);

            fire.Init(BossId, Damage);
            return fire;
        }

        private Bomb GenerateBombCount() {
            var timeToExplode = UnityEngine.Random.Range(4f, 6f);
            var bomb = (Bomb) _providerBomb.CreateInstance(EntityManager);
            bomb.transform.SetParent(EntityManager.View.transform, false);
            EntityManager.AddEntity(bomb);

            var body = bomb.GetComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            var isThrough = _boss.EnemyType is EnemyType.LordPirates or EnemyType.JesterKing;
            var bossId = new HeroId(BossId, HeroAccountType.Trial);
            bomb.Init(0, bossId, BombSkin, ExplosionSkin, Damage, 0, ExplosionLength, timeToExplode, false,
                OnAfterBombExplosion, true, isThrough);
            bomb.SetCountDownEnable(false);
            return bomb;
        }

        private async void BigTankShoot() {
            if (_turnSkill) {
                for (var i = 0; i < 2; i++) {
                    _followMove.ForceStopForShoot();
                    _enemyAnimator.PlayShoot(DoShoot);
                    await App.WebGLTaskDelay.Instance.Delay(500);
                }
            } else {
                ShootOneSkillTwo();
            }
            _turnSkill = !_turnSkill;
        }

        private void ShootOne() {
            _bombId = 0;
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.TankShoot, null, true);
            var bomb = GenerateBomb(0);
            bomb.GroupId = "BigTank_" + _bombId;
            bomb.ForceMove(_vectorShoot);
        }

        private void ShootOneSkillTwo() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.TankShoot);
            _bombId = 1;
            var bombs = new Bomb[2]; // new Bomb[3];
            for (var i = 0; i < bombs.Length; i++) {
                bombs[i] = GenerateBomb(1);
            }
            bombs[1].ForceMove(_vectorShoot);
            if (_vectorShoot.x != 0) {
                bombs[0].ForceMove(_vectorShoot + Vector2.up);
                bombs[1].ForceMove(_vectorShoot + Vector2.down);
            } else {
                bombs[0].ForceMove(_vectorShoot + Vector2.left);
                bombs[1].ForceMove(_vectorShoot + Vector2.right);
            }
        }

        private void ShootTwo() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.KingShoot);
            var bombs = new Bomb[3]; // new Bomb[5];
            for (var i = 0; i < bombs.Length; i++) {
                bombs[i] = GenerateBomb();
            }
            bombs[0].ForceMove(_vectorShoot);
            if (_vectorShoot.x != 0) {
                bombs[1].ForceMove(_vectorShoot + Vector2.up);
                bombs[2].ForceMove(_vectorShoot + Vector2.down);
                // bombs[3].ForceMove(Vector2.up);
                // bombs[4].ForceMove(Vector2.down);
            } else {
                bombs[1].ForceMove(_vectorShoot + Vector2.left);
                bombs[2].ForceMove(_vectorShoot + Vector2.right);
                // bombs[3].ForceMove(Vector2.left);
                // bombs[4].ForceMove(Vector2.right);
            }
        }

        private void ShootThree() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.MonsterShoot);
            var bombs = new Bomb[4];
            for (var i = 0; i < bombs.Length; i++) {
                bombs[i] = GenerateBomb();
            }
            bombs[0].ForceMove(Vector2.up);
            bombs[1].ForceMove(Vector2.down);
            bombs[2].ForceMove(Vector2.left);
            bombs[3].ForceMove(Vector2.right);
        }

        private void ShootFour() { }

        private async void ShootFive() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.BombExplode);
            _sparkable.Fire(ExplosionLength);

            await App.WebGLTaskDelay.Instance.Delay(1000);
            _boss.OnAfterShoot();
            _boss.SetStatus(EnemyStatus.Moving);
        }

        private void ShootSix() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.PirateShoot);
            const int numBombs = 20;
            var locations = EntityManager.MapManager.GetRandomEmptyLocations(numBombs, 5);

            var bombs = new Bomb[numBombs];
            for (var i = 0; i < bombs.Length && i < locations.Count; i++) {
                bombs[i] = GenerateBomb();

                var targetLocation = locations[i];
                var startLocation = locations[i];
                startLocation.y += 5;
                var position = EntityManager.MapManager.GetTilePosition(startLocation);
                bombs[i].transform.localPosition = position;

                bombs[i].GetEntityComponent<BombMovable>().SetTargetLocation(targetLocation);
                bombs[i].ForceMove(Vector2.down * 2);
            }
        }

        private async void ShootSeven() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.ChefShoot);
            const int numBombs = 20;
            var locations = EntityManager.MapManager.GetRandomEmptyLocations(numBombs);

            var fires = new Fire[numBombs];
            for (var i = 0; i < fires.Length && i < locations.Count; i++) {
                fires[i] = GenerateFire();
                var position = EntityManager.MapManager.GetTilePosition(locations[i]);
                fires[i].transform.localPosition = position;
            }

            await App.WebGLTaskDelay.Instance.Delay(1000);
            _boss.OnAfterShoot();
            _boss.SetStatus(EnemyStatus.Moving);
        }

        private void ShootNine() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.ChefSpawn);
            var numBombs = UnityEngine.Random.Range(3, 8);
            var locations = EntityManager.MapManager.GetRandomEmptyLocations(numBombs, 5);
            DropCounterBomb(numBombs, locations);
        }

        public void ShootExtraAround(Vector2Int location, int numBombs) {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.ChefSpawn);
            _followMove.ForceStopForShoot();
            var locations = EntityManager.MapManager.GetRandomEmptyAround(location, numBombs, 1, 3, 5);
            if (locations.Count == 0) {
                OnAfterBombExplosion(null);
            } else {
                DropCounterBomb(numBombs, locations);
            }
        }

        private void DropCounterBomb(int numBombs, IReadOnlyList<Vector2Int> locations) {
            var bombs = new Bomb[numBombs];
            for (var i = 0; i < bombs.Length && i < locations.Count; i++) {
                bombs[i] = GenerateBombCount();

                var targetLocation = locations[i];
                var startLocation = locations[i];
                startLocation.y += 5;
                var position = EntityManager.MapManager.GetTilePosition(startLocation);
                bombs[i].transform.localPosition = position;

                bombs[i].GetEntityComponent<BombMovable>().SetTargetLocation(targetLocation);
                bombs[i].ForceMove(Vector2.down * 2);
            }
        }

        private void OnAfterBombExplosion(Entity entity) {
            if (entity != null) {
                var bomb = (Bomb) entity;
                if (bomb.BombId != _bombId) {
                    return;
                }
            }
            _boss.OnAfterShoot();
            _boss.SetStatus(EnemyStatus.Moving);
        }

        private Vector2 ChangDirectionToShootPlayer() {
            Weapon.StopLoader();

            var myPosition = _boss.transform.localPosition;
            var playerPosition = EntityManager.PlayerManager.Players[0].GetPosition();

            var diff = playerPosition - myPosition;
            var direction = Vector2.zero;
            if (Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) {
                direction.x = diff.x > 0 ? 1 : -1;
            } else {
                direction.y = diff.y > 0 ? 1 : -1;
            }
            _followMove.ForceChangeDirection(direction);
            return direction;
        }

        public Shootable SetWeapon(IWeapon weapon) {
            Weapon = weapon;
            return this;
        }

        public Shootable SetShootLocation(ShootLocationCallback callback) {
            _shootLocationCallback = callback;
            return this;
        }
    }
}