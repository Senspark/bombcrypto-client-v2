using Engine.Entities;

using UnityEngine;

namespace Engine.Manager {
    public interface ITakeDamageEventManager {
        void PushEvent(int idxPlayer, int killerId, Entity dealer, Vector2Int lastPlayerLocation);
        void UpdateProcess();
    }
}