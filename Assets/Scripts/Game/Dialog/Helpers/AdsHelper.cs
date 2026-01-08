using System;
using System.Threading.Tasks;

using Analytics;

using App;

using Services.IapAds;

using UnityEngine;

namespace Utils {
    public static class AdsHelper {
        public static async Task ShowInterstitial(
            IUnityAdsManager unityAdsManager,
            IServerManager serverManager,
            IAnalytics analytics,
            AdsCategory adsCategory,
            InterstitialCategory interstitialCategory
        ) {
            try {
                var noAds = await serverManager.BomberLand.GetRemoveInterstitialAds();
                if (noAds) {
                    return;
                }
                analytics.TrackInterstitial(adsCategory, TrackAdsResult.Start);
                var result = await unityAdsManager.ShowInterstitial(interstitialCategory);
                switch (result) {
                    case AdResult.Done:
                        analytics.TrackInterstitial(adsCategory, TrackAdsResult.Complete);
                        break;
                    case AdResult.Error:
                        analytics.TrackInterstitial(adsCategory, TrackAdsResult.Error);
                        break;
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }
}