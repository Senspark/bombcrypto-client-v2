using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.DailyGiftScene.Scripts {
    public class DailyGiftSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference dailyGiftScenePrefab;
        
        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(dailyGiftScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}