using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using Constant;

using Data;

using Senspark;

using Newtonsoft.Json;

namespace Services {
    public class EarlyConfigManager : IEarlyConfigManager {
        private class Data {
            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("disable_features")]
            public int[] DisableFeature;

            [JsonProperty("config_hero_traditional")]
            public EarlyConfigHeroData[] Heroes;

            [JsonProperty("product_items")]
            public EarlyConfigItemData[] Items;

            [JsonProperty("bomb_rank_config")]
            public EarlyConfigRankData[] Ranks;
            
            [JsonProperty("es")]
            public string Message;

            [JsonProperty("is_get_hero_tr")]
            public bool IsGetTrHero;

            [JsonProperty("current_season")]
            public int CurrentSeason;

            [JsonProperty("item_id_booster_adv")]
            public int[] PvEBoosterIds;

            [JsonProperty("item_id_booster_pvp")]
            public int[] PvPBoosterIds;

            [JsonProperty("update_status")]
            public int UpdateStatus;
        }

        private Data _data;
        private readonly IServerRequester _serverRequester;

        public EarlyConfigManager(IServerRequester serverRequester) {
            _serverRequester = serverRequester;
        }

        public int[] DisableFeatures => _data.DisableFeature;
        public EarlyConfigHeroData[] Heroes => _data.Heroes;
        public EarlyConfigRankData[] Ranks => _data.Ranks;
        public bool IsGetTrHero => _data.IsGetTrHero;
        public int CurrentSeason => _data.CurrentSeason;
        public EarlyConfigItemData[] Items => _data.Items;
        public int[] PvEBoosterIds => _data.PvEBoosterIds;
        public int[] PvPBoosterIds => _data.PvPBoosterIds;
        public int UpdateStatus => _data.UpdateStatus;

        private HashSet<int> _disableFeature;

        private HashSet<int> HashSetDisableFeature {
            get {
                _disableFeature ??= _data.DisableFeature.ToHashSet();
                return _disableFeature;
            }
        }

        public void Destroy() {
        }

        Task<bool> IService.Initialize() {
            return Task.FromResult(true);
        }

        public void Initialize(string json) {
            _data = JsonConvert.DeserializeObject<Data>(json) ?? throw new Exception();
        }

        public async Task InitializeAsync() {
            if(AppConfig.IsSolana())
                return;
            _data ??= JsonConvert.DeserializeObject<Data>(await _serverRequester.GetEarlyConfig()) ??
                      throw new Exception();
            if (_data.Code != 0) {
                throw new Exception(_data.Message);
            }
        }

        public bool IsDisableFeature(FeatureId featureId) {
            if (HashSetDisableFeature.Contains((int)featureId)) {
                return true;
            }
            return false;
        }
    }
}