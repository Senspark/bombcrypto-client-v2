using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using Senspark;

using Game.UI.Information;

namespace App {
    public class DefaultInformationManager : IInformationManager {
        private readonly ILogManager _logManager;

        private InformationData[] _data;

        private const string _starCoreAirdropInfo =
            "1. STAR CORE is obtained from Treasure Hunt mode.\n\n" +
            "2. Players will compete for rankings based on the number of Star Cores they collect The top-ranked players will be rewarded with airdropped tokens.";

        public DefaultInformationManager(ILogManager logManager) {
            _logManager = logManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() { }

        public UniTask SyncRemoteData() {
            if (_data != null) {
                return UniTask.CompletedTask;
            }

            _data = InformationTable.Build();
            foreach (var d in _data) {
                UpdateTextByNetwork(d.displayName, ref d.content);
            }
            return UniTask.CompletedTask;
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
    }
}
