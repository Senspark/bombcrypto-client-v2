using Senspark;

using Share.Scripts.PrefabsManager;

using UnityEngine;

using Utils;

namespace Share.Scripts.Services {
    public static class FirstClassServices {
        private static bool _isInitialized;

        /**
         * Đăng ký các service luôn có trước
         */
        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            if (_isInitialized) {
                return;
            }
            _isInitialized = true;
            var prefabLoaderManager = new PrefabLoaderManager();
            ServiceLocator.Instance.Provide(prefabLoaderManager);
            
            prefabLoaderManager.Initialize().Forget();
        }
    }
}