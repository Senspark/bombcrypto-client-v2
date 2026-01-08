using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using UnityEngine;

namespace Share.Scripts.PrefabsManager {
    public class PrefabLoaderManager : IPrefabLoaderManager {
        private IPrefabLoader _currentLoader;
        private IPrefabLoader _shareLoader;
        private bool _initialized;

        public async Task<bool> Initialize() {
            _shareLoader = await new SharePrefabLoaderWrapper().Load();
            _initialized = true;
            return true;
        }

        public void Destroy() {
        }

        public void RegisterPrefabLoader(IPrefabLoader loader) {
            if(loader is SharedPrefabLoader) {
                return;
            }
            _currentLoader = loader;
        }

        public void UnregisterPrefabLoader(IPrefabLoader loader) {
            if (_currentLoader == loader) {
                _currentLoader = null;
            }
        }

        public async UniTask<T> Instantiate<T>() where T : MonoBehaviour {
            if (_initialized == false) {
                await UniTask.WaitUntil(() => _initialized);
            }
            if (_currentLoader != null && _currentLoader.Contains<T>()) {
                return await _currentLoader.Instantiate<T>();
            }
            if (_shareLoader.Contains<T>()) {
                return await _shareLoader.Instantiate<T>();
            }
            throw new System.Exception($"PrefabLoaderManager: No asset reference found for type {typeof(T)}");
        }
    }
}