using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.MarketplaceScene.Scripts {
    public class MarketplaceSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference marketplaceScenePrefab;
        
        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(marketplaceScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}