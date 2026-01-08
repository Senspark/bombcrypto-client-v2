using UnityEngine;

namespace BLPvpMode.Engine.Strategy.Position {
    public interface IPositionStrategy {
        Vector2 GetPosition(int timestamp);
        void AddPosition(int timestamp, Vector2 position);
    }
}