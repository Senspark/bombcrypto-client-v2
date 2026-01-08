using System;
using System.Collections.Generic;

using Animation;

using App;

using BLPvpMode.Engine.Entity;

using Constant;

using Senspark;

using Engine.Components;
using Engine.ScriptableObject;
using Engine.Strategy.Spawner;
using Engine.Strategy.Weapon;
using Engine.UI;

using StoryMode.UI;

using UnityEngine;

namespace Engine.Entities {
    public class Player : BasePlayer {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        public Dropper dropper;

        [SerializeField]
        public SpriteRenderer backlight;

        [SerializeField]
        public Components.Avatar avatar;

        [SerializeField]
        public HealthBar healthBar;
        
        [SerializeField]
        public Transform trail;

        public Dictionary<StatId, int> MaximumStats { set; private get; }

        [SerializeField]
        private HeroAnimator animator;

        [SerializeField]
        private BlinkEffect blinkEffect;

        [SerializeField]
        private BlinkColorEffect blinkColorEffect;

        [SerializeField]
        private Color blinkColor;

        [SerializeField]
        private Animator pickAnimator;

        [SerializeField]
        protected SpriteRenderer shieldEffect;

        [SerializeField]
        private FlyerText flyer;

        [SerializeField]
        private BLTrailRes trailRes;

        [SerializeField]
        private ParticleSystem killEffect;

        [SerializeField]
        protected GameObject reverseIcon;
        
        private Shieldable shieldable;
        public bool IsFreeze { get; private set; } = false;
        public bool IsInJail { get; private set; } = false;

        public bool BombLoseControl { get; private set; } = false;

        public int Slot { get; protected set; }

        private float _currentSpeed;
        private bool _isSkullSpeedEffect;
        protected Prison Prison { get; private set; }

        public bool IsTakeDamage { get; private set; }

        public bool StopMusicWhenDie { get; set; } = true;

        protected override void Awake() {
            base.Awake();
            
            // add updater
            var updater = new Updater();
            updater.OnBegin(InitFirstFrame);
            AddEntityComponent<Updater>(updater);
            
            // add Health
            Health = new Health(this, healthBar);
            AddEntityComponent<Health>(Health);
            
            // Add Bombable
            Bombable = new Bombable(this);
            AddEntityComponent<Bombable>(Bombable);
            
            // Add Shieldable
            shieldable = new Shieldable(shieldEffect, updater);
            AddEntityComponent<Shieldable>(shieldable);
            
            // add others
            InitMoveComponents();
            
            if (killEffect != null) {
                killEffect.gameObject.SetActive(false);
                var main = killEffect.main;
                main.stopAction = ParticleSystemStopAction.Callback;
            }
        }

        protected virtual void InitMoveComponents() {
            var callback = GenerateMovableCallback();
            callback.SetActiveReverseIcon = (value) => {
                if (reverseIcon != null) {
                    reverseIcon.SetActive(value);
                }
            };
            
            Movable = new Movable(this);
            WalkThrough = new WalkThrough(this.Type, Movable);
            Movable.Init(WalkThrough, callback);
            AddEntityComponent<WalkThrough>(WalkThrough);
            AddEntityComponent<Movable>(Movable);            
        }
        
        public void Init(int slot) {
            Slot = slot;
        }

        private void InitFirstFrame() {
            if (Receiver != null) {
                Receiver.SetOnTakeDamage(TakeDamage);
            }
            SetupWeapon();
            _isSkullSpeedEffect = false;
            animator.PlayIdle(FaceDirection.Down);
        }

        public override void SetPlayerID(HeroId heroId) {
            base.SetPlayerID(heroId);
            spriteRenderer.sortingOrder = 30;
        }

        public void SetShieldEffectVisible(bool visible) {
            shieldEffect.sortingOrder = spriteRenderer.sortingOrder + 1;
            shieldEffect.gameObject.SetActive(visible);
        }

        public void RequestTakeItem(Item item) {
            EntityManager.PlayerManager.RequestTakeItem(item);
        }

