using System;

using GoogleMobileAds.Api;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark.Internal {
    internal class AdMobBannerAdBridge : IBannerAdBridge {
        [NotNull]
        private readonly string _id;

        private readonly bool _isAdaptive;
        private readonly bool _isRect;

        [NotNull]
        private readonly IAdMobAdListener _listener;

        [CanBeNull]
        private BannerView _ad;

        public bool IsLoaded { get; private set; }
        public Vector2 Size { get; private set; }

        public AdMobBannerAdBridge(
            [NotNull] string id,
            bool isAdaptive,
            bool isRect,
            [NotNull] IAdMobAdListener listener
        ) {
            _id = id;
            _isAdaptive = isAdaptive;
            _isRect = isRect;
            _listener = listener;
        }

        public void Dispose() {
            _ad?.Destroy();
            _ad = null;
        }

        public void Initialize(Action<bool, string> loadCallback) {
            var size = _isRect
                ? AdSize.MediumRectangle
                : _isAdaptive
                    ? AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth)
                    : AdSize.Banner;
            _ad = new BannerView(_id, size, GoogleMobileAds.Api.AdPosition.Bottom);
            _ad.OnBannerAdLoaded += () => {
                IsLoaded = true;
                var normalizedSize = new Vector2(_ad.GetWidthInPixels(), _ad.GetHeightInPixels());
                if (Application.isEditor) {
                    // Default prefab canvas height is 1080.
                    normalizedSize *= Screen.height / 1080f;
                }
                Size = normalizedSize;
                loadCallback?.Invoke(true, "");
            };
            _ad.OnBannerAdLoadFailed += error => { //
                loadCallback?.Invoke(false, error.GetMessage());
            };
            _ad.OnAdPaid += adValue => {
                var info = _ad.GetResponseInfo()?.GetLoadedAdapterResponseInfo();
                _listener.OnAdPaid(
                    value: adValue.Value,
                    currencyCode: adValue.CurrencyCode,
                    format: _isRect ? AdFormat.Rect : AdFormat.Banner,
                    adUnit: _id,
                    adSourceName: info?.AdSourceName ?? ""
                );
            };
            _ad.LoadAd(new AdRequest());
        }

        public void SetVisible(bool visible) {
            if (visible) {
                _ad?.Show();
            } else {
                _ad?.Hide();
            }
        }

        public void SetPosition(AdPosition position) {
            _ad?.SetPosition(position switch {
                AdPosition.Top => GoogleMobileAds.Api.AdPosition.Top,
                AdPosition.Bottom => GoogleMobileAds.Api.AdPosition.Bottom,
                _ => throw new ArgumentOutOfRangeException(),
            });
        }

        public void SetPosition(Vector2 position) {
            if (Application.isEditor) {
                // Default prefab canvas height is 1080.
                position /= Screen.height / 1080f;
            } else {
                position /= MobileAds.Utils.GetDeviceScale();
            }
            _ad?.SetPosition((int) position.x, (int) position.y);
        }
    }
}