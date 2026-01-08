using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Senspark;

using Utils;

namespace Services.IapAds {
    public enum AdResult {
        Cancel, Skip, Error, Done 
    }

    public enum InterstitialCategory {
        Unknown, PveBegin, PveEnd, PvpEnd
    }

    public static class UnityAdsExtensions  {
        public static Map<InterstitialCategory, string> ConfigEnum = new(
            new Dictionary<InterstitialCategory, string> {
                [InterstitialCategory.PveBegin] = "pve_begin",
                [InterstitialCategory.PveEnd] = "pve_end",
                [InterstitialCategory.PvpEnd] = "pvp_end",
            });
    }


    public class AdException : Exception {
        public readonly AdResult Result;

        public AdException(AdResult result, string message) : base(message) {
            Result = result;
        }
    }

    [Service(nameof(IUnityAdsManager))]
    public interface IUnityAdsManager : IObserverManager<AdsManagerObserver>,  IService {
        bool IsAdLoaded();
        Task<string> ShowRewarded();
        Task<AdResult> ShowInterstitial(InterstitialCategory category);
    }
}