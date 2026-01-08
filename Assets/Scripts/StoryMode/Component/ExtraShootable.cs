using Engine.Entities;

using UnityEngine;

namespace Engine.Components {
    public class ExtraShootable : DistanceShootable {

        private Shootable _shootable;
        private const int Numbombs = 2;

        public ExtraShootable(Entity entity, Updater updater, Shootable shootable) : base(entity, updater) {
            _shootable = shootable;
        }
        
        public void DoShoot() {
            var mapManager = Entity.EntityManager.MapManager;
            var location = mapManager.GetTileLocation(Entity.transform.localPosition);
            _shootable.ShootExtraAround(location, Numbombs);
        }
    }
}