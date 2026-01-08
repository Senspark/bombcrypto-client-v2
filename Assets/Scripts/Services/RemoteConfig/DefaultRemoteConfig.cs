using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;

namespace Services.RemoteConfig {
    public class DefaultRemoteConfig : IRemoteConfig {
        private readonly IRemoteConfig _bridge;

        public DefaultRemoteConfig(ILogManager logManager) {
            if (!Application.isEditor) {
                var platform = Application.platform;
                switch (platform) {
                    case RuntimePlatform.Android or RuntimePlatform.IPhonePlayer:
                        _bridge = new MobileRemoteConfig(logManager);
                        return;
                    case RuntimePlatform.WebGLPlayer:
                        _bridge = new WebGLRemoteConfig();
                        return;
                }
            }
            _bridge = new NullRemoteConfig();
        }

        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() {
            _bridge.Destroy();
        }

        public async UniTask SyncData() {
            await _bridge.SyncData();
        }

        public void SetDefaultValues(Dictionary<string, object> defaultValues) {
            _bridge.SetDefaultValues(defaultValues);
        }

        public int GetInt(string key) {
            return _bridge.GetInt(key);
        }

        public string GetString(string key) {
            return _bridge.GetString(key);
        }

        public bool GetBool(string key) {
            return _bridge.GetBool(key);
        }
    }
}