using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Share.Scripts.PrefabsManager {
    public abstract class TemplatePrefabLoader : MonoBehaviour, IPrefabLoader {
        public Dictionary<Type, AssetReference> Map { get; protected set; }

        public virtual void Initialize() {
            var manager = ServiceLocator.Instance.Resolve<IPrefabLoaderManager>();
            manager.RegisterPrefabLoader(this);
        }

        public bool Contains<T>() where T : MonoBehaviour {
            return Map.ContainsKey(typeof(T));
        }

        public async UniTask<T> Instantiate<T>() where T : MonoBehaviour {
            if (!Map.TryGetValue(typeof(T), out var assetReference)) {
                throw new Exception($"PrefabLoader: No asset reference found for type {typeof(T)}");
            }
            var handle = assetReference.InstantiateAsync();
            await handle.Task;
            return handle.Result.GetComponent<T>();
        }
        
        public void OnDestroy() {
            var manager = ServiceLocator.Instance.Resolve<IPrefabLoaderManager>();
            manager.UnregisterPrefabLoader(this);
        }
    }
}