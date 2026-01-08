using System;

using Castle.Core.Internal;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

using Object = UnityEngine.Object;


namespace Share.Scripts.Utils {
    public static class SceneLoader
    {
        public static bool IsLoading { get; private set; }
        private static string _currentSceneName;
        private static string _newSceneName;
        private static bool _isReloadScene;
        
        private static Action<GameObject> _onLoaded;
        private static bool _newSceneLoaded, _isMoveToNewSceneComplete;

        private static Scene _newScene;

        /// <summary>
        /// Dùng cho scene hiện tại muốn load sang scene mới
        /// </summary>
        /// <param name="newSceneName"></param>
        /// <param name="onLoaded"></param>
        /// <returns></returns>
        public static async UniTask LoadSceneAsync(string newSceneName, Action<GameObject> onLoaded = null) {
            var currentSceneName = SceneManager.GetActiveScene().name;
            
            if (IsLoading)
            {
                Debug.LogWarning("Scene is loading, must wait until it's done");
                return;
            }
            if(newSceneName.IsNullOrEmpty() || currentSceneName.IsNullOrEmpty())
            {
                Debug.LogWarning("Scene name is null or empty");
                return;
            }

            _onLoaded = onLoaded;
            IsLoading = true;
            _currentSceneName = currentSceneName;
            _newSceneName = newSceneName;
            
            _newSceneLoaded = false;
            await SceneManager.LoadSceneAsync(newSceneName, LoadSceneMode.Additive);
            _newScene = SceneManager.GetSceneByName(_newSceneName);
            _newSceneLoaded = true;
 
        }

        /// <summary>
        /// Dùng để unload scene hiện tại sau khi mọi thứ đã đc khởi tạo xong ở scene mới
        /// </summary>
        private static async UniTask UnLoadSceneAsync(Scene oldScene)
        {
            // Unload the current scene
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(oldScene);

            // Wait until the current scene is fully unloaded
            while (asyncUnload is { isDone: false })
            {
                await UniTask.Yield();
            }
            IsLoading = false;
        }
        
        /// <summary>
        /// Dùng cho scene mới khi đã load thành công muốn instantiate prefab chính cùa scene
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="waitFor"></param>
        public static async void InstantiateAsync(GameObject prefab, UniTask waitFor = default)
        {
            if (!IsLoading) {
                Debug.LogWarning("Scene is not loading, must call LoadSceneAsync first");
                return;
            }

            await UniTask.WaitUntil(() => _newSceneLoaded);
            
            var newObject = Object.Instantiate(prefab);
       
            SceneManager.SetActiveScene(_newScene);
            SceneManager.MoveGameObjectToScene(newObject, _newScene);

            if (!waitFor.Equals(default(UniTask))) {
                await waitFor;
            }
            _isMoveToNewSceneComplete = true;


            if (!_isReloadScene) {
                var oldScene = SceneManager.GetSceneByName(_currentSceneName);
                await UnLoadSceneAsync(oldScene);
            } 
            else {
                _isReloadScene = false;
            }
            
            _onLoaded?.Invoke(newObject);
            _onLoaded = null;
        }
    
    
        /// <summary>
        /// Di chuyển gameObject mới tạo ra vào scene mới nếu nó ko ở sẵn trong scene mới
        /// </summary>
        /// <param name="obj"></param>
        public static void MoveObjectToNewScene(GameObject obj)
        {
            if(_currentSceneName.IsNullOrEmpty())
                return;
            Scene scene = SceneManager.GetSceneByName(_newSceneName);
            SceneManager.MoveGameObjectToScene(obj, scene);
        }
    
        /// <summary>
        /// Reload scene hiện tại
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async UniTask ReloadSceneAsync(Action onLoaded = null) 
        {
            if (IsLoading)
            {
                Debug.LogWarning("Scene is already loading, must wait until it's done");
                return;
            }
            _isReloadScene = true;
            _newSceneLoaded = false;
            IsLoading = true;
            _isMoveToNewSceneComplete = false;
            var currentScene = SceneManager.GetActiveScene();
            var currentSceneHash = currentScene.GetHashCode();
            await SceneManager.LoadSceneAsync(_newSceneName, LoadSceneMode.Additive);
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == _newSceneName && scene.GetHashCode() != currentSceneHash) {
                    _newScene = scene;
                    _newSceneLoaded = true;
                    break;
                }
            }
            await UniTask.WaitUntil(()=> _isMoveToNewSceneComplete);
            await SceneManager.UnloadSceneAsync(currentScene);
            onLoaded?.Invoke();
            IsLoading = false;

        }
    
        /// <summary>
        /// Dùng để kiểm tra xem scene đã load xong chưa
        /// </summary>
        public static async UniTask IsSceneLoaded()
        {
            await UniTask.WaitUntil(() => IsLoading == false);
        }

        public static string GetCurrentScene() {
            if (_newSceneName.IsNullOrEmpty()) {
                return SceneManager.GetActiveScene().name;
            }
            return _newSceneName;
        }
    
    }
}
