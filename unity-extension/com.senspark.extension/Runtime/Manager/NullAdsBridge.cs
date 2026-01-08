using System.Threading.Tasks;

namespace Senspark.Internal {
    internal class NullAdsBridge : IAdBridge {
        public Task<bool> Initialize(IAdListener listener) {
            return Task.FromResult(true);
        }

        public Task OpenInspector() {
            return Task.CompletedTask;
        }

        public IBannerAd CreateBannerAd() {
            return new NullBannerAd();
        }

        public IBannerAd CreateRectAd() {
            return new NullBannerAd();
        }

        public IFullScreenAd CreateAppOpenAd() {
            return new NullFullScreenAd();
        }

        public IFullScreenAd CreateInterstitialAd() {
            return new NullFullScreenAd();
        }

        public IFullScreenAd CreateRewardedInterstitialAd() {
            return new NullFullScreenAd();
        }

        public IFullScreenAd CreateRewardedAd() {
            return new NullFullScreenAd();
        }
    }
}