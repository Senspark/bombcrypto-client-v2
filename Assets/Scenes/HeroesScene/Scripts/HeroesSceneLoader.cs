using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.HeroesScene.Scripts {
    public class HeroesSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference heroesScenePrefab;
        
        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(heroesScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}