using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace Senspark {
    public static class AdMobAdsManager {
        public class Builder {
            [CanBeNull]
            public string BannerAdId { get; set; }

            [CanBeNull]
            public string RectAdId { get; set; }

            [CanBeNull]
            public string AppOpenAdId { get; set; }

            [CanBeNull]
            public string InterstitialAdId { get; set; }

            [CanBeNull]
            public string RewardedInterstitialAdId { get; set; }

            [CanBeNull]
            public string RewardedAdId { get; set; }

            public bool UseAdaptiveBannerAd { get; set; }

            [CanBeNull]
            public Action BannerAdLoadedCallback { get; set; }

            [CanBeNull]
            public List<string> TestDeviceIds { get; set; }

            [CanBeNull]
            public IAnalyticsManager AnalyticsManager { get; set; }

            [CanBeNull]
            public IAudioManager AudioManager { get; set; }

            [NotNull]
            public IAdsManager Build() {
#if UNITY_WEBGL
                return BuildWebGL();
#else
                return BuildMobile();
#endif
            }

#if !UNITY_WEBGL
            private IAdsManager BuildMobile() {
                var logManager = new UnityLogManager();
                var serviceLocator = new ServiceLocator();
                serviceLocator.Provide(logManager);
                var bridge = new Internal.AdMobAdBridge(
                    serviceLocator: serviceLocator,
                    bannerAdId: BannerAdId,
                    rectAdId: RectAdId,
                    appOpenAdId: AppOpenAdId,
                    interstitialAdId: InterstitialAdId,
                    rewardedInterstitialAdId: RewardedInterstitialAdId,
                    rewardedAdId: RewardedAdId,
                    useAdaptiveBannerAd: UseAdaptiveBannerAd,
                    testDeviceIds: TestDeviceIds
                );
                var analyticsManager = AnalyticsManager ?? new UnityAnalyticsManager();
                var audioManager = AudioManager ?? new NullAudioManager();
                return new Internal.BaseAdsManager(bridge, BannerAdLoadedCallback, analyticsManager, audioManager);
            }
#endif

#if UNITY_WEBGL
            private IAdsManager BuildWebGL() {
                var analyticsManager = AnalyticsManager ?? new UnityAnalyticsManager();
                var audioManager = AudioManager ?? new NullAudioManager();
                return new NullAdsManager(analyticsManager, audioManager);
            }
#endif
        }
    }
}