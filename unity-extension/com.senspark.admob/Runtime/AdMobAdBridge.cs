using System.Collections.Generic;
using System.Threading.Tasks;

using GoogleMobileAds.Api;

using JetBrains.Annotations;

namespace Senspark.Internal {
    internal class AdMobAdBridge : IAdBridge, IAdMobAdListener {
        [NotNull]
        private readonly IServiceLocator _serviceLocator;

        [CanBeNull]
        private readonly string _bannerAdId;

        [CanBeNull]
        private readonly string _rectAdId;

        [CanBeNull]
        private readonly string _appOpenAdId;

        [CanBeNull]
        private readonly string _interstitialAdId;

        [CanBeNull]
        private readonly string _rewardedInterstitialAdId;

        [CanBeNull]
        private readonly string _rewardedAdId;

        private readonly bool _useAdaptiveBannerAd;

        [CanBeNull]
        private readonly List<string> _testDeviceIds;

        [CanBeNull]
        private IAdListener _listener;

        public AdMobAdBridge(
            [NotNull] IServiceLocator serviceLocator,
            [CanBeNull] string bannerAdId,
            [CanBeNull] string rectAdId,
            [CanBeNull] string appOpenAdId,
            [CanBeNull] string interstitialAdId,
            [CanBeNull] string rewardedInterstitialAdId,
            [CanBeNull] string rewardedAdId,
            bool useAdaptiveBannerAd,
            [CanBeNull] List<string> testDeviceIds
        ) {
            _serviceLocator = serviceLocator;
            _bannerAdId = bannerAdId;
            _rectAdId = rectAdId;
            _appOpenAdId = appOpenAdId;
            _interstitialAdId = interstitialAdId;
            _rewardedInterstitialAdId = rewardedInterstitialAdId;
            _rewardedAdId = rewardedAdId;
            _useAdaptiveBannerAd = useAdaptiveBannerAd;
            _testDeviceIds = testDeviceIds;
        }

        public async Task<bool> Initialize(IAdListener listener) {
            var tcs = new TaskCompletionSource<bool>();
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(status => { //
                _listener = listener;
                tcs.SetResult(true);
            });
            MobileAds.SetRequestConfiguration(new RequestConfiguration {
                TestDeviceIds = _testDeviceIds ?? new List<string>(), //
            });
            return await tcs.Task;
        }

        public async Task OpenInspector() {
            var tcs = new TaskCompletionSource<object>();
            MobileAds.OpenAdInspector(error => { //
                tcs.SetResult(null);
            });
            await tcs.Task;
        }

        public IBannerAd CreateBannerAd() {
            return _bannerAdId == null
                ? new NullBannerAd()
                : new BaseBannerAd(_serviceLocator,
                    new AdMobBannerAdBridge(_bannerAdId, _useAdaptiveBannerAd, false, this));
        }

        public IBannerAd CreateRectAd() {
            return _rectAdId == null
                ? new NullBannerAd()
                : new BaseBannerAd(_serviceLocator,
                    new AdMobBannerAdBridge(_rectAdId, false, true, this));
        }

        public IFullScreenAd CreateAppOpenAd() {
            return _appOpenAdId == null
                ? new NullFullScreenAd()
                : new BaseFullScreenAd(_serviceLocator,
                    new AdMobAppOpenAdBridge(_appOpenAdId, this)).AutoLoad();
        }

        public IFullScreenAd CreateInterstitialAd() {
            return _interstitialAdId == null
                ? new NullFullScreenAd()
                : new BaseFullScreenAd(_serviceLocator,
                    new AdMobInterstitialAdBridge(_interstitialAdId, this)).AutoLoad();
        }

        public IFullScreenAd CreateRewardedInterstitialAd() {
            return _rewardedInterstitialAdId == null
                ? new NullFullScreenAd()
                : new BaseFullScreenAd(_serviceLocator,
                    new AdMobRewardedInterstitialAdBridge(_rewardedInterstitialAdId, this)).AutoLoad();
        }

        public IFullScreenAd CreateRewardedAd() {
            return _rewardedAdId == null
                ? new NullFullScreenAd()
                : new BaseFullScreenAd(_serviceLocator,
                    new AdMobRewardedAdBridge(_rewardedAdId, this)).AutoLoad();
        }

        public void OnAdPaid(
            long value,
            string currencyCode,
            AdFormat format,
            string adUnit,
            string adSourceName
        ) {
            _listener?.OnAdPaid(
                mediationNetwork: AdNetwork.AdMob,
                monetizationNetwork: adSourceName,
                revenue: value / 1e6f,
                currencyCode: currencyCode,
                format: format,
                adUnit: adUnit
            );
        }
    }
}