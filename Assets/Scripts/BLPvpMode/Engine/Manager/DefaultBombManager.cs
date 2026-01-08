using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Strategy.Map;
using BLPvpMode.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using PvpMode.Services;

using Senspark;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Manager {
    public class BombListener : IBombListener {
        [CanBeNull]
        public Action<IBomb, BombReason> OnAdded { get; set; }

        [CanBeNull]
        public Action<IBomb, BombReason> OnRemoved { get; set; }

        [CanBeNull]
        public Action<IBomb, Dictionary<Direction, int>> OnExploded { get; set; }

        [CanBeNull]
        public Action<Vector2Int, int> OnDamaged { get; set; }

        void IBombListener.OnAdded(IBomb bomb, BombReason reason) {
            OnAdded?.Invoke(bomb, reason);
        }

        void IBombListener.OnRemoved(IBomb bomb, BombReason reason) {
            OnRemoved?.Invoke(bomb, reason);
        }

        void IBombListener.OnExploded(IBomb bomb, Dictionary<Direction, int> ranges) {
            OnExploded?.Invoke(bomb, ranges);
        }

        void IBombListener.OnDamaged(Vector2Int position, int amount) {
            OnDamaged?.Invoke(position, amount);
        }
    }

    public class BombManagerState : IBombManagerState {
        [NotNull]
        public static IBombManagerState DecodeDelta([NotNull] IBombStateDelta[] delta) {
            return new BombManagerState(
                bombCounter: 0,
                bombs: delta.Associate(
                    it => (it.Id, BombState.Decode(it.State))
                )
            );
        }

        public int BombCounter { get; }
        public Dictionary<int, IBombState> Bombs { get; }

        public BombManagerState(
            int bombCounter,
            [NotNull] Dictionary<int, IBombState> bombs
        ) {
            BombCounter = bombCounter;
            Bombs = bombs;
        }

        public IBombManagerState Apply(IBombManagerState state) {
            var items = Bombs.ToDictionary(it => it.Key, it => it.Value);
            state.Bombs.ForEach((key, value) => items[key] = value);
            return new BombManagerState(
                bombCounter: state.BombCounter,
                bombs: items
            );
        }
    }

    public class DefaultBombManager : IBombManager {
        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IMapManager _mapManager;

        [NotNull]
        private readonly ITimeManager _timeManager;

        [NotNull]
        private readonly IBombListener _listener;

        [NotNull]
        private readonly IExpandStrategy _expandStrategy = new InstantExpandStrategy();

        [NotNull]
        private readonly IExplodeRangeStrategy _explodeRangeStrategy = new LinearExplodeRangeStrategy();

        [NotNull]
        private readonly Dictionary<Vector2Int, IBomb> _itemByPosition = new();

        private readonly Dictionary<int, IBomb> _itemById = new();

        [NotNull]
        private readonly Dictionary<int, List<IBomb>> _itemsBySlot = new();

        [NotNull]
        private readonly SortedDictionary<int, IBomb> _deadItems = new();

        private int _bombCounter;

        public IBombManagerState State
            => new BombManagerState(
                bombCounter: _bombCounter,
                bombs: _itemById.Values.Concat(_deadItems.Values)
                    .ToDictionary(
                        it => it.Id,
                        it => it.State
                    )
            );

        public DefaultBombManager(
            [NotNull] IBombManagerState initialState,
            [NotNull] ILogManager logger,
            [NotNull] IMapManager mapManager,
            [NotNull] ITimeManager timeManager,
            [NotNull] IBombListener listener
        ) {
            _logger = logger;
            _mapManager = mapManager;
            _timeManager = timeManager;
            _listener = listener;
            _bombCounter = initialState.BombCounter;
            initialState.Bombs.ForEach((key, state) => {
                var bomb = new Bomb(
                    id: key,
                    initialState: state,
                    bombManager: this,
                    timeManager: _timeManager
                );
                if (state.IsAlive) {
                    AddBomb(bomb);
                } else {
                    RemoveBomb(bomb);
                }
            });
        }

        public void ApplyState(IBombManagerState state) {
            state.Bombs.ForEach((key, itemState) => {
                var item = new Bomb(
                    id: key,
                    initialState: itemState,
                    bombManager: this,
                    timeManager: _timeManager
                );
                if (itemState.IsAlive) {
                    if (_itemById.TryGetValue(key, out var currentItem)) {
                        currentItem.ApplyState(itemState);
                    } else {
                        AddBomb(item);
                    }
                } else {
                    if (_deadItems.TryGetValue(key, out var currentItem)) {
                        currentItem.ApplyState(itemState);
                    } else {
                        if (_itemById.ContainsKey(item.Id)) {
                            RemoveBomb(item);
                            if (item.Reason == BombReason.Exploded) {
                                _listener.OnExploded(item, item.State.ExplodeRanges);
                            }
                        } else {
                            // Removed since time-out (see Step).
                        }
                    }
                }
            });
        }

        public List<IBomb> GetBombs(int slot) {
            return _itemsBySlot.TryGetValue(slot, out var value) ? value : new List<IBomb>();
        }

        public IBomb GetBomb(Vector2Int position) {
            return _itemByPosition.TryGetValue(position, out var value) ? value : null;
        }

        public IBomb PlantBomb(IBombState state) {
            var bomb = new Bomb(
                id: _bombCounter,
                initialState: state,
                bombManager: this,
                timeManager: _timeManager
            );
            AddBomb(bomb);
            ++_bombCounter;
            return bomb;
        }

        public void AddBomb(IBomb bomb) {
            var position = bomb.Position;
            var positionInt = new Vector2Int(
                Mathf.FloorToInt(position.x),
                Mathf.FloorToInt(position.y));
            Assert.IsNull(GetBomb(positionInt), $"Bomb existed at x={positionInt.x} y={positionInt.y}");
            _itemByPosition[positionInt] = bomb;
            _itemById[bomb.Id] = bomb;
            if (_itemsBySlot.TryGetValue(bomb.Slot, out var bombs)) {
                bombs.Add(bomb);
            } else {
                _itemsBySlot[bomb.Slot] = new List<IBomb> { bomb };
            }
            _deadItems.Remove(bomb.Id);
            _listener.OnAdded(bomb, bomb.Reason);
        }

        public void RemoveBomb(IBomb bomb) {
            var position = bomb.Position;
            var positionInt = new Vector2Int(
                Mathf.FloorToInt(position.x),
                Mathf.FloorToInt(position.y));
            _itemByPosition.Remove(positionInt);
            _itemById.Remove(bomb.Id);
            _itemsBySlot[bomb.Slot].Remove(bomb);
            _deadItems[bomb.Id] = bomb;
            _listener.OnRemoved(bomb, bomb.Reason);
        }

        public void ExplodeBomb(IBomb bomb) {
            var destroyedBlocks = new List<IBlock>();
            var expandResult = _expandStrategy.Expand(this, _mapManager, bomb);
            expandResult.DamagedPositions.ForEach((position, damage) => {
                _listener.OnDamaged(position, damage);
                var block = _mapManager.GetBlock(position);
                if (block != null) {
                    block.TakeDamage(damage);
                    if (!block.IsAlive) {
                        destroyedBlocks.Add(block);
                    }
                }
            });
            expandResult.ExplodedBombs.ForEach(it => {
                it.Kill(BombReason.Exploded);
                _listener.OnExploded(it, it.State.ExplodeRanges);
            });
            Assert.IsTrue(destroyedBlocks.Count == destroyedBlocks.Distinct().Count());
            Assert.IsTrue(destroyedBlocks.All(it => !it.IsAlive));
            destroyedBlocks.ForEach(it =>
                it.Kill(BlockReason.Exploded)
            );
        }

        public Dictionary<Direction, int> GetExplodeRanges(IBomb bomb) {
            var positionInt = new Vector2Int(
                Mathf.FloorToInt(bomb.Position.x),
                Mathf.FloorToInt(bomb.Position.y));
            return new[] {
                Direction.Left, //
                Direction.Right, // 
                Direction.Up, //
                Direction.Down,
            }.AssociateWith(it =>
                _explodeRangeStrategy.GetExplodeRange(
                    _mapManager,
                    positionInt,
                    bomb.Range,
                    bomb.Piercing,
                    it
                )
            );
        }

        public void ThrowBomb(IBomb bomb, Direction direction, int distance, int duration) {
            throw new NotImplementedException();
        }

        public void Step(int delta) {
            const int maxAge = 60000;
            var thresholdTimestamp = _timeManager.Timestamp - maxAge;
            while (_deadItems.Count > 0) {
                var key = _deadItems.First().Key;
                var item = _deadItems[key];
                if (item.PlantTimestamp >= thresholdTimestamp) {
                    break;
                }
                _deadItems.Remove(key);
            }
            _itemByPosition.Values.ToList().ForEach(it =>
                it.Update(delta)
            );
        }
    }
}