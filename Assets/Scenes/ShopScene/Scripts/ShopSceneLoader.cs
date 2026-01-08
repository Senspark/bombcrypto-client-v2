using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.ShopScene.Scripts {
    public class ShopSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference shopScenePrefab;

        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(shopScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}