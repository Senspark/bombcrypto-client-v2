using System;
using System.Collections.Generic;
using System.Linq;

using BLPvpMode.Engine.Delta;
using BLPvpMode.Engine.Entity;
using BLPvpMode.Engine.Info;
using BLPvpMode.Engine.Utility;
using BLPvpMode.Manager;
using BLPvpMode.Manager.Api;

using JetBrains.Annotations;

using Senspark;

using UnityEngine;
using UnityEngine.Assertions;

namespace BLPvpMode.Engine.Manager {
    public class MapListener : IMapListener {
        [CanBeNull]
        public Action<IBlock, BlockReason> OnAdded { get; set; }

        [CanBeNull]
        public Action<IBlock, BlockReason> OnRemoved { get; set; }

        void IMapListener.OnAdded(IBlock block, BlockReason reason) {
            OnAdded?.Invoke(block, reason);
        }

        void IMapListener.OnRemoved(IBlock block, BlockReason reason) {
            OnRemoved?.Invoke(block, reason);
        }
    }

    public class MapManagerState : IMapManagerState {
        public static IMapManagerState Create(
            [NotNull] IMapInfo info,
            bool canDropChestBlock
        ) {
            return new MapManagerState(
                canDropChestBlock,
                info.Blocks.Associate(it =>
                    (it.Position, (IBlockState) new BlockState(
                        isAlive: true,
                        reason: BlockReason.Spawn,
                        type: it.BlockType,
                        health: it.Health,
                        maxHealth: it.Health
                    ))
                )
            );
        }

        [NotNull]
        public static IMapManagerState DecodeDelta([NotNull] IBlockStateDelta[] delta) {
            return new MapManagerState(
                canDropChestBlock: false,
                blocks: delta.Associate(it =>
                    (it.Position, BlockState.Decode(it.State))
                )
            );
        }

        public bool CanDropChestBlock { get; }
        public Dictionary<Vector2Int, IBlockState> Blocks { get; }

        public MapManagerState(
            bool canDropChestBlock,
            [NotNull] Dictionary<Vector2Int, IBlockState> blocks
        ) {
            CanDropChestBlock = canDropChestBlock;
            Blocks = blocks;
        }

        public IMapManagerState Apply(IMapManagerState state) {
            var items = Blocks.ToDictionary(it => it.Key, it => it.Value);
            state.Blocks.ForEach((key, value) => items[key] = value);
            return new MapManagerState(
                canDropChestBlock: state.CanDropChestBlock,
                blocks: items
            );
        }

        public long[] Encode() {
            return null; // FIXME.
            // return Blocks
            //     .Select(entry => entry.Value.Encode(entry.Key))
            //     .ToArray();
        }
    }

    public class DefaultMapManager : IMapManager {
        public static IMapManager CreateMap(
            [NotNull] IMapInfo info,
            [NotNull] ILogManager logger,
            [NotNull] ITimeManager timeManager,
            [NotNull] IRandom random,
            [NotNull] IMapListener listener
        ) {
            return new DefaultMapManager(
                tileset: info.Tileset,
                width: info.Width,
                height: info.Height,
                initialState: MapManagerState.Create(info, true),
                logger: logger,
                blockDropper: DefaultBlockDropper.Create(info, logger, random),
                timeManager: timeManager,
                listener: listener
            );
        }

        [NotNull]
        private readonly ILogManager _logger;

        [NotNull]
        private readonly IBlockDropper _blockDropper;

        [NotNull]
        private readonly IMapListener _listener;

        [NotNull]
        private readonly Dictionary<Vector2Int, IBlock> _items = new();

        [NotNull]
        private readonly Dictionary<Vector2Int, IBlock> _deadItems = new();

        private bool _canDropItem;

        public int Tileset { get; }
        public int Width { get; }
        public int Height { get; }
        public bool CanDropChestBlock { get; private set; }

        public IMapManagerState State
            => new MapManagerState(
                CanDropChestBlock,
                _items.Values.Concat(_deadItems.Values).ToDictionary(
                    it => it.Position,
                    it => it.State
                )
            );

        private DefaultMapManager(
            int tileset,
            int width,
            int height,
            [NotNull] IMapManagerState initialState,
            [NotNull] ILogManager logger,
            [NotNull] IBlockDropper blockDropper,
            [NotNull] ITimeManager timeManager,
            [NotNull] IMapListener listener
        ) {
            Tileset = tileset;
            Width = width;
            Height = height;
            CanDropChestBlock = initialState.CanDropChestBlock;
            _logger = logger;
            _blockDropper = blockDropper;
            _listener = listener;
            _canDropItem = false;
            initialState.Blocks.ForEach((key, state) => {
                var item = new Block(
                    position: key,
                    initialState: state,
                    logger: logger,
                    mapManager: this
                );
                if (state.IsAlive) {
                    AddBlock(item);
                } else {
                    RemoveBlock(item);
                }
            });
            _canDropItem = true;
        }

        public void ApplyState(IMapManagerState state) {
            _canDropItem = false;
            CanDropChestBlock = state.CanDropChestBlock;
            state.Blocks.ForEach((key, itemState) => {
                var item = new Block(
                    position: key,
                    initialState: itemState,
                    logger: _logger,
                    mapManager: this
                );
                if (itemState.IsAlive) {
                    if (_items.TryGetValue(key, out var currentItem)) {
                        if (currentItem.Reason == itemState.Reason) {
                            currentItem.ApplyState(itemState);
                            return;
                        }
                        var reason = itemState.Reason switch {
                            BlockReason.Dropped => BlockReason.Exploded,
                            BlockReason.Falling => BlockReason.Removed,
                            _ => throw new Exception("Invalid block reason"),
                        };
                        currentItem.Kill(reason);
                    }
                    AddBlock(item);
                } else {
                    if (_deadItems.TryGetValue(key, out var currentItem)) {
                        currentItem.ApplyState(itemState);
                    } else {
                        RemoveBlock(item);
                    }
                }
            });
            _canDropItem = true;
        }

        public IBlock GetBlock(Vector2Int position) {
            if ((position.x % 2 == 1 && position.y % 2 == 1) ||
                (position.x < 0 || position.y < 0 || position.x >= Width || position.y >= Height)) {
                return Block.CreateHardBlock(position, BlockReason.Null, _logger, this);
            }
            return _items.TryGetValue(position, out var result) ? result : null;
        }

        public void AddBlock(IBlock block) {
            Assert.IsNull(GetBlock(block.Position), $"Block exists at [{block.Position.x} {block.Position.y}]");
            _items[block.Position] = block;
            _deadItems.Remove(block.Position);
            _listener.OnAdded(block, block.Reason);
        }

        public void RemoveBlock(IBlock block) {
            _items.Remove(block.Position);
            _deadItems[block.Position] = block;
            _listener.OnRemoved(block, block.Reason);
            if (_canDropItem && block.IsBlock() && block.Reason == BlockReason.Exploded) {
                var droppedBlock = _blockDropper.Drop(this, block);
                if (droppedBlock != null) {
                    AddBlock(droppedBlock);
                    if (droppedBlock.IsChest()) {
                        CanDropChestBlock = false;
                    }
                }
            }
        }
    }
}