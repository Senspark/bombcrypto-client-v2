using System.Collections.Generic;

using Engine.Manager;

using UnityEngine;

namespace BLPvpMode.Test {
    public class SimulatorExplodeEventManager : DefaultExplodeEventManager {
        public struct ExplodeCallback {
            public System.Action<List<CellExplosion>> TakeDamageFromExplosions;
        }

        private readonly ExplodeCallback _explodeCallback;

        public SimulatorExplodeEventManager(IEntityManager entityManager, ExplodeCallback callback) : base(
            entityManager) {
            _explodeCallback = callback;
        }

        protected override void ProcessExplodeResult(float delta) {
            // delay để chờ các explosion liền kề sau đó.
            Elapse += delta;
            if (Elapse < 0.1f) {
                return;
            }
            Elapse = 0;

            var result = PopResult();
            if (result == null) {
                return;
            }

            var totalBrokenList = new List<Vector2Int>();
            var totalRemovedItem = new List<Vector2Int>();
            var cellExplosionList = new List<CellExplosion>();
            while (result != null) {
                totalBrokenList.AddRange(result.BrokenList);
                totalRemovedItem.AddRange(result.RemovedItem);
                cellExplosionList.AddRange(result.CellExplosionList);
                result = PopResult();
            }
            AfterExplode(null, totalBrokenList, totalRemovedItem);
            TakeDamageFromCellExplosion(cellExplosionList);
        }

        protected override void AfterExplode(BombExplodeEvent explode, List<Vector2Int> brokenList,
            List<Vector2Int> itemsRemovedList) {
            BreakBrickAndRemoveItems(brokenList.ToArray(), itemsRemovedList.ToArray());
        }

        private void TakeDamageFromCellExplosion(List<CellExplosion> cellExplosion) {
            _explodeCallback.TakeDamageFromExplosions(cellExplosion);
        }

        private void BreakBrickAndRemoveItems(Vector2Int[] brokenList, Vector2Int[] itemsRemoved) {
            foreach (var location in itemsRemoved) {
                EntityManager.MapManager.RemoveItem(location.x, location.y);
            }
            foreach (var location in brokenList) {
                EntityManager.MapManager.BreakBrick(location.x, location.y);
            }
        }
    }
}