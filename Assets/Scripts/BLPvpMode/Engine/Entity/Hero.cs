using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Config;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Manager;
using BLPvpMode.Engine.Strategy.Position;
using BLPvpMode.Engine.User;
using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using PvpMode.Services;

using Senspark;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Entity {
    public class HeroListener : IHeroListener {
        [CanBeNull]
        public Action<IHero, int, HeroDamageSource> OnDamaged { get; set; }

        [CanBeNull]
        public Action<IHero, int, int> OnHealthChanged { get; set; }

        public Action<IHero, HeroItem, int, int> OnItemChanged { get; set; }

        [CanBeNull]
        public Action<IHero, HeroEffect, HeroEffectReason, int> OnEffectBegan { get; set; }

        [CanBeNull]
        public Action<IHero, HeroEffect, HeroEffectReason> OnEffectEnded { get; set; }

        [CanBeNull]
        public Action<IHero, Vector2> OnMoved { get; set; }

        void IHeroListener.OnDamaged(IHero hero, int amount, HeroDamageSource source) {
            OnDamaged?.Invoke(hero, amount, source);
        }

        void IHeroListener.OnHealthChanged(IHero hero, int amount, int oldAmount) {
            OnHealthChanged?.Invoke(hero, amount, oldAmount);
        }

        void IHeroListener.OnItemChanged(IHero hero, HeroItem item, int amount, int oldAmount) {
            OnItemChanged?.Invoke(hero, item, amount, oldAmount);
        }

        void IHeroListener.OnEffectBegan(IHero hero, HeroEffect effect, HeroEffectReason reason, int duration) {
            OnEffectBegan?.Invoke(hero, effect, reason, duration);
        }

        void IHeroListener.OnEffectEnded(IHero hero, HeroEffect effect, HeroEffectReason reason) {
            OnEffectEnded?.Invoke(hero, effect, reason);
        }

        void IHeroListener.OnMoved(IHero hero, Vector2 position) {
            OnMoved?.Invoke(hero, position);
        }
    }

    internal interface IEffectManager {
        [NotNull]
        IHeroEffectState GetState(HeroEffect effect);

        void Push(HeroEffect effect, int duration, HeroEffectReason reason);
        void Pop(HeroEffect effect, HeroEffectReason reason);
        void Step(int delta, Action<HeroEffect> onTimeOut);
    }

    internal static class EffectManagerExtensions {
        public static bool IsActive(this IEffectManager manager, HeroEffect effect) {
            return manager.GetState(effect).IsActive;
        }
    }

    internal class EffectManager : IEffectManager {
        [NotNull]
        private readonly ITimeManager _timeManager;

        [NotNull]
        private readonly Dictionary<HeroEffect, (int, int)> _states;

        [NotNull]
        private readonly Dictionary<HeroEffect, HeroEffectReason> _reasons;

        public EffectManager(
            [NotNull] Dictionary<HeroEffect, IHeroEffectState> initialState,
            [NotNull] ITimeManager timeManager
        ) {
            _timeManager = timeManager;
            _states = initialState
                .Where(it => it.Value.IsActive)
                .MapValues((_, state) => (state.Timestamp, state.Duration));
            _reasons = initialState
                .MapValues((_, state) => state.Reason);
        }

        public IHeroEffectState GetState(HeroEffect effect) {
            var (timestamp, duration) = _states.TryGetValue(effect, out var result) ? result : (0, 0);
            return new HeroEffectState(
                isActive: _states.ContainsKey(effect),
                reason: _reasons.TryGetValue(effect, out var reason) ? reason : HeroEffectReason.Null,
                timestamp: timestamp,
                duration: duration);
        }

        public void Push(HeroEffect effect, int duration, HeroEffectReason reason) {
            _states[effect] = ((int) _timeManager.Timestamp, duration);
            _reasons[effect] = reason;
        }

        public void Pop(HeroEffect effect, HeroEffectReason reason) {
            _states.Remove(effect);
            _reasons[effect] = reason;
        }

        public void Step(int delta, Action<HeroEffect> onTimeOut) {
            var now = (int) _timeManager.Timestamp;
            var effects = _states.Keys.ToList();
            effects.ForEach(it => {
                if (!_states.TryGetValue(it, out var state)) {
                    return;
                }
                var (timestamp, duration) = state;
                if (timestamp + duration > now) {
                    // OK.
                } else {
                    onTimeOut(it);
                }
            });
        }
    }

    public class Hero : IHero {
        [NotNull]
        private readonly IParticipantController _controller;

        [NotNull]
        private readonly IHeroConfig _config;

        [NotNull]
        private readonly IMatchHeroInfo _info;

        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IBombManager _bombManager;

        [NotNull]
        private readonly ITimeManager _timeManager;

        [NotNull]
        private readonly IRandom _random;

        [NotNull]
        private readonly IHeroListener _listener;

        [NotNull]
        private readonly ComponentContainer _componentContainer;

        [NotNull]
        private readonly IPositionStrategy _positionStrategy;

        [NotNull]
        private readonly IEffectManager _effectManager;

        private int _health;

        private int Speed {
            get {
                if (_effectManager.IsActive(HeroEffect.SpeedTo1)) {
                    return 1;
                }
                if (_effectManager.IsActive(HeroEffect.SpeedTo10)) {
                    return 10;
                }
                var items = Items.TryGetValue(HeroItem.Boots, out var result) ? result : 0;
                return Mathf.Min(_info.Speed + items, _info.MaxSpeed);
            }
        }

        private float MovementSpeed => 1.5f + Speed * 0.5f;

        private int BombRange {
            get {
                var items = Items.TryGetValue(HeroItem.FireUp, out var result) ? result : 0;
                return Mathf.Min(_info.BombRange + items, _info.MaxBombRange);
            }
        }

        private int BombCount {
            get {
                var items = Items.TryGetValue(HeroItem.BombUp, out var result) ? result : 0;
                return Mathf.Min(_info.BombCount + items, _info.MaxBombCount);
            }
        }

        private bool Shielded => IsAlive && _effectManager.IsActive(HeroEffect.Shield);
        private bool Invincible => IsAlive && _effectManager.IsActive(HeroEffect.Invincible);
        private bool Imprisoned => IsAlive && _effectManager.IsActive(HeroEffect.Imprisoned);

        public IHeroState State
            => new HeroState(
                isAlive: IsAlive,
                position: Position,
                direction: Direction,
                health: _health,
                damageSource: DamageSource,
                items: new Dictionary<HeroItem, int>(Items),
                effects: new[] {
                    HeroEffect.Shield, // 
                    HeroEffect.Invincible, //
                    HeroEffect.Imprisoned, // 
                    HeroEffect.SpeedTo1, // 
                    HeroEffect.SpeedTo10, // 
                    HeroEffect.ReverseDirection, //
                    HeroEffect.PlantBombRepeatedly,
                }.AssociateWith(it =>
                    _effectManager.GetState(it)
                )
            );

        public IEntityManager EntityManager { get; private set; }
        public bool IsAlive { get; private set; }
        public int Slot { get; }
        public int TeamId { get; }
        public HeroDamageSource DamageSource { get; private set; }

        public Dictionary<HeroItem, int> Items { get; }
        public Vector2 Position { get; private set; }
        public Direction Direction { get; private set; }

        public Hero(
            int slot,
            int teamId,
            [NotNull] IHeroState initialState,
            [NotNull] IParticipantController controller,
            [NotNull] IHeroConfig config,
            [NotNull] IMatchHeroInfo info,
            [NotNull] ILogManager logger,
            [NotNull] IBombManager bombManager,
            [NotNull] ITimeManager timeManager,
            [NotNull] IRandom random,
            [NotNull] IHeroListener listener
        ) {
            var baseState = initialState.BaseState;
            var positionState = initialState.PositionState;
            Assert.IsTrue(baseState != null && positionState != null);
            IsAlive = initialState.IsAlive;
            Slot = slot;
            TeamId = teamId;
            _health = baseState.Health;
            DamageSource = baseState.DamageSource;
            Items = new Dictionary<HeroItem, int>(baseState.Items);
            Position = positionState.Position;
            Direction = positionState.Direction;
            _controller = controller;
            _config = config;
            _info = info;
            _logger = logger;
            _bombManager = bombManager;
            _timeManager = timeManager;
            _random = random;
            _listener = listener;
            _componentContainer = new ComponentContainer();
            _componentContainer.AddComponent(new StateComponent(this, () => State));
            _positionStrategy = new InterpolationPositionStrategy(500, 10, 100);
            _positionStrategy.AddPosition((int) _timeManager.Timestamp, Position);
            _effectManager = new EffectManager(baseState.Effects, _timeManager);
        }

        public T GetComponent<T>() where T : IEntityComponent {
            return _componentContainer.GetComponent<T>();
        }

        public void Kill() {
            IsAlive = false;
        }

        public void ApplyState(IHeroState state) {
            IsAlive = state.IsAlive;
            var baseState = state.BaseState;
            if (baseState != null) {
                var health = _health;
                _health = baseState.Health;
                if (health != _health) {
                    _listener.OnHealthChanged(this, _health, health);
                }
                var damageSource = DamageSource;
                DamageSource = baseState.DamageSource;
                if (health != _health || damageSource != DamageSource) {
                    _listener.OnDamaged(this, health - _health, DamageSource);
                }
                baseState.Items.ForEach((item, value) => {
                    var oldValue = Items.TryGetValue(item, out var oldValueRes) ? oldValueRes : 0;
                    if (oldValue == value) {
                        return;
                    }
                    Items[item] = value;
                    _listener.OnItemChanged(this, item, value, oldValue);
                });
                baseState.Effects.ForEach((effect, effectState) => {
                    if (effectState.IsActive) {
                        PushEffect(effect, effectState.Reason, effectState.Duration);
                    } else {
                        PopEffect(effect, effectState.Reason);
                    }
                });
            }
            var positionState = state.PositionState;
            if (positionState != null) {
                Position = positionState.Position;
                Direction = positionState.Direction;
                _listener.OnMoved(this, Position);
            }
        }

        public void Move(int timestamp, Vector2 position) {
            Assert.IsTrue(IsAlive, $"Cannot move when not alive [slot={Slot}]");
            Assert.IsTrue(!Imprisoned, $"Cannot move when imprisoned [slot={Slot}]");
            _positionStrategy.AddPosition(timestamp, position);
            var delta = position - Position;
            var direction = (delta.x, delta.y) switch {
                (< 0, _) => Direction.Left,
                (> 0, _) => Direction.Right,
                (_, < 0) => Direction.Down,
                (_, > 0) => Direction.Up,
                _ => Direction,
            };
            Position = position;
            Direction = direction;
            _listener.OnMoved(this, position);
        }

        public IBomb PlantBomb(int timestamp, bool byHero) {
            Assert.IsTrue(IsAlive, $"Cannot plant bomb when not alive [slot={Slot}]");
            Assert.IsTrue(!Imprisoned, $"Cannot plant bomb when imprisoned [slot={Slot}]");
            var bombs = _bombManager.GetBombs(Slot);
            Assert.IsTrue(bombs.Count < BombCount,
                $"Maximum amount of planted bombs reached [slot={Slot}]");
            var position = _positionStrategy.GetPosition(timestamp);
            var state = new BombState(
                isAlive: true,
                slot: Slot,
                reason: byHero ? BombReason.Planted : BombReason.PlantedBySkull,
                position: position,
                range: BombRange,
                damage: _info.Damage,
                piercing: false,
                explodeDuration: _config.ExplodeDuration,
                explodeRanges: new Dictionary<Direction, int>(),
                plantTimestamp: (int) _timeManager.Timestamp
            );
            return _bombManager.PlantBomb(state);
        }

        public void DamageBomb(int amount) {
            if (!IsAlive) {
                return;
            }
            if (Invincible || Imprisoned) {
                return;
            }
            if (Shielded) {
                DisableShield(HeroEffectReason.Damaged);
                EnableInvincible();
                return;
            }
            var health = _health;
            Assert.IsTrue(health > 0, "Expected health > 0");
            _health = Mathf.Max(0, _health - amount);
            _listener.OnHealthChanged(this, _health, health);
            DamageSource = HeroDamageSource.Bomb;
            _listener.OnDamaged(this, amount, DamageSource);
            if (_health > 0) {
                EnableInvincible();
            } else {
                EnableImprisoned();
                DisableSkullEffects();
            }
        }

        public void DamagePrison() {
            EndImprisoned(HeroEffectReason.Damaged);
        }

        public void RescuePrison() {
            EndImprisoned(HeroEffectReason.Rescue);
        }

        private void EndImprisoned(HeroEffectReason reason) {
            if (!IsAlive) {
                return;
            }
            if (!Imprisoned) {
                return;
            }
            DisableImprisoned(reason);
            switch (reason) {
                case HeroEffectReason.UseBooster or HeroEffectReason.Rescue: {
                    var health = _health;
                    Assert.IsTrue(health == 0, "Expected health = 0");
                    _health = Mathf.Min(_info.Health, 3);
                    _listener.OnHealthChanged(this, _health, health);
                    EnableInvincible();
                    break;
                }
                case HeroEffectReason.TimeOut or HeroEffectReason.Damaged: {
                    DamageSource = HeroDamageSource.PrisonBreak;
                    _listener.OnDamaged(this, 0, DamageSource);
                    Kill();
                    break;
                }
                default: throw new Exception($"Invalid imprison reason");
            }
        }

        public void DamageFallingBlock() {
            if (!IsAlive) {
                return;
            }
            var health = _health;
            _health = 0;
            _listener.OnHealthChanged(this, _health, health);
            DamageSource = HeroDamageSource.HardBlock;
            _listener.OnDamaged(this, health, DamageSource);
            Kill();
        }

        public void UseBooster(Booster booster) {
            Assert.IsTrue(IsAlive, "Cannot use booster when not alive");
            if (Imprisoned) {
                Assert.IsTrue(booster == Booster.Key, "Can only use key when imprisoned");
            } else {
                Assert.IsTrue(booster != Booster.Key, "Can use any items except key when not imprisoned");
            }
            _controller.UseBooster(booster);
            _logger.Log($"[Hero:UseBooster] slot={Slot} booster={booster}");
            switch (booster) {
                case Booster.Shield: {
                    EnableShield(HeroEffectReason.UseBooster);
                    break;
                }
                case Booster.Key: {
                    EndImprisoned(HeroEffectReason.UseBooster);
                    break;
                }
                default: throw new Exception($"Invalid item type: {booster}");
            }
        }

        public void TakeItem(BlockType blockType) {
            switch (blockType) {
                // Handle booster items.
                case BlockType.Shield: {
                    EnableShield(HeroEffectReason.TakeItem);
                    break;
                }
                case BlockType.Skull: {
                    DisableSkullEffects();
                    EnableSkullEffects();
                    break;
                }
                default: {
                    // Normal items.
                    var (item, amount) = blockType switch {
                        BlockType.BombUp => (HeroItem.BombUp, 1),
                        BlockType.FireUp => (HeroItem.FireUp, 1),
                        BlockType.Boots => (HeroItem.Boots, 1),
                        BlockType.GoldX1 => (HeroItem.Gold, _random.RandomInt(1, 3 + 1)),
                        BlockType.GoldX5 => (HeroItem.Gold, _random.RandomInt(4, 6 + 1)),
                        _ => throw new ArgumentOutOfRangeException(nameof(blockType), blockType, null)
                    };
                    var oldValue = Items.TryGetValue(item, out var oldValueRes) ? oldValueRes : 0;
                    Items[item] = oldValue + amount;
                    _listener.OnItemChanged(this, item, oldValue + amount, oldValue);
                    break;
                }
            }
        }

        private void PushEffect(HeroEffect effect, HeroEffectReason reason, int duration) {
            var state = _effectManager.GetState(effect);
            _effectManager.Push(effect, duration, reason);
            if (!state.IsActive || state.Reason != reason) {
                _listener.OnEffectBegan(this, effect, reason, duration);
            }
        }

        private void PopEffect(HeroEffect effect, HeroEffectReason reason) {
            var state = _effectManager.GetState(effect);
            _effectManager.Pop(effect, reason);
            if (state.IsActive || state.Reason != reason) {
                _listener.OnEffectEnded(this, effect, reason);
            }
        }

        private void EnableShield(HeroEffectReason reason) {
            Assert.IsTrue(IsAlive, $"Cannot be shielded when not alive [slot={Slot}]");
            PushEffect(HeroEffect.Shield, reason, _config.ShieldedDuration);
        }

        private void DisableShield(HeroEffectReason reason) {
            Assert.IsTrue(Shielded, $"Cannot disable shield when not shielded [slot={Slot}]");
            PopEffect(HeroEffect.Shield, reason);
        }

        private void EnableInvincible() {
            Assert.IsTrue(IsAlive, $"Cannot be invincible when not alive [slot={Slot}]");
            PushEffect(HeroEffect.Invincible, HeroEffectReason.Null, _config.InvincibleDuration);
        }

        private void DisableInvincible() {
            Assert.IsTrue(Invincible, $"Cannot disable invincible when not invincible [slot={Slot}]");
            PopEffect(HeroEffect.Invincible, HeroEffectReason.Null);
        }

        private void EnableImprisoned() {
            Assert.IsTrue(IsAlive, $"Cannot be imprisoned when not alive [slot={Slot}]");
            PushEffect(HeroEffect.Imprisoned, HeroEffectReason.Null, _config.ImprisonedDuration);
        }

        private void DisableImprisoned(HeroEffectReason reason) {
            Assert.IsTrue(Imprisoned, $"Cannot disable imprison when not imprisoned [slot={Slot}]");
            PopEffect(HeroEffect.Imprisoned, reason);
        }

        private void EnableSkullEffects() {
            var effects = new[] {
                HeroEffect.SpeedTo1, //
                HeroEffect.SpeedTo10, //
                HeroEffect.ReverseDirection, //
                HeroEffect.PlantBombRepeatedly,
            };
            var randomizer = new WeightedRandomizer<HeroEffect>(effects, new[] { 1f, 1f, 1f, 1f });
            var effect = randomizer.Random(_random);
            PushEffect(effect, HeroEffectReason.TakeItem, _config.SkullEffectDuration);
        }

        private void DisableSkullEffects() {
            new[] {
                HeroEffect.SpeedTo1, //
                HeroEffect.SpeedTo10, //
                HeroEffect.ReverseDirection, //
                HeroEffect.PlantBombRepeatedly,
            }.ForEach(it => {
                if (_effectManager.IsActive(it)) {
                    PopEffect(it, HeroEffectReason.TakeItem);
                }
            });
        }

        public void Begin(IEntityManager entityManager) {
            EntityManager = entityManager;
        }

        public void Update(int delta) {
            if (!IsAlive) {
                return;
            }
            _effectManager.Step(delta, it => {
                switch (it) {
                    case HeroEffect.Shield: {
                        DisableShield(HeroEffectReason.TimeOut);
                        break;
                    }
                    case HeroEffect.Invincible: {
                        DisableInvincible();
                        break;
                    }
                    case HeroEffect.Imprisoned: {
                        EndImprisoned(HeroEffectReason.TimeOut);
                        break;
                    }
                    default: {
                        PopEffect(it, HeroEffectReason.TimeOut);
                        break;
                    }
                }
            });
            var positionInt = new Vector2Int(
                Mathf.FloorToInt(Position.x),
                Mathf.FloorToInt(Position.y));
            if (_effectManager.IsActive(HeroEffect.PlantBombRepeatedly) &&
                _bombManager.GetBombs(Slot).Count < BombCount &&
                _bombManager.GetBomb(positionInt) == null) {
                try {
                    PlantBomb((int) _timeManager.Timestamp, false);
                } catch (Exception ex) {
                    // Ignore.
                }
            }
        }

        public void End() {
            EntityManager = null;
        }
    }
}