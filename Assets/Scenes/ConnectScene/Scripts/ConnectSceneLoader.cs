using App;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Scenes.ConnectScene.Scripts {
    public class ConnectSceneLoader : MonoBehaviour, ILoader {
        [SerializeField]
        private AssetReference connectSceneTon;

        [SerializeField]
        private AssetReference connectSceneWebGL;

        [SerializeField]
        private AssetReference connectSceneSolana;

        public void Initialize()
        {
            AssetReference prefab;
            switch (AppConfig.GamePlatform) {
                case GamePlatform.TON:
                    prefab = connectSceneTon;
                    break;
                case GamePlatform.WEBGL:
                    prefab = connectSceneWebGL;
                    break;
                case GamePlatform.SOL:
                    prefab = connectSceneSolana;
                    break;
                default:
                    prefab = connectSceneWebGL;
                    break;
            }
            
            prefab.InstantiateAsync(Vector3.zero, Quaternion.identity);
            Destroy(gameObject); // self destruct
        }
    }
}