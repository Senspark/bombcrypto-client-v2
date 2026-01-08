using System;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Senspark {
    public class SceneManager : ISceneManager {
        public async Task<T> LoadScene<T>(string sceneName) where T : MonoBehaviour {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            var item = UnityEngine.Object.FindObjectOfType<T>();
            if (!item) {
                throw new ArgumentNullException(nameof(sceneName));
            }
            return item;
        }
    }
}