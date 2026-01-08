using System;
using System.Linq;
using System.Threading.Tasks;

using App;

using Data;

using Senspark;

using Newtonsoft.Json;

namespace Services {
    public class BoosterManager : IBoosterManager {
        private class GetBoosterData {
            [JsonProperty("item_id")]
            public int BoosterId;

            [JsonProperty("cool_down")]
            public float Cooldown;

            [JsonProperty("time_effect")]
            public float Duration;

            [JsonProperty("quantity")]
            public int Quantity;
        }

        private struct SendExtensionRequestResult<T> {
            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("boosters")]
            public T Data;

            [JsonProperty("es")]
            public string Message;
        }

        private IEarlyConfigManager _earlyConfigManager;
        private bool _initialized;
        private IProductItemManager _productItemManager;
        private readonly IServerRequester _serverRequester;

        public BoosterManager(IServerRequester serverRequester) {
            _serverRequester = serverRequester;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        private async Task<BoosterData[]> GetBoostersAsync(Func<int[]> boosterIdsGetter) {
            await InitializeAsync();
            var result = JsonConvert.DeserializeObject<SendExtensionRequestResult<GetBoosterData[]>>(
                await _serverRequester.GetBooster()
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
            var boosters = result.Data.ToDictionary(it => it.BoosterId);
            return boosterIdsGetter().Select(boosterId => {
                if (!boosters.TryGetValue(boosterId, out var booster)) {
                    booster = new GetBoosterData();
                }
                return new BoosterData(
                    boosterId,
                    booster.Cooldown,
                    _productItemManager.GetDescription(boosterId),
                    booster.Duration,
                    booster.Quantity
                );
            }).ToArray();
        }

        public Task<BoosterData[]> GetPvEBoostersAsync() {
            return GetBoostersAsync(() => _earlyConfigManager.PvEBoosterIds);
        }

        public Task<BoosterData[]> GetPvPBoostersAsync() {
            return GetBoostersAsync(() => _earlyConfigManager.PvPBoosterIds);
        }

        private async Task InitializeAsync() {
            _earlyConfigManager ??= ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
            await _earlyConfigManager.InitializeAsync();
            await _productItemManager.InitializeAsync();
        }
    }
}