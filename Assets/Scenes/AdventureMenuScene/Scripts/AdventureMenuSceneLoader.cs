using App;
using Senspark;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.AdventureMenuScene.Scripts {
    public class AdventureMenuSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference adventureMenuScenePrefab;

        private async void Awake() {
            var obj = await AddressableLoader.LoadAsset<GameObject>(adventureMenuScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
            var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            soundManager.StopImmediateMusic();
            soundManager.PlayMusic(Audio.StoryMenuMusic);
        }
    }
}