using Engine.Components;
using Engine.Entities;
using Engine.Strategy.Provider;

namespace Engine.Strategy.Spawner {
    public class BombSpawner : ISpawner {
        public BombSpawner() { }

        public Entity Spawn(Spawnable spawnable) {
            var bombable = (Bombable) spawnable;
            var manager = bombable.Entity.EntityManager;

            var tileLocation = bombable.SpawnLocation;
            var position = manager.MapManager.GetTilePosition(tileLocation);
            var bomb = manager.MapManager.TryCreateEntityLocation(EntityType.Bomb, tileLocation);
            bomb.Type = EntityType.Bomb;

            var transform = bomb.transform;
            transform.SetParent(manager.View.transform, false);
            transform.localPosition = position;

            manager.MapManager.AddBomb(tileLocation, bombable.ExplosionLength);
            manager.AddEntity(bomb);

            return bomb;
        }
    }
}