        private void TakeDamage(Entity dealer) {
            if (!IsAlive) {
                return;
            }
            var isTakeDamage = false;
            var isInJail = false;
            var ownerId = 0;
            if (!Immortal) {
                switch (dealer) {
                    case Enemy enemy:
                        if (enemy.IsActive) {
                            ownerId = enemy.Id;
                            isTakeDamage = true;
                        }
                        break;
                    case BossCollider bossCollider:
                        ownerId = bossCollider.boss.Id;
                        isTakeDamage = true;
                        break;
                    case BombExplosion explosion:
                        ownerId = explosion.OwnerId.Id;
                        //isInJail = true; // no inJail any more
                        isTakeDamage = true;
                        break;
                    case Fire fire:
                        ownerId = fire.OwnerId;
                        //isInJail = true;
                        isTakeDamage = true;
                        break;
                    case Spike spike:
                        ownerId = spike.OwnerId;
                        //isInJail = true;
                        isTakeDamage = true;
                        dealer.Kill(false);
                        break;
                }
            } else {
                ShieldBreak();
            }

            if (isTakeDamage) {
                if (!_isSkullSpeedEffect) {
                    _currentSpeed = Movable.Speed;
                }
                StopSkullHeadEffect();
                SetImmortal();
                RequestTakeDamage(ownerId, dealer);
                return;
            }
            if (isInJail) {
                SetInJail(ownerId, dealer);
            }
        }

        private void RequestTakeDamage(int killerId, Entity dealer) {
            var lastPlayerLocation = GetTileLocation();
            SoundManager.PlaySound(Audio.HeroTakeDamage);
            EntityManager.TakeDamageEventManager.PushEvent(Slot, killerId, dealer, lastPlayerLocation);
        }

        public void PlayAnimTakeDamage() {
            IsTakeDamage = true;
            animator.PlayTakeDamage();
        }

        public void TakeDamage(int heroHp, Entity dealer) {
            UpdateHealthUi(heroHp);
            if (heroHp <= 0) {
                SoundManager.StopMusic();
                SaveCurrentInfo();
                Kill(true);
            } else {
                IsTakeDamage = false;
                if (dealer is not Enemy enemy) {
                    return;
                }
                if (enemy.EnemyType == EnemyType.BeerEaiter) {
                    TakeReverseEffect();
                } else if (enemy.EnemyType == EnemyType.BabyHoe) {
                    enemy.StandingWhenHitPlayer();
                    PushBack((transform.position - enemy.transform.position).normalized);
                }
            }
        }

        private void UpdateHealthUi(int value) {
            EntityManager.LevelManager.OnUpdateHealthUi(Slot, value);
        }

        private void SaveCurrentInfo() {
            var hero = EntityManager.LevelManager.StoryModeHero;
            var speed = _currentSpeed > 0 ? _currentSpeed : hero.Speed;
            hero.Speed = (int) speed;
            hero.BombCount = Bombable.MaxBombNumber;
            hero.BombRange = Bombable.ExplosionLength;
        }

        private void PushBack(Vector3 direction) {
            var location = GetTileLocation();
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
                // pushBack Horizontal
                var dx = direction.x < 0 ? -1 : 1;
                for (var i = 0; i < 3; i++) {
                    location.x += dx;
                    var empty = EntityManager.MapManager.IsEmpty(location);
                    if (empty) {
                        continue;
                    }
                    location.x -= dx;
                    break;
                }
            } else {
                // PushBack Vertical
                var dy = direction.y < 0 ? -1 : 1;
                for (var i = 0; i < 3; i++) {
                    location.y += dy;
                    var empty = EntityManager.MapManager.IsEmpty(location);
                    if (empty) {
                        continue;
                    }
                    location.y -= dy;
                    break;
                }
            }

            //Move player to location.
            var position = EntityManager.MapManager.GetTilePosition(location);
            SetPosition(position);
        }

        private async void TakeReverseEffect() {
            if (Movable.ReverseEffect) {
                return;
            }

            Movable.ReverseEffect = true;
            await WebGLTaskDelay.Instance.Delay(3000);
            Movable.ReverseEffect = false;
        }

        protected virtual void SetupWeapon() {
            var weapon = new BombWeapon(WeaponType.Bomb,
                new BombSpawner(),
                Bombable);
            Bombable.SetWeapon(weapon);
            Bombable.SetSpawnLocation(GetSpawnLocation);
        }

