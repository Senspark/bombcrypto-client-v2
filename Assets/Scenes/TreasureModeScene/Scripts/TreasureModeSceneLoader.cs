using Animation;
using App;
using Cysharp.Threading.Tasks;
using Game.UI;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace Scenes.TreasureModeScene.Scripts {
    public class TreasureModeSceneLoader : MonoBehaviour, ILoader {
        public ServiceMock serviceMock;
        public AssetReference animationResource;
        public AssetReference sceneTonPrefab;
        public AssetReference sceneSolanaPrefab;
        public AssetReference sceneWebAirdropPrefab;

        public async void Initialize() {
            serviceMock.Initialize();
            var res = await AddressableLoader.LoadAsset<AnimationResource>(animationResource);
            //Init scriptable object cho animation trước khi load scene
            if (AppConfig.IsTon()) {
                await res.LoadDataFirstForTon();
            } else {
                await res.LoadDataFirstForWeb();
            }
            LevelScene.IsLoadMapDone = false;
            var obj = await AddressableLoader.LoadAsset<GameObject>(GetScenePrefab());
            SceneLoader.InstantiateAsync(obj, UniTask.WaitUntil(() => LevelScene.IsLoadMapDone));
            Destroy(gameObject);
        }
        
        private AssetReference GetScenePrefab() {
            switch (AppConfig.GamePlatform) {
                case GamePlatform.TON:
                    return sceneTonPrefab;
                case GamePlatform.SOL:
                    return sceneSolanaPrefab;
                case GamePlatform.WEBGL:
                    if (AppConfig.IsWebAirdrop()) {
                        return sceneWebAirdropPrefab;
                    }
                    throw new System.Exception("Invalid game platform");
                default:
                    throw new System.Exception("Invalid game platform");
            }
        }
    }
}