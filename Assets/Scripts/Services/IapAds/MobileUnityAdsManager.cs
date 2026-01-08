using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Analytics;

using App;

using Senspark;

using GoogleMobileAds.Api;

using Services.RemoteConfig;

using UnityEngine;

namespace Services.IapAds {
    public enum AdViewState {
        NotStart,
        IsViewing,
        Cancel,
        Error,
        Done
    }

    public enum AdLoadState {
        NotLoad,
        Loading,
        Loaded
    }

    /// https://developers.google.com/admob/unity/test-ads
    /// https://developers.google.com/admob/unity/mediation/applovin#import
    /// https://github.com/googleads/googleads-mobile-unity
    /// Current version: https://github.com/googleads/googleads-mobile-unity/releases/tag/v8.0.0
    /// Tham khảo Show Consent nếu cần: https://developers.google.com/admob/unity/privacy
    public class MobileUnityAdsManager : ObserverManager<AdsManagerObserver>, IUnityAdsManager {
        private static readonly List<string> TestDevices = new() {
        };

        private readonly ILogManager _logManager;
        private readonly IRemoteConfig _remoteConfig;
        private readonly UserAccount _userAccount;
        private readonly UnityRewardedAd _rewardedAd;
        private readonly UnityInterstitialAd _interstitialAd;

        private TaskCompletionSource<bool> _initTask;
        private bool _processing;
        private DateTime _lastTimeShowInterstitial = DateTime.MinValue;

        private ObserverHandle _handle;
        
        public MobileUnityAdsManager(
            ILogManager logManager,
            IRemoteConfig remoteConfig,
            UserAccount account,
            IAnalytics analytics
        ) {
            _logManager = logManager;
            _remoteConfig = remoteConfig;
            _userAccount = account;
            _rewardedAd = new UnityRewardedAd(logManager, analytics);
            _interstitialAd = new UnityInterstitialAd(logManager);
        }

        public async Task<bool> Initialize() {
            if (_initTask == null) {
                _initTask = new TaskCompletionSource<bool>();
                var builder = new RequestConfiguration.Builder();
                builder.SetTestDeviceIds(TestDevices);
                builder.SetSameAppKeyEnabled(true);
                MobileAds.RaiseAdEventsOnUnityMainThread = true;
                MobileAds.SetiOSAppPauseOnBackground(true);
                MobileAds.SetRequestConfiguration(builder.build());
                MobileAds.Initialize(InitCompleteAction);
                
                _handle = new ObserverHandle();
                _handle.AddObserver(_rewardedAd, new AdsManagerObserver {
                    OnAdLoad = (adFormat, isLoaded) => {
                        DispatchEvent(e => e.OnAdLoad?.Invoke(adFormat, isLoaded));
                        // retry reload rewarded ad
                        if (!isLoaded) {
                            _rewardedAd.CreateNewAd();
                        }
                    }
                });
                
            }
            // await _initTask.Task;
            // no need to wait
            return true;
        }

        public void Destroy() {
        }

        public bool IsAdLoaded() {
            return _rewardedAd.IsAdLoaded();
        }
        
        public async Task<string> ShowRewarded() {
            DispatchEvent(e => e.OnAdLoad?.Invoke(AdFormat.Rewarded, false));
            if (!CanShowVideoAd()) {
                throw new AdException(AdResult.Error, "No ads");
            }
            _processing = true;
            var ssvId = $"{_userAccount.id}-{DateTime.Now.ToString("yyMMddHHmmssff")}";
            var result = await _rewardedAd.ShowAd(ssvId);
            _processing = false;
            return result switch {
                AdResult.Cancel => throw new AdException(result, "User cancelled"),
                AdResult.Error => throw new AdException(result, "No ads"),
                _ => ssvId
            };
        }

        public async Task<AdResult> ShowInterstitial(InterstitialCategory category) {
            if (!CanShowVideoAd()) {
                return AdResult.Error;
            }
            var config = RemoteConfigHelper.GetInterstitialConfig(_remoteConfig);
            if (config != null) {
                var thisCategory = UnityAdsExtensions.ConfigEnum.Forward[category];
                var hasCategory = config.categories.FirstOrDefault(e => e.category == thisCategory);
                var skipCategory = hasCategory != null && hasCategory.disable;
                
                var seconds = (DateTime.Now - _lastTimeShowInterstitial).TotalSeconds;
                var skipSecond = config.secondBetween > 0 && seconds < config.secondBetween;
                
                if (skipCategory || skipSecond) {
                    Debug.Log("devv skip ads");
                    return AdResult.Skip;
                }
            }
            _processing = true;
            var result = await _interstitialAd.ShowAd();
            if (result == AdResult.Done) {
                _lastTimeShowInterstitial = DateTime.Now;
            }
            _processing = false;
            return result;
        }

        private bool CanShowVideoAd() {
            if (!_initTask.Task.IsCompleted) {
                _logManager.Log($"devv ads is initializing");
                return false;
            }
            if (_processing) {
                _logManager.Log($"devv previous ads still processing");
                return false;
            }
            return !_processing;
        }
        
        private void InitCompleteAction(InitializationStatus initStatus) {
            _rewardedAd.CreateNewAd();
            _interstitialAd.CreateNewAd();
            _initTask.SetResult(true);
            LogInitStatus(initStatus);
        }
        
        private void LogInitStatus(InitializationStatus initStatus) {
            var map = initStatus.getAdapterStatusMap();
            foreach (var (key, status) in map)
            {
                switch (status.InitializationState)
                {
                    case AdapterState.NotReady:
                        _logManager.Log($"devv Adapter: {key} not ready.");
                        break;
                    case AdapterState.Ready:
                        _logManager.Log($"devv Adapter: {key} is initialized.");
                        break;
                }
            }
        }
    }
}