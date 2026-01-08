using App;
using Senspark;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.PvpModeScene.Scripts {
    public class PvpModeSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference pvpModeScenePrefab;

        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(pvpModeScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
            var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            soundManager.StopImmediateMusic();
            soundManager.PlayMusic(Audio.PvpMusic);
        }
    }
}