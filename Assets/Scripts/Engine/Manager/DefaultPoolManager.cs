using System.Collections.Generic;

using Engine.Components;
using Engine.Entities;
using Engine.Utils;

using UnityEngine.Assertions;

namespace Engine.Manager {
    public class DefaultPoolManager : IPoolManager {
        private readonly Dictionary<string, IObjectPool<Entity>> pools;

        public DefaultPoolManager() {
            pools = new Dictionary<string, IObjectPool<Entity>>();
        }

        public T Instantiate<T>(T prefab) where T : Entity {
            var poolable = prefab.GetComponent<Poolable>();
            if (poolable == null) {
                return UnityEngine.Object.Instantiate(prefab);
            }
            var key = poolable.PoolId;
            var instance = Instantiate(key, prefab);
            instance.GetComponent<Poolable>().InstantiatedFromPool = true;
            instance.Resurrect();
            return instance;
        }

        public void Destroy<T>(T instance) where T : Entity {
            var poolable = instance.GetComponent<Poolable>();
            if (poolable.InstantiatedFromPool) {
                var key = poolable.PoolId;
                Destroy(key, instance);
            } else {
                UnityEngine.Object.Destroy(instance);
            }
        }

        public void Reserve<T>(T prefab, int capacity) where T : Entity {
            var poolable = prefab.GetComponent<Poolable>();
            Assert.IsTrue(poolable != null, $"Not has poolable {prefab.name}");

            var key = poolable.PoolId;
            if (!pools.ContainsKey(key)) {
                pools.Add(key, new ComponentPool<Entity>(prefab));
            }
            pools[key].Reserve(capacity);
        }

        private T Instantiate<T>(string key, T prefab) where T : Entity {
            if (!pools.ContainsKey(key)) {
                pools.Add(key, new ComponentPool<Entity>(prefab));
            }
            var pool = pools[key];
            var instance = pool.Instantiate();
            return instance as T;
        }

        private void Destroy<T>(string key, T instance) where T : Entity {
            var pool = pools[key];
            pool.Destroy(instance);
        }
    }
}