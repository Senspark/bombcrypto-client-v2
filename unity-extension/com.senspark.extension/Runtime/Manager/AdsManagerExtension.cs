using System.Collections.Generic;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark {
    public static class AdsManagerExtension {
        [NotNull]
        public static Task<(AdResult, string)> ShowAppOpenAd(this IAdsManager manager) {
            return manager.ShowFullScreenAd(AdFormat.AppOpen);
        }
        
        [NotNull]
        public static Task<(AdResult, string)> ShowAppOpenAd(this IAdsManager manager,
            Dictionary<string, object> extraParams) {
            return manager.ShowFullScreenAd(AdFormat.AppOpen, extraParams);
        }

        [NotNull]
        public static Task<(AdResult, string)> ShowInterstitialAd(this IAdsManager manager) {
            return manager.ShowFullScreenAd(AdFormat.Interstitial);
        }
        
        [NotNull]
        public static Task<(AdResult, string)> ShowInterstitialAd(this IAdsManager manager,
            Dictionary<string, object> extraParams) {
            return manager.ShowFullScreenAd(AdFormat.Interstitial, extraParams);
        }

        [NotNull]
        public static Task<(AdResult, string)> ShowRewardedInterstitialAd(this IAdsManager manager) {
            return manager.ShowFullScreenAd(AdFormat.RewardedInterstitial);
        }
        
        [NotNull]
        public static Task<(AdResult, string)> ShowRewardedInterstitialAd(this IAdsManager manager,
            Dictionary<string, object> extraParams) {
            return manager.ShowFullScreenAd(AdFormat.RewardedInterstitial, extraParams);
        }

        [NotNull]
        public static Task<(AdResult, string)> ShowRewardedAd(this IAdsManager manager) {
            return manager.ShowFullScreenAd(AdFormat.Rewarded);
        }
        
        [NotNull]
        public static Task<(AdResult, string)> ShowRewardedAd(this IAdsManager manager,
            Dictionary<string, object> extraParams) {
            return manager.ShowFullScreenAd(AdFormat.Rewarded, extraParams);
        }
    }
}