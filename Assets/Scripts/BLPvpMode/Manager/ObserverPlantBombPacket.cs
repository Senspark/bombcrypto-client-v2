using BLPvpMode.Engine.Entity;

using UnityEngine;

namespace BLPvpMode.Manager {
    public interface IObserverPlantBombPacket : ICommandPacket {
        int Slot { get; }
        int BombId { get; }
        Vector2Int Position { get; }
        BombReason Reason { get; }
    }

    public class ObserverPlantBombPacket : IObserverPlantBombPacket {
        public long Timestamp { get; set; }
        public int Slot { get; set; }
        public int BombId { get; set; }
        public Vector2Int Position { get; set; }
        public BombReason Reason { get; set; }
    }
}