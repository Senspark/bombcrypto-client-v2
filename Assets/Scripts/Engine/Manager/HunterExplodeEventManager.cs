using System.Collections.Generic;

using UnityEngine;

namespace Engine.Manager {
    public class HunterExplodeEventManager : DefaultExplodeEventManager {
        public HunterExplodeEventManager(IEntityManager entityManager) : base(entityManager) {
        }

        protected override void AfterExplode(BombExplodeEvent explode, List<Vector2Int> brokenList,
            List<Vector2Int> itemsRemovedList) {
            // Send GenID, tileLocation, brokenList to Server...
            var tileLocation = EntityManager.MapManager.GetTileLocation(explode.Position);
            EntityManager.LevelManager.OnBombExploded(explode.OwnerId, explode.BombId, tileLocation,
                brokenList);
        }
    }
}