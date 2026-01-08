using Engine.Components;
using Engine.Entities;
using Engine.Strategy.Provider;

namespace Engine.Strategy.Shooter {
    public class BombShooter : IShooter {
        public BombShooter() { }

        public Entity Shoot(Shootable shootable) {
            var manager = shootable.EntityManager;

            var position = shootable.ShootLocation;

            var bomb = manager.MapManager.CreateEntity(EntityType.Bomb);
            bomb.Type = EntityType.Bomb;

            var transform = bomb.transform;
            transform.SetParent(manager.View.transform, false);
            transform.localPosition = position;

            manager.AddEntity(bomb);

            return bomb;
        }
    }
}