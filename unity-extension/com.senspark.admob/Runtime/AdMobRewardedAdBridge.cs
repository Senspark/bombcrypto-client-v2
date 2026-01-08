using System.Threading.Tasks;

using GoogleMobileAds.Api;

using JetBrains.Annotations;

using UnityEngine.Assertions;

namespace Senspark.Internal {
    internal class AdMobRewardedAdBridge : IFullScreenAdBridge {
        [NotNull]
        private readonly string _id;

        [NotNull]
        private readonly IAdMobAdListener _listener;

        [CanBeNull]
        private RewardedAd _ad;

        [CanBeNull]
        private TaskCompletionSource<(AdResult, string)> _tcs;

        private bool _isRewarded;

        public bool IsLoaded => _ad != null;

        public AdMobRewardedAdBridge(
            [NotNull] string id,
            [NotNull] IAdMobAdListener listener
        ) {
            _id = id;
            _listener = listener;
        }

        public void Dispose() {
            _ad?.Destroy();
            _ad = null;
        }

        public void Initialize() { }

        public async Task<(bool, string)> Load() {
            _ad?.Destroy();
            _ad = null;
            var tcs = new TaskCompletionSource<(RewardedAd, string)>();
            // ReSharper disable once VariableHidesOuterVariable
            RewardedAd.Load(_id, new AdRequest(), (ad, error) => {
                if (error != null || ad == null) {
                    tcs.SetResult((null, error?.GetMessage() ?? ""));
                    return;
                }
                ad.OnAdFullScreenContentOpened += () => _isRewarded = false;
                ad.OnAdFullScreenContentFailed += adError => _tcs?.SetResult((AdResult.Failed, adError.GetMessage()));
                ad.OnAdFullScreenContentClosed += async () => {
                    await Task.Delay(500);
                    _tcs?.SetResult(_isRewarded
                        ? (AdResult.Completed, "")
                        : (AdResult.Canceled, "Canceled")
                    );
                };
                ad.OnAdPaid += adValue => {
                    var info = ad.GetResponseInfo()?.GetLoadedAdapterResponseInfo();
                    _listener.OnAdPaid(
                        value: adValue.Value,
                        currencyCode: adValue.CurrencyCode,
                        format: AdFormat.Rewarded,
                        adUnit: _id,
                        adSourceName: info?.AdSourceName ?? ""
                    );
                };
                tcs.SetResult((ad, null));
            });
            var (ad, message) = await tcs.Task;
            _ad = ad;
            return (ad != null, message);
        }

        public async Task<(AdResult, string)> Show() {
            Assert.IsNotNull(_ad);
            if (!_ad.CanShowAd()) {
                return (AdResult.Failed, "Failed to show");
            }
            try {
                _tcs = new TaskCompletionSource<(AdResult, string)>();
                _ad.Show(_ => _isRewarded = true);
                return await _tcs.Task;
            } finally {
                _tcs = null;
            }
        }
    }
}