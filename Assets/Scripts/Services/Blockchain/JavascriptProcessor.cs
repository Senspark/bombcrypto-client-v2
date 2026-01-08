using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;
using UnityEngine.Assertions;

namespace App {
    public class JavascriptProcessor : MonoBehaviour {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void Javascript_CallMethod(
            string sender, string tag, string callback, int callbackId, string args);

        [DllImport("__Internal")]
        private static extern void Javascript_Subscribe(string sender, string tag, string callback);

        [DllImport("__Internal")]
        private static extern void Javascript_Unsubscribe(string tag);
#endif

        private const string InstanceName = "WebGLBlockchainProcessor";
        private static JavascriptProcessor _sharedInstance;

        public static JavascriptProcessor Instance {
            get {
                if (_sharedInstance == null) {
                    var go = new GameObject(InstanceName);
                    _sharedInstance = go.AddComponent<JavascriptProcessor>();
                    DontDestroyOnLoad(go);
                }
                return _sharedInstance;
            }
        }

        private int _idCounter;
        private Dictionary<int, TaskCompletionSource<string>> _tasks;
        private Dictionary<string, Action<string>> _events;

        private void Awake() {
            _tasks = new Dictionary<int, TaskCompletionSource<string>>();
            _events = new Dictionary<string, Action<string>>();
        }

        public async Task<string> CallMethod(string methodTag, params object[] args) {
#if UNITY_WEBGL
            var id = _idCounter++;
            var task = new TaskCompletionSource<string>();
            _tasks[id] = task;
            var argsJson = JsonConvert.SerializeObject(args);
            Javascript_CallMethod(InstanceName, methodTag, nameof(CallMethodResponse), id, argsJson);
            var result = await task.Task;
            return result;
#else
            return null;
#endif
        }

        private void CallMethodResponse(string response) {
            var responseJson = JObject.Parse(response);
            var id = (int) responseJson["id"];
            var result = (string) responseJson["response"];
            var task = _tasks[id];
            task.SetResult(result);
        }

        public void Subscribe(string methodTag, Action<string> action) {
#if UNITY_WEBGL
            _events[methodTag] = action;
            Javascript_Subscribe(InstanceName, methodTag, nameof(SubscribeResponse));
#endif
        }

        public void Unsubscribe(string methodTag) {
#if UNITY_WEBGL
            _events.Remove(methodTag);
            Javascript_Unsubscribe(methodTag);
#endif
        }

        private void SubscribeResponse(string response) {
            var responseJson = JObject.Parse(response);
            var methodName = (string) responseJson["tag"];
            var result = (string) responseJson["response"];
            Assert.IsNotNull(methodName);
            if (_events.ContainsKey(methodName)) {
                _events[methodName].Invoke(result);
            } else {
                Debug.LogWarning($"{methodName} event not existed");
            }
        }
    }
}