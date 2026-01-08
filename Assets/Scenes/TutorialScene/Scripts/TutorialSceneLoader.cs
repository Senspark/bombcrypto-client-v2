using Animation;

using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.TutorialScene.Scripts {
    public class TutorialSceneLoader : MonoBehaviour {
        [SerializeField]
        public AssetReference animationResource;
        [SerializeField]
        private AssetReference tutorialScenePrefab;

        private async void Awake() {
            var res = await AddressableLoader.LoadAsset<AnimationResource>(animationResource);
            //Init scriptable object cho animation trước khi load scene
            await res.LoadDataFirstForWeb();
            
            var obj = await AddressableLoader.LoadAsset<GameObject>(tutorialScenePrefab);
            SceneLoader.InstantiateAsync(obj);
            Destroy(gameObject);
        }
    }
}