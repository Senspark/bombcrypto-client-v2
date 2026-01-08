using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Senspark;

using Newtonsoft.Json;

using UnityEngine;

namespace App {
    [Serializable]
    public class AirDropEventData {
        public string code;
        public bool canClaimNft;
        public string title;
        public float fee;
        public string content;
        public string homePage;
        public Sprite icon;
    }
    
    public class DefaultAirDropManager : IAirDropManager {
        private const string AirDropFolder = "AirDrop";
        private const string AirDropJsonFile = "AirDrop.json";
        
        private readonly ILogManager _logManager;
        private Dictionary<string, AirDropEventData> _dict;

        public DefaultAirDropManager(ILogManager logManager) {
            _logManager = logManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task SyncRemoteData() {
            if (_dict != null) {
                return;
            }

            var basePath = Path.Combine(Application.streamingAssetsPath, AirDropFolder);
            var jsonPath = Path.Combine(basePath, AirDropJsonFile);
            var data = await GetTextFile(jsonPath);
            var result = JsonConvert.DeserializeObject<AirDropEventData[]>(data);
            if (result != null) {
                _dict = new Dictionary<string, AirDropEventData>();
                foreach (var r in result) {
                    var iconPath = Path.Combine(basePath, $"{r.code}.png");
                    r.icon = await Utils.LoadImageFromPath(iconPath);
                    _dict[r.code] = r;
                }
            }
        }

        public AirDropEventData GetData(string code) {
            return _dict.ContainsKey(code) ? _dict[code] : null;
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