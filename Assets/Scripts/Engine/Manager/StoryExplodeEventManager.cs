using System.Collections.Generic;
using System.Linq;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace Engine.Manager {
    public class StoryExplodeEventManager : DefaultExplodeEventManager {
        public StoryExplodeEventManager(IEntityManager entityManager) : base(entityManager) { }

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
            while (result != null) {
                totalBrokenList.AddRange(result.BrokenList);
                totalRemovedItem.AddRange(result.RemovedItem);
                result = PopResult();
            }
            AfterExplode(null, totalBrokenList, totalRemovedItem);
        }

        protected override void AfterExplode(BombExplodeEvent explode, List<Vector2Int> brokenList,
            List<Vector2Int> itemsRemovedList) {
            if (!GameConstant.AdventureRequestServer) {
                BreakBrickAndRemoveItems(brokenList.ToArray(), itemsRemovedList.ToArray());
            }

            var storyModeManager = ServiceLocator.Instance.Resolve<IStoryModeManager>();
            var entityManager = EntityManager;

            // Add itemsRemoved to brokenList
            var effectList = GenerateEffectList(brokenList, itemsRemovedList);

            if (effectList.Count <= 0) {
                return;
            }

            UniTask.Void(async () => {
                var result =
                    await storyModeManager.StartExplode(effectList);
                OnStartExplode(entityManager, result.BrokenLocation, result.ItemsRemoved);
                entityManager.LevelManager.GenerateEnemiesFromDoor(result.EnemiesFromDoor);
            });
        }

        private static List<Vector2Int> GenerateEffectList(List<Vector2Int> brokenList, List<Vector2Int> itemsRemoved) {
            var effectList = new List<Vector2Int>();
            foreach (var iter in brokenList.Where(iter => !effectList.Contains(iter))) {
                effectList.Add(iter);
            }
            foreach (var iter in itemsRemoved.Where(iter => !effectList.Contains(iter))) {
                effectList.Add(iter);
            }
            return effectList;
        }

        private void BreakBrickAndRemoveItems(Vector2Int[] brokenList, Vector2Int[] itemsRemoved) {
            foreach (var location in itemsRemoved) {
                EntityManager.MapManager.RemoveItem(location.x, location.y);
            }
            foreach (var location in brokenList) {
                EntityManager.MapManager.BreakBrick(location.x, location.y);
            }
        }

        private void OnStartExplode(IEntityManager entityManager, Vector2Int[] brokens, Vector2Int[] itemsRemoved) {
            foreach (var location in brokens) {
                entityManager.MapManager.BreakBrick(location.x, location.y);
            }
            foreach (var location in itemsRemoved) {
                entityManager.MapManager.RemoveItem(location.x, location.y);
            }
        }
    }
}