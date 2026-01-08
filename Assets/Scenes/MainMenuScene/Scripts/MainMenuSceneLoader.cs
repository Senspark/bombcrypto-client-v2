using Animation;
using App;
using Senspark;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.MainMenuScene.Scripts {
    public class MainMenuSceneLoader : MonoBehaviour {
        [SerializeField]
        public AssetReference animationResource;

        [SerializeField]
        private AssetReference mainMenuPrefab;
        
        private async void Awake() {
            var res = await AddressableLoader.LoadAsset<AnimationResource>(animationResource);
            //Init scriptable object cho animation trước khi load scene
            await res.LoadDataFirstForWeb();
            var obj = await AddressableLoader.LoadAsset<GameObject>(mainMenuPrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
            var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            soundManager.StopImmediateMusic();
            soundManager.PlayMusic(Audio.MainMenuMusic);
        }
    }
}