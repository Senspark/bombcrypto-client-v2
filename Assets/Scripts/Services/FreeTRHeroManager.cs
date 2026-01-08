using System;
using System.Linq;
using System.Threading.Tasks;

using App;

using Constant;

using Data;

using Senspark;

using Newtonsoft.Json;

using Utils;

namespace Services {
    public class FreeTRHeroManager : IFreeTRHeroManager {
        private class ChooseHeroData {
            [JsonProperty("id")]
            public int InstanceId;

            [JsonProperty("item_id")]
            public int ItemId;

            [JsonProperty("level")]
            public int Level;

            [JsonProperty("quantity")]
            public int Quantity;

            [JsonProperty("status")]
            public int Status;
        }

        private FreeTRHeroData[] _heroes;
        private bool _initialized;
        private bool _isChose;
        private readonly ILogManager _logManager;
        private readonly IServerRequester _serverRequester;

        public FreeTRHeroManager(
            ILogManager logManager,
            IServerRequester serverRequester
        ) {
            _logManager = logManager;
            _serverRequester = serverRequester;
        }

        public async Task<int> ChooseAsync(FreeTRHeroData hero) {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<ChooseHeroData>>(
                await _serverRequester.ChooseTRHero(hero.ItemId)
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
            _isChose = true;
            return result.Data.InstanceId;
        }

        public void Destroy() {
        }

        public FreeTRHeroData[] GetHeroes() {
            Initialize();
            return _heroes;
        }

        private void Initialize() {
            if (_initialized) {
                return;
            }
            _initialized = true;
            var earlyConfigManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            var heroAbilityManager = ServiceLocator.Instance.Resolve<IHeroAbilityManager>();
            var productManager = ServiceLocator.Instance.Resolve<IProductManager>();
            var heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            
            _heroes = earlyConfigManager.Heroes.Where(it => it.Tutorial > 0)
                .Select(it => new FreeTRHeroData(
                    heroAbilityManager.GetAbilities(it.ItemId),
                    it.Color,
                    it.Skin,
                    productManager.GetProduct(it.ItemId).ProductName,
                    HeroType.TR,
                    it.ItemId,
                    heroStatsManager.GetStats(it.ItemId)
                )).ToArray();
            _isChose = !earlyConfigManager.IsGetTrHero;
        }

        Task<bool> IService.Initialize() {
            return Task.FromResult(true);
        }

        public bool IsChose() {
            Initialize();
            return _isChose;
        }
    }
}