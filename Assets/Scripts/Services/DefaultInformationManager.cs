using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Senspark;

using Game.UI.Information;

using Newtonsoft.Json;

using UnityEngine;

namespace App {
    public class DefaultInformationManager : IInformationManager {
        private string GetBasePath => Path.Combine(Application.streamingAssetsPath, RemoteFolder);
        private string GetFilePath => Path.Combine(GetBasePath, JsonFile);
        
        private const string RemoteFolder = "Information";
        private const string JsonFile = "data.json";

        private readonly ILogManager _logManager;
        private readonly ICacheRequestManager _cacheRequestManager;
        
        private InformationData[] _data;
        
        private const string _starCoreAirdropInfo =
            "1. STAR CORE is obtained from Treasure Hunt mode.\n\n" +
            "2. Players will compete for rankings based on the number of Star Cores they collect The top-ranked players will be rewarded with airdropped tokens.";
        
        public DefaultInformationManager(ILogManager logManager, ICacheRequestManager cacheRequestManager) {
            _logManager = logManager;
            _cacheRequestManager = cacheRequestManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() { }

        public async Task SyncRemoteData() {
            if (_data != null) {
                return;
            }

            var jsonPath = GetFilePath;
            var data = await GetTextFile(jsonPath);
            _data = JsonConvert.DeserializeObject<InformationData[]>(data);
            foreach (var d in _data) {
                UpdateTextByNetwork(d.displayName, ref d.content);
                d.content = d.content.Replace("\\n", Environment.NewLine);
            }
        }

        //DevHoang_20250715: Star core các mạng airdrop khác nhau, tạm thời xài chung đợi server phân biệt sẽ cập nhật lại
        private void UpdateTextByNetwork(string displayName, ref string info) {
            switch (displayName) {
                case "STAR CORE" when AppConfig.IsAirDrop():
                    info = _starCoreAirdropInfo;
                    break;
            }
        }

        public InformationData[] GetTokenData() {
            return _data;
        }

        public InformationData GetTokenData(ITokenReward reward) {
            var code = reward.Type.Name;
            var network = reward.Network;
            var data = _data.Where(e => e.code.Contains(code)).ToList();
            if (data.Count == 0) {
                return null;
            }
            if (data.Count == 1) {
                return data[0];
            }
            return data.Find(e => e.network == network);
        }

        private async Task<string> GetTextFile(string path) {
            if (Utils.IsUrl(path)) {
                var (code, res) =  await _cacheRequestManager.GetWebResponse(SFSDefine.SFSCommand.GET_INFORMATION_DATA, path);
                return res;
            }
            return await File.ReadAllTextAsync(path);
        }
    }
}