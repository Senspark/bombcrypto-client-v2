using System;
using System.Threading.Tasks;

using App;

using Data;

using Newtonsoft.Json;

using Services.Server.Exceptions;

using Utils;

namespace Services {
    public class LuckyWheelManager : ILuckyWheelManager {
        private LuckyWheelRewardData[] _data;
        private readonly IServerRequester _serverRequester;

        public LuckyWheelManager(IServerRequester serverRequester) {
            _serverRequester = serverRequester;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public LuckyWheelRewardData[] GetRewards() {
            return _data;
        }

        public async Task InitializeAsync() {
            var r = await _serverRequester.GetLuckyWheelReward();
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<LuckyWheelRewardData[]>>(r);
            if (result.Code == 0) {
                _data = result.Data;
            } else {
                throw new ErrorCodeException(result.Code, result.Message);
            }
        }
    }
}