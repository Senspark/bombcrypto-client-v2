using System;
using System.Threading.Tasks;

using App;

namespace Services {
    public class DefaultOfflineRewardManager : IOfflineRewardManager {
        private long _lastLogout;
        private (int ItemId, int Quantity)[] _rewards;
        private readonly IServerRequester _serverRequester;

        public bool HadRewards => _rewards is {Length: > 0};
        
        public DefaultOfflineRewardManager(IServerRequester serverRequester) {
            _serverRequester = serverRequester;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
        
        public long GetLastLogout() {
            return _lastLogout;
        }

        public (int ItemId, int Quantity)[] GetRewards() {
            return _rewards;
        }
        
        public async Task InitializeAsync() {
            var (lastLogout, rewards) = await _serverRequester.GetOfflineReward();
            _lastLogout = lastLogout;
            _rewards = rewards;
        }

        public void ResetRewards() {
            _rewards = null;
        }
    }
}
