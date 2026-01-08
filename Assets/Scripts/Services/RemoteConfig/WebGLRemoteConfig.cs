using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using AOT;

using Cysharp.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Services.RemoteConfig {
    public class WebGLRemoteConfig : IRemoteConfig {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void FetchAndActivate(string defaultConfig, Action callback);

        [DllImport("__Internal")]
        private static extern double GetDoubleValue(string key);
        
        [DllImport("__Internal")]
        private static extern string GetStringValue(string key);

        [MonoPInvokeCallback(typeof(Action))]
        private static void InitAndFetched() {
            _isFetched = true;
        }
#endif

        private static bool _isFetched;
        private readonly Dictionary<string, object> _defaultValues = new();

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public UniTask SyncData() {
#if UNITY_WEBGL
            var obj = new JObject();
            foreach (var kv in _defaultValues) {
                obj[kv.Key] = kv.Value.ToString();
            }
            FetchAndActivate(obj.ToString(), InitAndFetched);
#endif
            return UniTask.CompletedTask;
        }

        public void SetDefaultValues(Dictionary<string, object> defaultValues) {
            foreach (var (k,v) in defaultValues) {
                _defaultValues[k] = v;
            }
        }

        public int GetInt(string key) {
            if (!_isFetched) {
                return _defaultValues.TryGetValue(key, out var value) ? (int) value : 0;
            }
#if UNITY_WEBGL
            var v = (int) GetDoubleValue(key);
            return v;
#endif
            return 0;
        }

        public string GetString(string key) {
            if (!_isFetched) {
                return _defaultValues.TryGetValue(key, out var value) ? (string) value : null;
            }
#if UNITY_WEBGL
            var v = GetStringValue(key);
            return v;
#endif
            return null;
        }

        public bool GetBool(string key) {
            throw new NotImplementedException();
        }
    }
}