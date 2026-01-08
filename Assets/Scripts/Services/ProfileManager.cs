using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Data;

using Newtonsoft.Json;

using UnityEngine;

namespace Services {
    public class ProfileManager : IProfileManager {
        private const string Key = "profiles";

        private class Data {
            [JsonProperty("hash")]
            public string Hash;

            [JsonProperty("login_time")]
            public long LoginTime;
        }

        private Dictionary<string, ProfileData> _data;

        public void Destroy() {
        }

        public Task<bool> Initialize() {
            var value = PlayerPrefs.GetString(Key, string.Empty);
            try {
                var data = JsonConvert.DeserializeObject<Data[]>(value) ?? Array.Empty<Data>();
                _data = data.ToDictionary(
                    it => it.Hash,
                    it => new ProfileData {
                        Hash = it.Hash,
                        LoginTime = DateTime.UnixEpoch + TimeSpan.FromTicks(it.LoginTime)
                    }
                );
            } catch (Exception e) {
#if UNITY_EDITOR
                Debug.LogWarning($"message: {e.Message}\nstack trace: {e.StackTrace}");
#endif
                _data = new Dictionary<string, ProfileData>();
            }
            return Task.FromResult(true);
        }

        private void Save() {
            PlayerPrefs.SetString(Key, JsonConvert.SerializeObject(_data.Values.Select(it => new Data {
                Hash = it.Hash,
                LoginTime = (it.LoginTime - DateTime.UnixEpoch).Ticks
            })));
        }

        public DateTime? GetLoginTime(string hash) {
            return _data.TryGetValue(hash, out var profile) ? profile.LoginTime : null;
        }

        public bool TryGetProfile(string hash, out ProfileData profile) {
            return _data.TryGetValue(hash, out profile);
        }

        public void UpdateLoginTime(string hash, DateTime time) {
            if (!_data.TryGetValue(hash, out var profile)) {
                profile = new ProfileData();
                _data[hash] = profile;
            }
            profile.LoginTime = time;
            Save();
        }
    }
}