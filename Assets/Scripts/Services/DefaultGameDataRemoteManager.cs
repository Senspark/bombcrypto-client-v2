using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Senspark;

using Game.UI.GameData;

using Newtonsoft.Json;

using Services;

using UnityEngine;

namespace App {
    public class DefaultGameDataRemoteManager : IGameDataRemoteManager {
        private string GetBasePath =>
            Path.Combine(Application.streamingAssetsPath, RemoteFolder, _networkConfig.NetworkName);

        private string GetFilePath => Path.Combine(GetBasePath, JsonFile);
        private Dictionary<string, GameData> _gameData;
        private const string RemoteFolder = "GameData";
        private const string JsonFile = "data.json";

        private readonly INetworkConfig _networkConfig;
        private readonly ILogManager _logManager;

        public DefaultGameDataRemoteManager(ILogManager logManager, INetworkConfig networkConfig) {
            _networkConfig = networkConfig;
            _logManager = logManager;
        }
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task SyncRemoteData() {
            if (_gameData != null) {
                return;
            }

            var jsonPath = GetFilePath;
            var data = await GetTextFile(jsonPath);
            var result = JsonConvert.DeserializeObject<GameData[]>(data);
            if (result != null) {
                _gameData = new Dictionary<string, GameData>();
                foreach (var r in result) {
                    _gameData[r.eventName] = r;
                }
            }
        }
        
        public bool GetData(string eventName) {
            var data = _gameData.ContainsKey(eventName);
            return data && _gameData[eventName].enable;
        }

        private async Task<string> GetTextFile(string path) {
            if (Utils.IsUrl(path)) {
                var (code, res) = await Utils.GetWebResponse(_logManager, path);
                return res;
            }
            return await File.ReadAllTextAsync(path);
        }
    }
}