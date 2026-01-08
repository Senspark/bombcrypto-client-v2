using Cysharp.Threading.Tasks;

using Engine.Entities;
using Engine.Manager;

using UnityEngine;
using UnityEngine.Assertions;

namespace Engine.Strategy.Provider {
    public class PoolableProvider : IProvider {
        private Entity _prefab;

        public PoolableProvider() {
            
        }

        public async UniTask<PoolableProvider> GetProvider(string path) {
            var obj  = await AddressableLoader.LoadAsset<GameObject>(path);
            _prefab = obj.GetComponent<Entity>();
            return this;
        }

        public PoolableProvider(Entity prefab) {
            _prefab = prefab;
            Assert.IsTrue(_prefab != null);
        }

        public PoolableProvider(string path) {
            _prefab = Resources.Load<Entity>(path);
            Assert.IsTrue(_prefab != null);
        }

        public Entity CreateInstance(IEntityManager manager) {
            var poolManager = manager.PoolManager;
            var instance = poolManager.Instantiate(_prefab);
            return instance;
        }

        public void Reserve(IEntityManager manager, int capacity) {
            var poolManager = manager.PoolManager;
            poolManager.Reserve(_prefab, capacity);
        }
    }
}