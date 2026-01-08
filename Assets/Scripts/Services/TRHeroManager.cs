using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using Data;

using Senspark;

using Newtonsoft.Json;

using Services.Server.Exceptions;

namespace Services {
    public class TRHeroManager : ITRHeroManager {
        private class GetHeroesItemHero {
            [JsonProperty("status")]
            public int Status;

            [JsonProperty("is_active")]
            public bool IsActive;
            
            [JsonProperty("quantity")]
            public int Quantity;

            [JsonProperty("hero_id")]
            public int HeroId;

            [JsonProperty("upgraded_speed")]
            public int UpgradedSpeed;

            [JsonProperty("upgraded_bomb")]
            public int UpgradedBomb;

            [JsonProperty("upgraded_range")]
            public int UpgradedRange;

            [JsonProperty("upgraded_hp")]
            public int UpgradedHp;

            [JsonProperty("upgraded_dmg")]
            public int UpgradedDmg;
            
            [JsonProperty("max_upgrade_speed")]
            public int MaxUpgradedSpeed;

            [JsonProperty("max_upgrade_bomb")]
            public int MaxUpgradedBomb;

            [JsonProperty("max_upgrade_range")]
            public int MaxUpgradedRange;

            [JsonProperty("max_upgrade_hp")]
            public int MaxUpgradedHp;

            [JsonProperty("max_upgrade_dmg")]
            public int MaxUpgradedDmg;
        }
        
        private class GetHeroesItem {
            [JsonProperty("item_id")]
            public int ItemId;

            [JsonProperty("heroes")]
            public GetHeroesItemHero[] Heroes;
        }

        private class GetHeroesResult {
            [JsonProperty("data")]
            public GetHeroesItem[] Data;

            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("es")]
            public string Message;
        }

        private IHeroColorManager _heroColorManager;
        private IHeroIdManager _heroIdManager;
        private IHeroStatsManager _heroStatsManager;
        private readonly ILogManager _logManager;
        private readonly IServerRequester _serverRequester;
        private readonly IServerManager _serverManager;

        public TRHeroManager(ILogManager logManager, IServerRequester serverRequester, IServerManager serverManager) {
            _logManager = logManager;
            _serverRequester = serverRequester;
            _serverManager = serverManager;
        }

        public void Destroy() {
        }

        public async Task<IEnumerable<TRHeroData>> GetHeroesAsync(string type) {
            _heroColorManager ??= ServiceLocator.Instance.Resolve<IHeroColorManager>();
            _heroIdManager ??= ServiceLocator.Instance.Resolve<IHeroIdManager>();
            _heroStatsManager ??= ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            var json = await _serverRequester.GetTRHeroes(type);
            // update gold
            await _serverManager.General.GetChestReward();

            var result = JsonConvert.DeserializeObject<GetHeroesResult>(json) ?? throw new Exception(json);
            if (result.Code == 0) {
                return result.Data.SelectMany(it => it.Heroes.Select(hero => {
                    var heroId = _heroIdManager.GetHeroId(it.ItemId);
                    _logManager.Log($"hero id: {heroId}");
                    var color = _heroColorManager.GetColor(heroId);
                    _logManager.Log($"color: {color}");
                    return new TRHeroData(
                        color,
                        heroId,
                        hero.HeroId,
                        it.ItemId,
                        int.MinValue,
                        hero.IsActive,
                        hero.Quantity,
                        _heroStatsManager.GetStats(it.ItemId),
                        hero.Status,
                        hero.UpgradedSpeed,
                        hero.UpgradedBomb,
                        hero.UpgradedRange,
                        hero.UpgradedHp,
                        hero.UpgradedDmg,
                        hero.MaxUpgradedSpeed,
                        hero.MaxUpgradedBomb,
                        hero.MaxUpgradedRange,
                        hero.MaxUpgradedHp,
                        hero.MaxUpgradedDmg
                    );
                }));
            }
            throw new ErrorCodeException(result.Code, result.Message);
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }
    }
}