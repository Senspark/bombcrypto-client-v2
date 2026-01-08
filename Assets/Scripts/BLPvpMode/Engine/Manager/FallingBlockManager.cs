using System;
using System.Collections.Generic;

using BLPvpMode.Engine.Info;

using JetBrains.Annotations;

using UnityEngine;

namespace BLPvpMode.Engine.Manager {
    public interface IFallingBlockManagerListener {
        void OnBlockDidFall(Vector2Int position);
        void OnBuffered([NotNull] List<IFallingBlockInfo> blocks);
    }

    public interface IFallingBlockManager : IUpdater { }

    public class FallingBlockManagerListener : IFallingBlockManagerListener {
        [CanBeNull]
        public Action<Vector2Int> OnBlockDidFall { get; set; }

        [CanBeNull]
        public Action<List<IFallingBlockInfo>> OnBuffered { get; set; }

        void IFallingBlockManagerListener.OnBlockDidFall(Vector2Int position) {
            OnBlockDidFall?.Invoke(position);
        }

        void IFallingBlockManagerListener.OnBuffered(List<IFallingBlockInfo> blocks) {
            OnBuffered?.Invoke(blocks);
        }
    }

    public class FallingBlockManager : IFallingBlockManager {
        [NotNull]
        private readonly IFallingBlockInfo[] _blocks;

        [NotNull]
        private readonly IFallingBlockManagerListener _listener;

        private int _elapsed;
        private int _index;
        private int _bufferIndex;

        public FallingBlockManager(
            [NotNull] IFallingBlockInfo[] blocks,
            [NotNull] IFallingBlockManagerListener listener
        ) {
            _blocks = blocks;
            _listener = listener;
            _elapsed = 0;
            _index = 0;
            _bufferIndex = 0;
        }

        public void Step(int delta) {
            _elapsed += delta;
            while (_index < _blocks.Length) {
                var block = _blocks[_index];
                if (block.Timestamp > _elapsed) {
                    break;
                }
                _listener.OnBlockDidFall(block.Position);
                ++_index;
            }
            const int bufferTime = 5000;
            const int bufferCount = 3;
            while (_bufferIndex < _blocks.Length) {
                var block = _blocks[_bufferIndex];
                if (block.Timestamp > _elapsed + bufferTime) {
                    break;
                }
                var blocks = new List<IFallingBlockInfo>();
                for (var i = 0; i < bufferCount; ++i) {
                    if (_bufferIndex >= _blocks.Length) {
                        break;
                    }
                    blocks.Add(_blocks[_bufferIndex++]);
                }
                _listener.OnBuffered(blocks);
            }
        }
    }
}