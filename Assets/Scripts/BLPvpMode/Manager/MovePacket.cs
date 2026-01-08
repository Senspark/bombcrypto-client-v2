using PvpMode.Services;

using UnityEngine;

namespace BLPvpMode.Manager {
    public interface IMovePacket : ICommandPacket {
        Vector2 Position { get; }
        Direction Direction { get; }
    }

    public class MovePacket : IMovePacket {
        public long Timestamp { get; set; }
        public Vector2 Position { get; set; }
        public Direction Direction { get; set; }
    }
}