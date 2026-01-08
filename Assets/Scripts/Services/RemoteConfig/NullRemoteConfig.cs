using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

namespace Services.RemoteConfig {
    public class NullRemoteConfig : IRemoteConfig {
        private readonly Dictionary<string, object> _defaultValues = new();
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public UniTask SyncData() {
            return UniTask.CompletedTask;
        }

        public void SetDefaultValues(Dictionary<string, object> defaultValues) {
            foreach (var (k,v) in defaultValues) {
                _defaultValues[k] = v;
            }
        }

        public int GetInt(string key) {
            if (_defaultValues.ContainsKey(key)) {
                return (int) GetLong(key);
            }
            return 0;
        }

        public long GetLong(string key) {
            if (_defaultValues.ContainsKey(key)) {
                return (long) _defaultValues[key];
            }
            return 0;
        }
        
        public string GetString(string key) {
            if (_defaultValues.ContainsKey(key)) {
                return (string) _defaultValues[key];
            }
            return null;
        }

        public bool GetBool(string key) {
            if (_defaultValues.ContainsKey(key)) {
                return (bool) _defaultValues[key];
            }
            return false;
        }
        
    }
}