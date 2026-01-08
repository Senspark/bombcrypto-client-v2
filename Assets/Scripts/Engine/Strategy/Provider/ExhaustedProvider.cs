using Cysharp.Threading.Tasks;

using Engine.Entities;
using Engine.Manager;

using UnityEngine;
using UnityEngine.Assertions;

namespace Engine.Strategy.Provider
{
    public class ExhaustedProvider : IProvider
    {
        private Entity _prefab;

        public ExhaustedProvider() {
        }
        
        public async UniTask<ExhaustedProvider> GetExhaustedProvider(string path) {
            var obj = await AddressableLoader.LoadAsset<GameObject>(path);
            _prefab = obj.GetComponent<Entity>();
            return this;
        }
        public ExhaustedProvider(string path)
        {
            _prefab = Resources.Load<Entity>(path);
            Assert.IsTrue(_prefab != null);
        }
        
        public ExhaustedProvider(Entity prefab) {
            _prefab = prefab;
            Assert.IsTrue(_prefab != null);
        }

        public Entity CreateInstance(IEntityManager manager)
        {
            return Object.Instantiate(_prefab);
        }

        public void Reserve(IEntityManager manager, int capacity) {
            throw new System.NotImplementedException();
        }
    }
}