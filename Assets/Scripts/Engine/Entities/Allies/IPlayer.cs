using App;

using Engine.Components;

using UnityEngine;

namespace Engine.Entities {
    public interface IPlayer {
        HeroId HeroId { get; }
        Health Health { get; }
        Movable Movable { get; }
        WalkThrough WalkThrough { get; }
        Bombable Bombable { get; }

        Vector3 GetPosition();
        void SetPosition(Vector3 position);
        Vector2Int GetTileLocation();

    }
}