        private Vector2Int GetSpawnLocation() {
            return GetTileLocation();
        }

        public void SetImmortal(float duration = 3f) {
            Immortal = true;
            blinkEffect.StartBlink(() => { Immortal = false; }, duration);
        }

        public void SetFreeze(bool value) {
            IsFreeze = value;
            if (value) {
                animator.PlayIdle(FaceDirection.Down);
                Movable.ForceStop();
            }
        }

        protected async void SetInJail(int ownerId, Entity dealer) {
            SoundManager.PlaySound(Audio.InJail);
            IsInJail = true;
            Immortal = true;
            SetFreeze(true);
            gameObject.SetActive(false);
            var location = GetTileLocation();
            Prison = (Prison) await EntityManager.LevelManager.CreateEntity(EntityType.Prison, location);
            if (dealer != null) {
                Prison.Init(() => OnEndJail(ownerId, dealer));
            } else {
                Prison.Init(null);
            }
            Prison.transform.localPosition = transform.localPosition;
            EntityManager.LevelManager.OnPlayerInJail(Slot);
        }

        public void ShieldBreak() {
            if (!shieldable.IsShielding) {
                return;
            }

            if (!_isSkullSpeedEffect) {
                _currentSpeed = Movable.Speed;
            }
            StopSkullHeadEffect();
            shieldable.StopShield();
            SetImmortal();
        }

        public void JailBreak() {
            if (IsInJail) {
                SoundManager.PlaySound(Audio.UseKey);
                Prison.JailBreak(OnAfterJailBreak);
            }
        }

        protected void JailBreakBeforeDie() {
            if (StopMusicWhenDie) {
                SoundManager.StopMusic();
            }

            SoundManager.PlaySound(Audio.HeroTakeDamage);
            if (IsInJail) {
                Prison.JailBreak(() => { Kill(true); });
                return;
            }
            Kill(true);
        }

        private void OnAfterJailBreak() {
            gameObject.SetActive(true);
            IsInJail = false;
            SetFreeze(false);
            Immortal = false;
            EntityManager.LevelManager.OnPlayerEndInJail(Slot);
        }

        private void OnEndJail(int ownerId, Entity dealer) {
            RequestTakeDamage(ownerId, dealer);
        }

        public void SetItem(ItemType type, int rewardValue = 0, bool playSound = true) {
            PlayAnimatePickItem(playSound);
            var levelManage = EntityManager.LevelManager;
            switch (type) {
                case ItemType.BombUp:
                    if (Bombable.MaxBombNumber < MaximumStats[StatId.Count]) {
                        var maxBomb = Bombable.IncreaseMaxBombNumber();
                        levelManage.OnUpdateItem(Slot, type, maxBomb);
                    }
                    break;
                case ItemType.FireUp:
                    if (Bombable.ExplosionLength < MaximumStats[StatId.Range]) {
                        var explosionLength = Bombable.IncreaseExplosionLength();
                        levelManage.OnUpdateItem(Slot, type, explosionLength);
                    }
                    break;
                case ItemType.Boots:
                    if (!_isSkullSpeedEffect) {
                        _currentSpeed = Movable.Speed;
                    }
                    if (_currentSpeed < MaximumStats[StatId.Speed]) {
                        _currentSpeed += 1f;
                        if (!_isSkullSpeedEffect) {
                            Movable.Speed = _currentSpeed;
                        }
                        levelManage.OnUpdateItem(Slot, type, (int) Movable.Speed);
                    }
                    break;
                case ItemType.Armor:
                    SetArmor(() => levelManage.OnUpdateItem(Slot, type, 0), 8, null);
                    levelManage.OnUpdateItem(Slot, type, 1);
                    break;
                case ItemType.Kick:
                    WalkThrough.ThroughBomb = false;
                    Bombable.SetBombsThroughAble(false);
                    KickAble = true;
                    levelManage.OnUpdateItem(Slot, type, 1);
                    break;
                case ItemType.GoldX5:
                case ItemType.GoldX1:
                case ItemType.BronzeChest:
                case ItemType.SilverChest:
                case ItemType.GoldChest:
                case ItemType.PlatinumChest:
                    if (rewardValue > 0) {
                        ShowFlyingText(rewardValue);
                    }
                    break;
            }
        }

