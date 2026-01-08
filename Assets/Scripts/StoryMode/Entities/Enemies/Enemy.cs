using System.Collections.Generic;

using Animation;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Engine.Components;
using Engine.Strategy.CountDown;
using Engine.UI;

using Senspark;

using UnityEngine;

namespace Engine.Entities {
    public enum EnemyStatus {
        //Các status không thực hiện tìm đường
        Sleeping, // đang ngủ 60 giây ban đầu.
        Spawning, // đang animation spawn quái
        Shooting, // đang animation shooting
        Stuck,    // đang bị kẹt giữa các block
        Standing, // đang standing 3s khi đụng hero 
        TakeDamage, // đang animation takeDamage
        // Các status tìm đường hoặc thay đổi cách tìm đường
        Following, // đang đi theo hero
        Moving, // đang đi tự do
    };
    
    public enum AutoType {
        Explode,
        ChangeToAnother,
        BlinkRed
    }

    public class Enemy : Entity {
        [SerializeField]
        protected HealthBar healthBar;
        
        [SerializeField]
        private EnemyAnimator animator;
        public EnemyAnimator Animator => animator;

        [SerializeField]
        public Dropper dropper;

        private EnemyType _enemyType;
        public EnemyType EnemyType {
            get => _enemyType;
            set {
                _enemyType = value;
                animator.SetEnemyType(value);
            }        
        }
        
        public int Id { get; set; }
        public float Damage { get; set; }
        public Health Health { get; protected set; }
        public Movable Movable { get; private set; }
        public Shootable Shootable { get; protected set; }
        public Spawnable Spawnable { get; protected set; }
        protected DamageReceiver Receiver { get; private set; }
        public WalkThrough WalkThrough { get; private set; }
        public SkullComponent Skull { get; private set; }
        public bool IsFallingDown { get; set; } = false;
        public bool IsActive { private set; get; } = true;

        private bool _isBoostSpeed;
        private BlinkEffect _blinkEffect;

        private List<int> _bombExplosion = new List<int>();

        protected ICountDown _countDown = null;
        protected AutoType _autoType;
        protected EnemyType _changeToType;
        protected float _timeToExplode;
        
        public EnemyStatus Status { get; protected set; }

        protected virtual void Awake() {
            Receiver = GetComponent<DamageReceiver>();
            Skull = GetComponent<SkullComponent>();
            _blinkEffect = GetComponent<BlinkEffect>();
            
            // Add Move Component
            var callback = new MovableCallback() {
                IsAlive = () => IsAlive,
                GetMapManager = () => EntityManager.MapManager,
                GetLocalPosition = () => transform.localPosition,
                SetLocalPosition = SetLocationPosition,
                NotCheckCanMove = () => false, // not check when entity is Boss or Bomb or Spike 
                IsThoughBomb = IsThroughBomb,
                FixHeroOutSideMap = () => { }
            };
            Movable = new Movable(this);
            WalkThrough = new WalkThrough(this.Type, Movable);
            Movable.Init(WalkThrough, callback);
            AddEntityComponent<WalkThrough>(WalkThrough);
            AddEntityComponent<Movable>(Movable);
        }
        
        private void SetLocationPosition(Vector2 localPosition) {
            transform.localPosition = localPosition;
        }

        private static bool IsThroughBomb(bool value) {
            return value;
        }
        
        public virtual void StartSpeed() {
            //do nothing
        }

        protected async void RandomStartSpeed() {
            if (_isBoostSpeed) {
                return;
            }
            var prevSpeed = Movable.Speed;
            Movable.Speed = UnityEngine.Random.Range(0, prevSpeed);
            await App.WebGLTaskDelay.Instance.Delay(1000);
            Movable.Speed = prevSpeed;
            _isBoostSpeed = false;
        }

        public void SetStatus(EnemyStatus status) {
            Status = status;
        }
        
        public void SetImmortal() {
            Immortal = true;
            if (_blinkEffect) {
                _blinkEffect.StartBlink(() => Immortal = false);
            }
        }

        public void SetAutoExplode(float timeToExplode) {
            _countDown = new AutoCountDown(timeToExplode);
            _autoType = AutoType.Explode;
        }

        public void SetAutoBlinkBeforeExplode(float timeToBlink, float timeToExplode) {
            _countDown = new AutoCountDown(timeToBlink);
            _autoType = AutoType.BlinkRed;
            _timeToExplode = timeToExplode;
        }

