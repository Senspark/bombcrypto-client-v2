using System.Threading.Tasks;

using GoogleMobileAds.Api;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.Assertions;

namespace Senspark.Internal {
    internal class AdMobAppOpenAdBridge : IFullScreenAdBridge {
        [NotNull]
        private readonly string _id;

        [NotNull]
        private readonly IAdMobAdListener _listener;

        [CanBeNull]
        private AppOpenAd _ad;

        [CanBeNull]
        private TaskCompletionSource<(AdResult, string)> _tcs;

        public bool IsLoaded => _ad != null;

        public AdMobAppOpenAdBridge(
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
            var tcs = new TaskCompletionSource<(AppOpenAd, string)>();
            // ReSharper disable once VariableHidesOuterVariable
            AppOpenAd.Load(_id, Screen.orientation, new AdRequest(), (ad, error) => {
                if (error != null || ad == null) {
                    tcs.SetResult((null, error?.GetMessage() ?? ""));
                    return;
                }
                ad.OnAdFullScreenContentFailed += adError => _tcs?.SetResult((AdResult.Failed, adError.GetMessage()));
                ad.OnAdFullScreenContentClosed += () => _tcs?.SetResult((AdResult.Completed, ""));
                ad.OnAdPaid += adValue => {
                    var info = ad.GetResponseInfo()?.GetLoadedAdapterResponseInfo();
                    _listener.OnAdPaid(
                        value: adValue.Value,
                        currencyCode: adValue.CurrencyCode,
                        format: AdFormat.AppOpen,
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
                _ad.Show();
                return await _tcs.Task;
            } finally {
                _tcs = null;
            }
        }
    }
}