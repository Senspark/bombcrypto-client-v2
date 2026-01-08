using System;
using System.Collections.Generic;

using Animation;

using App;

using Engine.Components;
using Engine.Manager;
using Engine.Strategy.CountDown;
using Engine.Utils;

using JetBrains.Annotations;

using PvpMode.Services;

using Senspark;

using UnityEngine;

namespace Engine.Entities {
    public class Bomb : Entity {
        [SerializeField]
        private SpriteRenderer sprite;

        [SerializeField]
        private GameObject body;

        [SerializeField]
        private RuntimeAnimatorController scaleAnimator;

        [SerializeField]
        private EntityAnimator entityAnimator;

        [SerializeField]
        private AnimationResource resource;

        /// <summary>
        /// Bomb ID, zero-indexed.
        /// </summary>
        public int BombId { get; private set; }

        public int BombSkin { get; private set; }
        public int ExplosionSkin { get; private set; }
        public HeroId OwnerId { get; private set; }
        public bool IsEnemy { get; private set; }
        public bool IsThroughHero { get; private set; }

        private string _groupId = "";

        public string GroupId {
            get => _groupId;
            set => _groupId = value;
        }

        private BombMovable _movable;
        private Action<Bomb> _onExplodedCallback;

        public float Damage { set; get; }
        private float DamageJail { set; get; }

        public int ExplosionLength { set; get; }
        public bool ThroughBrick { set; get; }
        protected ICountDown CountDown { set; get; }

        private Animator _animator;
        private IAnimatorHelper _animatorHelper;
        private float _elapsed;
        private float _duration;

        #region UNITY EVENTS

        protected virtual void Awake() {
            var damageReceiver = GetComponent<DamageReceiver>();
            if (damageReceiver != null) {
                damageReceiver.SetOnTakeDamage(TakeDamage);
            }

            var updater = new Updater().OnUpdate(OnUpdate);
            AddEntityComponent<Updater>(updater);
            var movableCallback = new MovableCallback() {
                IsAlive = () => IsAlive,
                GetMapManager = () => EntityManager.MapManager,
                GetLocalPosition = () => transform.localPosition,
                SetLocalPosition = SetLocationPosition,
                NotCheckCanMove = () => false, // not check when entity is Boss or Bomb or Spike 
                IsThoughBomb = IsThroughBomb,
                FixHeroOutSideMap = () => { },
                SetActiveReverseIcon = (value) => { }
            };
            _movable = new BombMovable(this) {
                Speed = 4
            };
            var walkThrough = new WalkThrough(this.Type, _movable);
            _movable.Init(walkThrough, movableCallback);
            AddEntityComponent<WalkThrough>(walkThrough);
            AddEntityComponent<Movable>(_movable);

        }

        protected virtual void OnUpdate(float delta) {
            UpdateCountDown(delta);

            if (_animator == null) {
                return;
            }
            if (!_animatorHelper.Enabled) {
                return;
            }
            _animatorHelper.Update(delta);
        }

        public void UpdateCountDown(float delta) {
            if (CountDown == null) {
                return;
            }
            CountDown.Update(delta);
            if (CountDown.IsTimeEnd) {
                StartExplode(transform.localPosition);
            }
        }

        public void SetOrderLayer(int value) {
            sprite.sortingOrder = value;
        }

        #endregion

        #region PUBLIC METHODS

        public void Init(int bId, HeroId id, int skin, int explosionSkin, float damage, float damageJail,
            int explosionlength,
            float timeToExplode, bool throughBrick, Action<Bomb> callback, bool isEnemy = false,
            bool isThroughHero = false) {
            BombId = bId;
            BombSkin = skin;
            ExplosionSkin = explosionSkin;
            OwnerId = id;
            IsEnemy = isEnemy;
            IsThroughHero = isThroughHero;
            Damage = damage;
            DamageJail = damageJail;
            ExplosionLength = explosionlength;
            ThroughBrick = throughBrick;
            _onExplodedCallback = callback;
            entityAnimator.SetItemId(skin);
            if (!entityAnimator.PlayIdle()) {
                PlayScaleAnimator();
            }

            if (timeToExplode >= 0) {
                CountDown = new AutoCountDown(timeToExplode);
            } else {
                CountDown = null;
            }
        }

        private void SetLocationPosition(Vector2 localPosition) {
            transform.localPosition = localPosition;
        }

        private static bool IsThroughBomb(bool value) {
            return value;
        }
        
        private void PlayScaleAnimator() {
            sprite.sprite = resource.GetSpriteBombSkin((EnemyBombSkin) BombSkin);
            _animator = body.GetComponent<Animator>();
            if (_animator == null) {
                _animator = body.AddComponent<Animator>();
            }
            _animator.runtimeAnimatorController = scaleAnimator;
            _animatorHelper = new AnimatorHelper(_animator);
            _animatorHelper.Enabled = true;
        }

        public void SetCountDownEnable(bool value) {
            if (CountDown != null) {
                CountDown.SetEnable(value);
            }
        }

        public void ForceMove(Vector2 direction) {
            _movable.TrySetVelocity(direction * _movable.Speed);
        }

        public void DestroyMe() {
            var tileLocation = EntityManager.MapManager.GetTileLocation(transform.localPosition);
            EntityManager.MapManager.RemoveBomb(tileLocation);
            OnExplodeEnd();
            Kill(false);
        }

        public void ForceExplode(Vector2 pos, [NotNull] Dictionary<Direction, int> ranges, (int, int)[] brokenList,
            bool isShaking = false) {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.BombExplode);
            Explode(pos, ranges, brokenList, isShaking);
            Kill(true);
        }

        public void StartExplode(Vector2 pos) {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.BombExplode);
            Explode(pos, !IsEnemy);
            Kill(true);
        }

        public void CheckOnBomb() {
            var hitArray = Physics2D.RaycastAll(transform.position, Vector2.right, 0.1f);
            if (hitArray.Length <= 1) {
                GetComponent<Collider2D>().isTrigger = false;
            }
        }

        #endregion

        #region PRIVATE METHODS

        private void TakeDamage(Entity dealer) {
            if (dealer is BombExplosion || dealer is WallDrop) {
                StartExplode(transform.localPosition);
            }
        }

        private void Explode(Vector2 pos, bool isShaking) {
            var explodeEvent = new BombExplodeEvent(pos, this, isShaking);
            EntityManager.ExplodeEventManager.PushEvent(explodeEvent);
            OnExplodeEnd();
        }

        private void Explode(Vector2 pos, [NotNull] Dictionary<Direction, int> ranges, (int, int)[] brokenList,
            bool isShaking) {
            var explodeEvent = new BombExplodeEvent(pos, this, ranges, brokenList, isShaking);
            EntityManager.ExplodeEventManager.PushEvent(explodeEvent);
            OnExplodeEnd();
        }

        private void OnExplodeEnd() {
            _onExplodedCallback?.Invoke(this);
            _onExplodedCallback = null;
            CountDown = null;
        }

        #endregion
    }
}