        public void SetAutoChangeToAnother(float timeToChange, EnemyType otherType) {
            _countDown = new AutoCountDown(timeToChange);
            _autoType = AutoType.ChangeToAnother;
            _changeToType = otherType;
        }

        public async void BoostSpeed(float speed, float duration) {
            if (_isBoostSpeed) {
                return;
            }
            _isBoostSpeed = true;
            var prevSpeed = Movable.Speed;
            Movable.Speed = speed;
            await App.WebGLTaskDelay.Instance.Delay((int) (duration * 1000));
            Movable.Speed = prevSpeed;
            _isBoostSpeed = false;
        }

        public void TakeDamage(Entity dealer) {
            if (!IsAlive) {
                return;
            }
            if (Immortal) {
                return;
            }
            if (!(dealer is BombExplosion explosion)) {
                return;
            }
            if (explosion.IsEnemy) {
                return;
            }
            if (CheckHadTakeDamage(explosion.BombId)) {
                return;
            }

            if (!GameConstant.AdventureRequestServer) {
                SetActive(false);
            }
            var storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            UniTask.Void(async () => {
                var enemy = await storyModeManager.EnemyTakeDamage(Id, explosion.OwnerId);
                SetActive(true);
                if (enemy.Id == Id) {
                    TakeDamage(enemy);
                }
            });
        }

        public void ForceKill() {
            if (!GameConstant.AdventureRequestServer) {
                SetActive(false);
            }
            var storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            UniTask.Void(async () => {
                var result = await storyModeManager.KillStoryEnemy(Id);
                SetActive(true);
                if (result.Enemy.Id == Id) {
                    TakeDamage(result.Enemy);
                }
            });
        }

        public async void StandingWhenHitPlayer() {
            // BabyHoe will standing in 3s when hit.
            if (EnemyType != EnemyType.BabyHoe) {
                return;
            }
            Movable.ForceStop();
            SetStatus(EnemyStatus.Standing);
            await WebGLTaskDelay.Instance.Delay(3000);
            SetStatus(EnemyStatus.Moving);
        }

        protected virtual void TakeDamage(IEnemyDetails enemy) {
            Health.SetCurrentHealth(enemy.Hp);
            if (enemy.Hp <= 0) {
                Kill(true);
            } else {
                UpdateWithHealth();
            }
        }

        private void UpdateWithHealth() {
            // Nếu hp CraftyCat giảm xuống còn 20% thì biến thành GhostCraftyCat 
            if (EnemyType == EnemyType.CraftyCat) {
                var percent = Health.GetPercentHealth();
                if (percent <= 0.5f) {
                    ChangeToAnother(EnemyType.GhostCraftyCat);
                }
            }
        }

        protected void ChangeToAnother(EnemyType changeToType) {
            EnemyType = changeToType;
            animator.SetEnemyType(changeToType);
            dropper.SetEnemySprite(changeToType);

            // tạm thời mọi biến đổi đều thành quái xuyên block và follow
            WalkThrough.ThroughBrick = true;

            // disable FixMove
            var fixMove = GetComponent<FixMove>();
            if (fixMove != null) {
                fixMove.IsActive = false;
            }

            // thêm FollowMove
            if (GetComponent<FollowMove>() == null) {
                var follow = gameObject.AddComponent<FollowMove>();
                follow.Initialized();
            }
        }

        protected void BlinkBeforeExplode() {
            if (_blinkEffect) {
                _blinkEffect.IsBlinkRed = true;
                _blinkEffect.StartBlink(() => Kill(true), _timeToExplode);
            }
        }

        private bool CheckHadTakeDamage(int bombId) {
            if (_bombExplosion.Contains(bombId)) {
                return true;
            } else {
                SetHadTakeDamage(bombId);
                return false;
            }
        }

        private async void SetHadTakeDamage(int bombId) {
            _bombExplosion.Add(bombId);
            await App.WebGLTaskDelay.Instance.Delay(2000);
            _bombExplosion.Remove(bombId);
        }

        public virtual void SetActive(bool value) {
            IsActive = value;
            if (!value) {
                _blinkEffect.StartBlink(null, -1);
            } else {
                _blinkEffect.StopBlink();
            }

            // enable/disable FixMove
            var fixMove = GetComponent<FixMove>();
            if (fixMove != null) {
                fixMove.IsActive = value;
            }

            // enable/disable FollowMove
            var follow = GetComponent<FollowMove>();
            if (follow != null) {
                follow.Active = value;
            }
        }
    }
}