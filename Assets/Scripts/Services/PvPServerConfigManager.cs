using System;
using System.Threading.Tasks;

using App;

using Data;

using Newtonsoft.Json;

using UnityEngine;

namespace Services {
    public class PvPServerConfigManager : IPvPServerConfigManager {
        public ITaskDelay TaskDelay { get; }

        private PvPServerConfigData _data;
        private readonly IServerRequester _serverRequester;
        private TaskCompletionSource<bool> _tcs;

        public PvPServerConfigManager(IServerRequester serverRequester, ITaskDelay taskDelay) {
            _serverRequester = serverRequester;
            TaskDelay = taskDelay;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }


        public PvPServerConfigData GetConfig() {
            return _data;
        }

        public async Task InitializeAsync() {
            if (_tcs == null) {
                _tcs = new TaskCompletionSource<bool>();
                try {
                    var result = JsonConvert.DeserializeObject<GetConfigResult>(
                        await _serverRequester.GetPvPServerConfig()
                    ) ?? throw new ArgumentNullException();
                    if (result.Code != 0) {
                        throw new Exception(result.Message);
                    }
                    _data = new PvPServerConfigData {
                        Servers = result.Servers,
                        Zones = result.Zones
                    };
                } catch (Exception e) {
#if UNITY_EDITOR
                    Debug.LogWarning($"message: {e.Message}, stack trace: {e.StackTrace}");
#endif
                    throw new Exception(e.Message);
                } finally {
                    _tcs.SetResult(true);
                }
            }
            await _tcs.Task;
        }
        
        private class GetConfigResult {
            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("servers")]
            public PvPServerData[] Servers;

            [JsonProperty("zones")]
            public ZoneData[] Zones;

            [JsonProperty("message")]
            public string Message;
        }
    }
}