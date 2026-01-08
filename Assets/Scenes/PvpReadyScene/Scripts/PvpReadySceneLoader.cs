using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.PvpReadyScene.Scripts {
    public class PvpReadySceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference pvpReadyScenePrefab;

        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(pvpReadyScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}