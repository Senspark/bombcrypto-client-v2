using BLPvpMode.Engine.Manager;
using BLPvpMode.Manager;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public class Bomb : IBomb {
        [NotNull]
        private readonly ComponentContainer _componentContainer;

        [NotNull]
        private readonly IBombManager _bombManager;

        [NotNull]
        private readonly ITimeManager _timeManager;

        private int _explodeDuration;

        public IEntityManager EntityManager { get; private set; }

        public IBombState State
            => new BombState(
                isAlive: IsAlive,
                slot: Slot,
                reason: Reason,
                position: Position,
                damage: Damage,
                range: Range,
                piercing: Piercing,
                explodeDuration: _explodeDuration,
                explodeRanges: _bombManager.GetExplodeRanges(this),
                plantTimestamp: PlantTimestamp
            );

        public bool IsAlive { get; private set; }
        public BombReason Reason { get; private set; }
        public Vector2 Position { get; private set; }
        public int Slot { get; private set; }
        public int Id { get; }
        public int Range { get; private set; }
        public int Damage { get; private set; }
        public bool Piercing { get; private set; }

        public int PlantTimestamp { get; private set; }

        public Bomb(
            int id,
            [NotNull] IBombState initialState,
            [NotNull] IBombManager bombManager,
            [NotNull] ITimeManager timeManager
        ) {
            IsAlive = initialState.IsAlive;
            Slot = initialState.Slot;
            Id = id;
            Reason = initialState.Reason;
            Position = initialState.Position;
            Range = initialState.Range;
            Damage = initialState.Damage;
            Piercing = initialState.Piercing;
            _explodeDuration = initialState.ExplodeDuration;
            PlantTimestamp = initialState.PlantTimestamp;
            _componentContainer = new ComponentContainer();
            _componentContainer.AddComponent(new StateComponent(this, () => State));
            _bombManager = bombManager;
            _timeManager = timeManager;
        }

        public T GetComponent<T>() where T : IEntityComponent {
            return _componentContainer.GetComponent<T>();
        }

        public void ApplyState(IBombState state) {
            IsAlive = state.IsAlive;
            Slot = state.Slot;
            Reason = state.Reason;
            Position = state.Position;
            Range = state.Range;
            Damage = state.Damage;
            Piercing = state.Piercing;
            _explodeDuration = state.ExplodeDuration;
            PlantTimestamp = state.PlantTimestamp;
        }

        public void Kill() {
            Kill(BombReason.Null);
        }

        public void Kill(BombReason reason) {
            Reason = reason;
            IsAlive = false;
            _bombManager.RemoveBomb(this);
        }

        public void Begin(IEntityManager entityManager) {
            EntityManager = entityManager;
        }

        public void Update(int delta) {
            if (!IsAlive) {
                return;
            }
            var timestamp = _timeManager.Timestamp;
            if (timestamp + delta >= PlantTimestamp + _explodeDuration) {
                _bombManager.ExplodeBomb(this);
            }
        }

        public void End() {
            EntityManager = null;
        }
    }
}