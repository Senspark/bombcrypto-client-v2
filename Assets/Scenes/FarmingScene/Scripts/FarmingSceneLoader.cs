using Animation;
using App;
using Cysharp.Threading.Tasks;
using Game.UI;
using Senspark;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.FarmingScene.Scripts {
    public class FarmingSceneLoader : MonoBehaviour {
        [SerializeField]
        private AssetReference animationResource;

        [SerializeField]
        private AssetReference farmingScenePrefab;

        private async void Awake() {
            var res = await AddressableLoader.LoadAsset<AnimationResource>(animationResource);
            //Init scriptable object cho animation trước khi load scene
            await res.LoadDataFirstForWeb();
            var obj = await AddressableLoader.LoadAsset<GameObject>(farmingScenePrefab);
            LevelScene.IsLoadMapDone = false;
            SceneLoader.InstantiateAsync(obj, UniTask.WaitUntil(() => LevelScene.IsLoadMapDone));
            Destroy(gameObject);
            var soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            soundManager.StopImmediateMusic();
            soundManager.PlayMusic(Audio.TreasureMusic);
        }
    }
}