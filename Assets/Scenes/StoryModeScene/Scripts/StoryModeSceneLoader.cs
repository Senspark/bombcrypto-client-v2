using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.StoryModeScene.Scripts {
    public class StoryModeSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference storyModeScenePrefab;
        
        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(storyModeScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}