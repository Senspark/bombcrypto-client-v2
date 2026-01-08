using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using UnityEngine;

namespace Communicate {
    public class ReactMessage {
        public ReactMessage(string cmd, string data) {
            Cmd = cmd;
            Data = data;
        }
        public string Cmd { get;}
        public string Data { get; }
    }
    public class NewJavascriptProcessor : MonoBehaviour, IJavascriptProcessor {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void JS_CallMethod(
            string sender, string tag, string callback, int callbackId, string data);
#endif

        private const string InstanceName = "JsProcessor";
        private static NewJavascriptProcessor _sharedInstance;

        public static NewJavascriptProcessor Instance {
            get {
                if (!_sharedInstance) {
                    var go = new GameObject(InstanceName);
                    _sharedInstance = go.AddComponent<NewJavascriptProcessor>();
                    DontDestroyOnLoad(go);
                }
                return _sharedInstance;
            }
        }

        private int _idCounter;
        private readonly Dictionary<int, UniTaskCompletionSource<string>> _tasks = new();
        private Action<ReactMessage> _onReactMessage;

        public async UniTask<string> CallReact(string methodTag, string data) {
#if UNITY_WEBGL
            var id = _idCounter++;
            var task = new UniTaskCompletionSource<string>();
            _tasks[id] = task;
            JS_CallMethod(InstanceName, methodTag, nameof(CallMethodResponse), id, data);
            var result = await task.Task;
            return result;
#else
            return null;
#endif
        }

        /// <summary>
        /// Đăng ký method để react gọi (hiện chỉ dùng 1 base class để lắng nghe event này và base sẽ gọi các callback tuơng ứng tuỳ theo cmd)
        /// </summary>
        /// <param name="action"></param>
        public void RegisterUnityAction(Action<ReactMessage> action) {
            _onReactMessage += action;
        }

        /// <summary>
        /// React sẽ gọi unity thông qua method này
        /// </summary>
        /// <param name="dataFromReact"></param>
        public void CallUnity(string dataFromReact) {
            var responseJson = JObject.Parse(dataFromReact);
            var cmd = (string)responseJson["cmd"];
            var data = (string)responseJson["data"];
            var react = new ReactMessage(cmd, data);
            _onReactMessage?.Invoke(react);
        }

        private void CallMethodResponse(string response) {
            var responseJson = JObject.Parse(response);
            var id = (int)responseJson["id"];
            var result = (string)responseJson["response"];
            var task = _tasks[id];
            task.TrySetResult(result);
        }
    }
}