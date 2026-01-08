using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace Share.Scripts.PrefabsManager {
    [Service(nameof(IPrefabLoaderManager))]
    public interface IPrefabLoaderManager : IService {
        void RegisterPrefabLoader(IPrefabLoader loader);
        void UnregisterPrefabLoader(IPrefabLoader loader);
        UniTask<T> Instantiate<T>() where T : MonoBehaviour;
    }
}