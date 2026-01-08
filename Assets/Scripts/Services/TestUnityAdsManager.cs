using System;
using System.Threading.Tasks;

using Senspark;

using Services.IapAds;

using AdResult = Services.IapAds.AdResult;

namespace Services {
    public class TestUnityAdsManager : ObserverManager<AdsManagerObserver>, IUnityAdsManager {
        private TaskCompletionSource<AdResult> _tcs;
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        private void OnGUI() {
            throw new NotImplementedException();
        }

        public bool IsAdLoaded() {
            return true;
        }

        public Task<string> ShowRewarded() {
            return Task.FromResult("test");
        }

        public async Task<AdResult> ShowInterstitial(InterstitialCategory category) {
            _tcs ??= new TaskCompletionSource<AdResult>();
            return await _tcs.Task;
        }
    }
}