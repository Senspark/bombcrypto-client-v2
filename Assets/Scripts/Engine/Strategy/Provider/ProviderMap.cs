using System.Collections.Generic;

using App;

using Engine.Entities;
using Engine.Manager;

using UnityEngine;

namespace Engine.Strategy.Provider {
    public class ProviderMap : IProviderMap {
        private static string GetEntityPath(EntityType entityType, GameModeType gameMode) {
            return entityType switch {
                EntityType.Door => "Prefabs/Items/Door",
                EntityType.Item => "Prefabs/Items/Item",
                EntityType.Brick => "Prefabs/Entities/Brick",
                EntityType.NftChest => "Prefabs/Entities/Brick",
                EntityType.Prison => "Prefabs/Items/Prison",
                EntityType.Boss => "Prefabs/Enemies/Boss",
                EntityType.Bomb => gameMode == GameModeType.PvpMode
                    ? "Prefabs/PvpMode/Entities/PvpBomb"
                    : "Prefabs/Entities/Bomb",
                EntityType.BombExplosion => "Prefabs/Entities/BombExplosion",
                EntityType.WallDrop => "Prefabs/Entities/WallDrop",
                EntityType.Fire => "Prefabs/Entities/Fire",
                EntityType.Enemy => "Prefabs/Enemies/Enemy",
                _ => null // not support
            };
        }

        private static IProvider CreatePoolProvider(EntityType type, GameModeType gameMode) {
            var path = GetEntityPath(type, gameMode);
            return path == null ? null : new PoolableProvider(path);
        }

        private readonly IEntityManager _entityManager;
        private readonly IDictionary<EntityType, IProvider> _hashIProvider;

        public ProviderMap(IEntityManager entityManager) {
            _entityManager = entityManager;
            _hashIProvider = new Dictionary<EntityType, IProvider>();
            InitCache();
        }

        private void InitCache() {
            var cacheList = new List<(EntityType type, int capacity)>() {
                (EntityType.Item, 12), //
                (EntityType.Brick, 12), //
                // (EntityType.Bomb, 12), //
                (EntityType.BombExplosion, 32), //
            };
            foreach (var cache in cacheList) {
                CreatePoolProvider(cache.type, _entityManager.LevelManager.GameMode)
                    .Reserve(_entityManager, cache.capacity);
            }
        }

        public Entity TryCreateEntityLocation(EntityType type, out EntityLocation entityLocation) {
            var result = CreateEntity(type);
            entityLocation = result as EntityLocation;
            return result;
        }

        public Entity CreateEntity(EntityType type) {
            if (_hashIProvider.ContainsKey(type)) {
                return _hashIProvider[type].CreateInstance(_entityManager);
            }
            var provider = CreatePoolProvider(type, _entityManager.LevelManager.GameMode);
            if (provider == null) {
                //Debug.LogWarning($"Not support type {type}");
                return null;
            }
            _hashIProvider[type] = provider;
            return _hashIProvider[type].CreateInstance(_entityManager);
        }
    }
}