using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Senspark;

using UnityEngine;
using UnityEngine.Assertions;

namespace Services.RemoteConfig {
    public class MobileRemoteConfig : IRemoteConfig {
        private bool _isFetched;
        private readonly Dictionary<string, object> _defaultValues = new();
        private readonly ILogManager _logManager;
        private IRemoteConfigManager _firebase;

        public MobileRemoteConfig(ILogManager logManager) {
            _logManager = logManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() { }

        public async UniTask SyncData() {
            try {
                _firebase = new FirebaseRemoteConfigManager.Builder() {
                        Defaults = _defaultValues,
                        UseRealTime = true
                    }
                    .Build();
                await _firebase.Initialize(2);
            } catch (Exception e) {
                Debug.LogError(e.Message);
            }
        }

        public void SetDefaultValues(Dictionary<string, object> defaultValues) {
            foreach (var (k, v) in defaultValues) {
                _defaultValues[k] = v;
            }
        }

        public int GetInt(string key) {
            Assert.IsNotNull(_firebase);
            return (int)_firebase.GetLong(key);
        }

        public string GetString(string key) {
            Assert.IsNotNull(_firebase);
            return _firebase.GetString(key);
        }

        public bool GetBool(string key) {
            Assert.IsNotNull(_firebase);
            return _firebase.GetBool(key);
        }
    }
}