        public void ShowFlyingText(int value) {
            if (flyer != null) {
                flyer.FlyingValueText(value);
            }
        }

        private void SetArmor(Action endCallback, float duration, UnityEngine.UI.Image process) {
            blinkEffect.StopBlink();
            Immortal = true;
            shieldable.StartShield(() => {
                endCallback?.Invoke();
                StopShieldEffect();
            }, duration, process);
        }

        public void SetShield(float duration, UnityEngine.UI.Image process) {
            SoundManager.PlaySound(Audio.UseShield);
            var levelManage = EntityManager.LevelManager;
            blinkEffect.StopBlink();
            Immortal = true;
            shieldable.StartShield(() => {
                levelManage.OnUpdateItem(Slot, ItemType.Armor, 0);
                StopShieldEffect();
            }, duration, process);
        }

        public void StartSkullHeadEffect(HeroEffect effect, int duration, bool playSound = true) {
            SoundManager.PlaySound(Audio.GetItem);
            PlayAnimatePickItem(playSound);
            if (!_isSkullSpeedEffect) {
                _currentSpeed = Movable.Speed;
            }
            StopSkullHeadEffect();
            switch (effect) {
                case HeroEffect.ReverseDirection:
                    StartReverseEffect();
                    break;
                case HeroEffect.SpeedTo10:
                    _isSkullSpeedEffect = true;
                    Movable.Speed = 10f;
                    EntityManager.LevelManager.OnUpdateItem(Slot, ItemType.Boots, (int) Movable.Speed);
                    break;
                case HeroEffect.PlantBombRepeatedly:
                    // pvp => plant bomb on step 
                    if (EntityManager.LevelManager.GameMode == GameModeType.StoryMode) {
                        BombLoseControl = true;
                    }
                    break;
                case HeroEffect.SpeedTo1:
                    _isSkullSpeedEffect = true;
                    Movable.Speed = 1f;
                    EntityManager.LevelManager.OnUpdateItem(Slot, ItemType.Boots, (int) Movable.Speed);
                    break;
            }

            blinkColorEffect.BlinkColor = blinkColor;
            blinkColorEffect.StartBlink(StopSkullHeadEffect, (duration / 1000f));
        }

        private void StartReverseEffect() {
            if (Movable.ReverseEffect) {
                return;
            }
            Movable.ReverseEffect = true;
        }

        private void StopReverseEffect() {
            Movable.ReverseEffect = false;
        }

        private void StopSkullHeadEffect() {
            blinkColorEffect.StopBlink();
            StopReverseEffect();
            _isSkullSpeedEffect = false;
            BombLoseControl = false;
            Movable.Speed = _currentSpeed;
            EntityManager.LevelManager.OnUpdateItem(Slot, ItemType.Boots, (int) Movable.Speed);
        }

        private void StopShieldEffect() {
            SoundManager.PlaySound(Audio.ArmorBreak);
            Immortal = false;
            shieldEffect.gameObject.SetActive(false);
        }

        private void PlayAnimatePickItem(bool playSound) {
            if (playSound) {
                SoundManager.PlaySound(Audio.PickUpItem);
            }
            pickAnimator.Play("Pick");
        }

        public void SetTrailEffect(int trailId) {
            if (!trailRes) {
                return;
            }
            if (!trail) {
                return;
            }
            trailRes.AttachTrailEffect(trail, trailId, blinkEffect);
        }

        public void PlayKillEffect() {
            if (killEffect != null) {
                killEffect.gameObject.SetActive(true);
                killEffect.Play();
            }
        }

        private void OnParticleSystemStopped() {
            killEffect.gameObject.SetActive(false);
        }

        public void ForceSetMaxSpeed(int maxSpeed) {
            MaximumStats[StatId.Speed] = maxSpeed;
        }

        public void ForceUpdateFace(Vector2 direction) {
            Movable.ForceStop();
            Movable.UpdateFace(direction);
            animator.PlayIdle(Movable.CurrentFace);
        }
    }
}