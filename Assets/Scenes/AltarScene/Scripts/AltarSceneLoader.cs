using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.AltarScene.Scripts {
    public class AltarSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference altarScenePrefab;

        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(altarScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}