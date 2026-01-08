using BLPvpMode.Engine.Manager;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;

namespace BLPvpMode.Engine.Entity {
    public class Block : IBlock {
        [NotNull]
        public static IBlock CreateHardBlock(
            Vector2Int position,
            BlockReason reason,
            [NotNull] ILogManager logger,
            [NotNull] IMapManager mapManager
        ) {
            return new Block(
                position: position,
                initialState: new BlockState(
                    isAlive: true,
                    reason: reason,
                    type: BlockType.Hard,
                    health: 1,
                    maxHealth: 1
                ),
                logger: logger,
                mapManager: mapManager
            );
        }

        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IMapManager _mapManager;

        private int _health;
        private int _maxHealth;

        public IEntityManager EntityManager { get; private set; }

        public IBlockState State
            => new BlockState(
                IsAlive,
                Reason,
                Type,
                _health,
                _maxHealth
            );

        public bool IsAlive => _health > 0;
        public BlockReason Reason { get; private set; }
        public BlockType Type { get; private set; }
        public Vector2Int Position { get; }

        public Block(
            Vector2Int position,
            [NotNull] IBlockState initialState,
            [NotNull] ILogManager logger,
            [NotNull] IMapManager mapManager
        ) {
            _logger = logger;
            _mapManager = mapManager;
            _health = initialState.Health;
            _maxHealth = initialState.MaxHealth;
            Reason = initialState.Reason;
            Type = initialState.Type;
            Position = position;
        }

        public T GetComponent<T>() where T : IEntityComponent {
            throw new System.NotImplementedException();
        }

        public void ApplyState(IBlockState state) {
            Reason = state.Reason;
            Type = state.Type;
            _health = state.Health;
            _maxHealth = state.MaxHealth;
        }

        public void Kill() {
            Kill(BlockReason.Null);
        }

        public void Kill(BlockReason reason) {
            Reason = reason;
            _health = 0;
            _mapManager.RemoveBlock(this);
        }

        public void TakeDamage(int amount) {
            if (Type == BlockType.Hard) {
                return;
            }
            _health = Mathf.Max(0, _health - amount);
            _logger.Log($"[Block:damage] amount={amount} health={_health} x={Position.x} y={Position.y}");
        }

        public void Begin(IEntityManager entityManager) {
            EntityManager = entityManager;
        }

        public void Update(int delta) { }

        public void End() {
            EntityManager = null;
        }
    }
}