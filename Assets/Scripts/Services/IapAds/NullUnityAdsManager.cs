using System;
using System.Threading.Tasks;

using App;

using Senspark;

namespace Services.IapAds {
    public class NullUnityAdsManager : ObserverManager<AdsManagerObserver>, IUnityAdsManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public bool IsAdLoaded() {
            return true;
        }

        public Task<string> ShowRewarded() {
            throw new Exception("No Ads");
        }

        public Task<AdResult> ShowInterstitial(InterstitialCategory category) {
            return Task.FromResult(AdResult.Error);
        }
    }
}