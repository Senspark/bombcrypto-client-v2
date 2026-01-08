using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark.Internal {
    public class BaseAdsManager : ObserverManager<AdsManagerObserver>, IAdsManager, IAdListener {
        [NotNull]
        private readonly IAdBridge _bridge;

        [NotNull]
        private readonly IAnalyticsManager _analyticsManager;

        [NotNull]
        private readonly IAudioManager _audioManager;

        [NotNull]
        private readonly IBannerAd _bannerAd;

        [NotNull]
        private readonly IBannerAd _rectAd;

        [NotNull]
        private readonly Dictionary<AdFormat, IFullScreenAd> _fullScreenAds;

        [CanBeNull]
        private Task _initializer;

        private bool _adLocker;
        private bool _isDisplayingAd;
        private ObserverHandle _handle;

        public bool IsBannerAdLoaded => _bannerAd.IsLoaded;

        public bool IsBannerAdVisible {
            get => _bannerAd.IsVisible;
            set => _bannerAd.IsVisible = value;
        }

        public AdPosition BannerAdRelativePosition {
            get => _bannerAd.RelativePosition;
            set => _bannerAd.RelativePosition = value;
        }

        public Vector2 BannerAdPosition {
            get => _bannerAd.Position;
            set => _bannerAd.Position = value;
        }

        public Vector2 BannerAdAnchor {
            get => _bannerAd.Anchor;
            set => _bannerAd.Anchor = value;
        }

        public Vector2 BannerAdSize => _bannerAd.Size;

        public bool IsRectAdLoaded => _rectAd.IsLoaded;

        public bool IsRectAdVisible {
            get => _rectAd.IsVisible;
            set => _rectAd.IsVisible = value;
        }

        public Vector2 RectAdPosition {
            get => _rectAd.Position;
            set => _rectAd.Position = value;
        }

        public Vector2 RectAdAnchor {
            get => _rectAd.Anchor;
            set => _rectAd.Anchor = value;
        }

        public Vector2 RectAdSize => _rectAd.Size;

        public BaseAdsManager(
            [NotNull] IAdBridge bridge,
            [CanBeNull] Action bannerAdLoadedCallback,
            [NotNull] IAnalyticsManager analyticsManager,
            [NotNull] IAudioManager audioManager
        ) {
            _bridge = bridge;
            _analyticsManager = analyticsManager;
            _audioManager = audioManager;
            _bannerAd = _bridge.CreateBannerAd();
            _bannerAd.AddObserver(new AdObserver {
                OnLoaded = () => bannerAdLoadedCallback?.Invoke(), //
            });
            _rectAd = _bridge.CreateRectAd();
            _fullScreenAds = new Dictionary<AdFormat, IFullScreenAd> {
                [AdFormat.AppOpen] = _bridge.CreateAppOpenAd(),
                [AdFormat.Interstitial] = _bridge.CreateInterstitialAd(),
                [AdFormat.RewardedInterstitial] = _bridge.CreateRewardedInterstitialAd(),
                [AdFormat.Rewarded] = _bridge.CreateRewardedAd(),
            };
            
            // Observers
            {
                _handle = new ObserverHandle();
                foreach (var k in _fullScreenAds.Keys) {
                    _handle.AddObserver(GetObserver(k), new AdObserver {
                        OnLoaded = () => DispatchEvent(e => e.OnAdLoad?.Invoke(k, true)),
                        OnFailedToLoad = _ => DispatchEvent(e => e.OnAdLoad?.Invoke(k, false)),
                    });
                }
                return;
                IAd GetObserver(AdFormat adFormat) {
                    return adFormat switch {
                        AdFormat.Banner => _bannerAd,
                        AdFormat.Rect => _rectAd,
                        _ => _fullScreenAds[adFormat]
                    };
                }    
            }
        }

        public Task Initialize(float timeOut, Action<bool> callback) =>
            _initializer ??= InitializeImpl(timeOut, callback);

        [NotNull]
        private async Task InitializeImpl(float timeOut, [CanBeNull] Action<bool> callback) {
            var isTimeOut = false;
            await Task.WhenAny(((Func<Task>) (async () => {
                var result = await _bridge.Initialize(this);
                _bannerAd.Initialize();
                _rectAd.Initialize();
                foreach (var (_, ad) in _fullScreenAds) {
                    ad.Initialize();
                }
                if (!isTimeOut) {
                    callback?.Invoke(result);
                }
            }))(), ((Func<Task>) (async () => {
                await Task.Delay((int) (timeOut * 1000));
                isTimeOut = true;
                callback?.Invoke(false);
            }))(), _analyticsManager.Initialize(timeOut), _audioManager.Initialize());
        }

        public async Task OpenInspector() {
            await _bridge.OpenInspector();
        }

        public bool IsAdLoaded(AdFormat format) {
            return _fullScreenAds[format].IsLoaded;
        }
        
        public async UniTask<bool> WaitForAdLoaded(AdFormat format, float timeOutSeconds = 60) {
            if (IsAdLoaded(format)) {
                return true;
            }
            var taskSrc = new UniTaskCompletionSource<bool>();
            var observerId = _fullScreenAds[format].AddObserver(new AdObserver {
                OnLoaded = () => taskSrc.TrySetResult(true),
                OnFailedToLoad = _ => taskSrc.TrySetResult(false),
            });
            using var cancellation = new CancellationTokenSource();
            cancellation.Token.Register(() => taskSrc.TrySetResult(false));
            cancellation.CancelAfter(TimeSpan.FromSeconds(timeOutSeconds));
            var result = await taskSrc.Task;
            _fullScreenAds[format].RemoveObserver(observerId);
            if (!cancellation.IsCancellationRequested) {
                cancellation.Cancel();
            }
            return result;
        }

        public Task<(AdResult, string)> ShowFullScreenAd(AdFormat format) {
            return ShowFullScreenAd(format, null);
        }

        public async Task<(AdResult, string)> ShowFullScreenAd(AdFormat format, Dictionary<string, object> extraParams) {
            var (result, message) = await ShowFullScreenAdInternal(format);
            if (_isDisplayingAd) {
                return (result, message);
            }
            
            // Track Ad Conversion
            if (result == AdResult.Completed) {
                _analyticsManager.LogEvent(format switch {
                    AdFormat.AppOpen => "conv_app_open_completed",
                    AdFormat.Interstitial => "conv_interstitial_completed",
                    AdFormat.RewardedInterstitial => "conv_interstitial_rewarded_completed",
                    AdFormat.Rewarded => "conv_rewarded_completed",
                    _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
                });
            }
            
            // Track Ad Done
            var trackAdParams = new Dictionary<string, object> {
                ["ad_format"] = format switch {
                    AdFormat.AppOpen => "app_open",
                    AdFormat.Interstitial => "interstitial",
                    AdFormat.RewardedInterstitial => "rewarded_interstitial",
                    AdFormat.Rewarded => "rewarded",
                    _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
                },
                ["ad_result"] = result switch {
                    AdResult.NotConfigured => "not_configured",
                    AdResult.NotInitialized => "not_initialized",
                    AdResult.NotLoaded => "not_loaded",
                    AdResult.Canceled => "canceled",
                    AdResult.Completed => "completed",
                    AdResult.Capped => "capped",
                    AdResult.Failed => "failed",
                    _ => throw new ArgumentOutOfRangeException()
                },
                ["message"] = message
            };
            InjectExtraParams(trackAdParams, extraParams);
            _analyticsManager.LogEvent("track_ad", trackAdParams);
            
            return (result, message);
        }

        [NotNull]
        private async Task<(AdResult, string)> ShowFullScreenAdInternal(AdFormat format) {
            if (_adLocker) {
                return (AdResult.Capped, "Capped");
            }
            var wasEnabled = (_audioManager.IsMusicEnabled, _audioManager.IsSoundEnabled);
            try {
                _adLocker = true;
                _isDisplayingAd = true;
                _audioManager.IsMusicEnabled = _audioManager.IsSoundEnabled = false;
                return await BlockDialog.Block(async () => {
                    var ad = _fullScreenAds[format];
                    return await ad.Show();
                });
            } finally {
                (_audioManager.IsMusicEnabled, _audioManager.IsSoundEnabled) = wasEnabled;
                _isDisplayingAd = false;
                _ = ((Func<Task>) (async () => {
                    await Task.Delay(1000);
                    _adLocker = false;
                }))();
            }
        }

        public void OnAdPaid(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            string currencyCode,
            AdFormat format,
            string adUnit
        ) {
            _analyticsManager.LogAdRevenue(
                mediationNetwork: mediationNetwork,
                monetizationNetwork: monetizationNetwork,
                revenue: revenue,
                currencyCode: currencyCode,
                format: format,
                adUnit: adUnit
            );
        }

        private static void InjectExtraParams(Dictionary<string, object> baseObj, Dictionary<string, object> addition) {
            if (addition == null) {
                return;
            }
            foreach (var (key, value) in addition) {
                baseObj[key] = value;
            }
        }
    }
}