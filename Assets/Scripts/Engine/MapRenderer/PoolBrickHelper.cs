using System;

using Engine.Entities;
using Engine.Manager;
using Engine.Strategy.Provider;

using UnityEngine;

namespace Engine.MapRenderer {
    public class PoolBrickHelper {
        private const string BrickPath = "Prefabs/Entities/Brick";
        private readonly IEntityManager _entityManager;
        private readonly Brick _brickPrefab;

        public PoolBrickHelper(IEntityManager entityManager, int tileIndex) {
            _entityManager = entityManager;
            var brick = Resources.Load<Entity>(BrickPath) as Brick ??
                        throw new Exception("Could not cast to brick");
            brick.Init(_entityManager.LevelManager.GameMode, tileIndex);
            _entityManager.PoolManager.Reserve(brick, 10);
            _brickPrefab = brick;
        }

        public void ShowEntityBreaking(TileType tileType, int i, int j) {
            var entity = FindEntityBreaking(tileType);
            var position = _entityManager.MapManager.GetTilePosition(i, j);

            var brickTransform = entity.transform;
            brickTransform.SetParent(_entityManager.View.transform, false);
            brickTransform.localPosition = position;
            _entityManager.AddEntity(entity);
            if (entity is Brick brick) {
                brick.PlayBroken(null);
            }
        }

        private Entity FindEntityBreaking(TileType tileType) {
            switch (tileType) {
                case TileType.Brick:
                    var brickProvider = new PoolableProvider(_brickPrefab);
                    var brick = brickProvider.CreateInstance(_entityManager) as Brick ??
                                throw new Exception("Could not cast to brick");
                    brick.CopyAnimatorFrom(_brickPrefab);
                    return brick;
                case TileType.NftChest:
                    var chestProvider = new PoolableProvider(BrickPath);
                    return chestProvider.CreateInstance(_entityManager);
            }
            throw new Exception($"Could not find entity {tileType} breaking");
        }
        
    }
}