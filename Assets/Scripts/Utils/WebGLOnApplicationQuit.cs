using UnityEngine;

namespace App {
    public class WebGLOnApplicationQuit : MonoBehaviour {
        private static WebGLOnApplicationQuit _sharedInstance;

        public static void Init() {
#if UNITY_WEBGL
            if (_sharedInstance) {
                return;
            }
            
            var go = new GameObject("WebGLOnApplicationQuit");
            _sharedInstance = go.AddComponent<WebGLOnApplicationQuit>();
            DontDestroyOnLoad(go);
            WebGLUtils.InitBeforeUnloadEvent();
#endif
        }
        
        private void OnBeforeUnload() {
#if UNITY_WEBGL
            var all = FindObjectsOfType<GameObject>();
            foreach (var go in all)
            {
                go.SendMessage("OnApplicationQuit", SendMessageOptions.DontRequireReceiver);
            }
#endif
        }
    }
}