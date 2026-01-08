using System.Threading.Tasks;
using App;
using Senspark;
using Newtonsoft.Json;

namespace Services {
    public class RankInfoManager : IRankInfoManager {
        private class Data {
            [JsonProperty("bomb_rank")]
            public int BombRank;

            [JsonProperty("current_point")]
            public int CurrentPoint;
            
            [JsonProperty("decay_point_config")]
            public int DecayPointConfig;

            [JsonProperty("min_matches_config")]
            public int MinMatchesConfig;
            
            [JsonProperty("amount_matches_current_date")]
            public int AmountMatches;
            
            [JsonProperty("decay_point_user")]
            public int DecayPointUser;
        }

        private Data _data;
        private GameModeType _lastPlayMode = GameModeType.UnKnown;
        private ILogManager _logManager;
        private readonly IServerRequester _serverRequester;

        public RankInfoManager(IServerRequester serverRequester) {
            _serverRequester = serverRequester;
        }

        public int BombRank => _data.BombRank;
        public int CurrentPoint => _data.CurrentPoint;
        public int DecayPointConfig => _data.DecayPointConfig;
        public int MinMatchesConfig => _data.MinMatchesConfig;
        public int AmountMatches => _data.AmountMatches;
        public int DecayPointUser {
            get =>
                _data.DecayPointUser;
            set =>
                _data.DecayPointUser = value;
        }

        public GameModeType LastPlayMode => _lastPlayMode;
        public void Destroy() {
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public async Task InitializeAsync() {
            _logManager ??= ServiceLocator.Instance.Resolve<ILogManager>();
            _logManager.Log();
            _data ??= JsonConvert.DeserializeObject<Data>(await _serverRequester.GetRankInfo());
        }

        public async Task ReloadData() {
            _data = JsonConvert.DeserializeObject<Data>(await _serverRequester.GetRankInfo());
        }

        public void UpdateLastPlayMode(GameModeType mode) {
            _lastPlayMode = mode;
        }
    }
}