using System;
using System.Collections.Generic;

using BLPvpMode.Engine.Info;

using Engine.Manager;
using Engine.Utils;

namespace BLPvpMode.Queue {
    public class FallingBlocksQueue {
        private readonly IEntityManager _entityManager;
        private readonly List<IFallingBlockInfo> _blocks;
        private int _index = 0;

        public FallingBlocksQueue(IEntityManager entityManager) {
            _entityManager = entityManager;
            _blocks = new List<IFallingBlockInfo>();
        }

        public void PushEventFalling(IFallingBlockInfo[] blocks) {
            _blocks.AddRange(blocks);
        }

        public void UpdateProcess(float delta, long roundStartTimestamp) {
            while (_blocks.Count > 0) {
                var block = _blocks[0];
                var timestamp = Epoch.GetUnixTimestamp(TimeSpan.TicksPerMillisecond);
                var timeAniBegin = block.Timestamp - 1000 + roundStartTimestamp;
                if (timestamp < timeAniBegin - 1000) {
                    break;
                }
                _blocks.RemoveAt(0);
                var delayToBegin = (timeAniBegin - timestamp) * 0.001f;
                _entityManager.MapManager.DropOneWall(_index++, block.Position.x, block.Position.y, delayToBegin);
                // Fix lag: Chỉ hiện 1 frame tạo ra tối đa 1 wallDrop 
                break;
            }
        }
    }
}