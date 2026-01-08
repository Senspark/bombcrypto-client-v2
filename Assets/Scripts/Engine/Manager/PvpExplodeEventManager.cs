using System.Collections.Generic;

using Engine.Entities;
using Engine.Manager;

using PvpMode.Services;

using UnityEngine;
using UnityEngine.Assertions;

namespace BomberLand.Manager {
    public class PvpExplodeEventManager : DefaultExplodeEventManager {
        public PvpExplodeEventManager(IEntityManager entityManager) : base(entityManager) { }

        protected override void ProcessExplodeResult(float delta) {
            // delay để chờ các explosion liền kề sau đó.
            Elapse += delta;
            if (Elapse < 0.1f) {
                return;
            }
            Elapse = 0;

            var result = PopResult();
            while (result != null) {
                AfterExplode(result.Explode, result.BrokenList, result.RemovedItem);
                result = PopResult();
            }
        }

        protected override ResultExplode ExplodeBomb(BombExplodeEvent explode) {
            if (explode.IsShaking) {
                ShakeCamera();
            }

            var pos = explode.Position;
            var mapManager = EntityManager.MapManager;
            var tileLocation = EntityManager.MapManager.GetTileLocation(pos);
            var i = tileLocation.x;
            var j = tileLocation.y;

            var brokenList = new List<Vector2Int>();
            var itemsRemovedList = new List<Vector2Int>();
            var cellExplosions = new List<CellExplosion>();

            // Create Explosion
            //// Center
            mapManager.RemoveHadBomb(tileLocation);
            mapManager.RemoveBomb(tileLocation);

            var explodeAniData = CreateBombExplodeAni(explode);

            explodeAniData.Push(i, j, ExplosionPose.Center);

            cellExplosions.Add(new CellExplosion(i, j, explode.Damage));

            Assert.IsTrue(explode != null && explode.Ranges.Count == 4);
            var configs = new[] {
                (Direction.Left, -1, 0, ExplosionPose.MidleHori, ExplosionPose.EndLeft),
                (Direction.Right, +1, 0, ExplosionPose.MidleHori, ExplosionPose.EndRight),
                (Direction.Up, 0, +1, ExplosionPose.MidleVert, ExplosionPose.EndUp),
                (Direction.Down, 0, -1, ExplosionPose.MidleVert, ExplosionPose.EndDown),
            };
            // left, right, up, down.
            foreach (var (direction, ki, kj, midPose, endPose) in configs) {
                var range = explode.Ranges[direction];
                for (var k = 1; k <= range; ++k) {
                    var pose = k < range ? midPose : endPose;
                    var ii = i + k * ki;
                    var jj = j + k * kj;
                    explodeAniData.Push(ii, jj, pose);
                    cellExplosions.Add(new CellExplosion(ii, jj, explode.Damage));
                }
            }
            return new ResultExplode {
                Explode = explode,
                BrokenList = brokenList,
                RemovedItem = itemsRemovedList,
                CellExplosionList = cellExplosions
            };
        }

        protected override void AfterExplode(BombExplodeEvent explode, List<Vector2Int> brokenList,
            List<Vector2Int> itemsRemovedList) {
            // remove soft block, items
            foreach (var (x, y) in explode.BrokenList) {
                var location = new Vector2Int(x, y);
                if (!RemoveBlockAt(location)) {
                    RemoveItemAt(location);
                }
            }
        }

        private bool RemoveBlockAt(Vector2Int location) {
            return EntityManager.MapManager.BreakBrick(location.x, location.y);
        }

        private void RemoveItemAt(Vector2Int location) {
            EntityManager.MapManager.RemoveItem(location.x, location.y);
        }
    }
}