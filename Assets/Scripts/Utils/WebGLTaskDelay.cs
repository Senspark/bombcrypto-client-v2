using System;
using System.Collections;
using System.Threading.Tasks;

using UnityEngine;

namespace App {
    public interface ITaskDelay {
        Task Delay(int milliseconds);
        Task WaitUtil(Func<bool> predicate, int milliseconds = 1000);
    }

    public class EditorTaskDelay : ITaskDelay {
        public Task Delay(int milliseconds) {
            return Task.Delay(milliseconds);
        }

        public async Task WaitUtil(Func<bool> predicate, int milliseconds = 1000) {
            while (!predicate()) {
                await Delay(milliseconds);
            }
        }
    }

    public class WebGLTaskDelay : MonoBehaviour, ITaskDelay {
        private static WebGLTaskDelay _sharedInstance;

        public static WebGLTaskDelay Instance {
            get {
                if (!_sharedInstance) {
                    if (Application.isPlaying) {
                        var go = new GameObject("WebGLTaskDelay");
                        _sharedInstance = go.AddComponent<WebGLTaskDelay>();
                        DontDestroyOnLoad(go);    
                    } else {
                        throw new Exception("Game is not running");
                    }
                }
                return _sharedInstance;
            }
        }

        public async Task Delay(int milliseconds) {
            var task = new TaskCompletionSource<object>();
            StartCoroutine(DelayImpl(milliseconds, task));
            await task.Task;
        }

        public async Task WaitUtil(Func<bool> predicate, int milliseconds = 1000) {
            while (!predicate()) {
                await Delay(milliseconds);
            }
        }

        private static IEnumerator DelayImpl(int milliseconds, TaskCompletionSource<object> task) {
            yield return new WaitForSeconds(milliseconds / 1000f);
            task.SetResult(null);
        }
    }
}