using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using UnityEngine;

namespace Senspark {
    public enum AdNetwork {
        AdColony,
        AdMob,
        AppLovin,
        InMobi,
        IronSource,
        MetaAudienceNetwork,
        Mintegral,
        Pangle,
        PubMatic,
        Smaato,
        Tapjoy,
        UnityAds,
        LiftoffMonetize,
    }

    public enum AdFormat {
        Banner,
        Rect,
        AppOpen,
        Interstitial,
        RewardedInterstitial,
        Rewarded,
    }

    /// <summary>
    /// Predefined position for banner ad (320x50).
    /// </summary>
    public enum AdPosition {
        Top,
        Bottom,
    }

    /// <summary>
    /// Result of displaying full-screen ads.
    /// </summary>
    public enum AdResult {
        /// <summary>
        /// Ad ID is not set.
        /// </summary>
        NotConfigured,

        /// <summary>
        /// SDK is not initialized.
        /// </summary>
        NotInitialized,

        /// <summary>
        /// Not loaded yet.
        /// </summary>
        NotLoaded,

        /// <summary>
        /// Cancel the ad (rewarded/rewarded interstitial).
        /// </summary>
        Canceled,

        /// <summary>
        /// Completes the full-screen ad.
        /// </summary>
        Completed,

        /// <summary>
        /// Capped.
        /// </summary>
        Capped,

        /// <summary>
        /// Other errors.
        /// </summary>
        Failed,
    }

    public class AdsManagerObserver {
        public Action<AdFormat, bool> OnAdLoad;
    }

    [Service(typeof(IAdsManager))]
    public interface IAdsManager : IObserverManager<AdsManagerObserver> {
        /// <summary>
        /// Gets whether the banner ad is loaded.
        /// </summary>
        bool IsBannerAdLoaded { get; }

        /// <summary>
        /// Gets or sets the banner ad visibility.
        /// </summary>
        bool IsBannerAdVisible { get; set; }

        /// <summary>
        /// Gets or sets the banner ad relative position.
        /// </summary>
        AdPosition BannerAdRelativePosition { get; set; }

        /// <summary>
        /// Gets or sets the banner ad position.
        /// </summary>
        Vector2 BannerAdPosition { get; set; }

        /// <summary>
        /// Gets or sets the banner ad anchor.
        /// </summary>
        Vector2 BannerAdAnchor { get; set; }

        /// <summary>
        /// Gets the banner ad size.
        /// </summary>
        Vector2 BannerAdSize { get; }

        /// <summary>
        /// Gets whether the rect ad is loaded.
        /// </summary>
        bool IsRectAdLoaded { get; }

        /// <summary>
        /// Gets or sets the rect ad visibility.
        /// </summary>
        bool IsRectAdVisible { get; set; }

        /// <summary>
        /// Gets or sets the rect ad position.
        /// </summary>
        Vector2 RectAdPosition { get; set; }

        /// <summary>
        /// Gets or sets the rect ad anchor.
        /// </summary>
        Vector2 RectAdAnchor { get; set; }

        /// <summary>
        /// Gets the rect ad size.
        /// </summary>
        Vector2 RectAdSize { get; }

        /// <summary>
        /// Initializes the service.
        /// </summary>
        [NotNull]
        Task Initialize(float timeOut, [CanBeNull] Action<bool> callback);

        /// <summary>
        /// Opens the ad inspector.
        /// </summary>
        [NotNull]
        Task OpenInspector();

        /// <summary>
        /// Checks whether the specified ad is loaded.
        /// </summary>
        [Pure]
        bool IsAdLoaded(AdFormat format);

        /// <summary>
        /// Attempts to show a full-screen ad.
        /// </summary>
        [NotNull]
        Task<(AdResult, string)> ShowFullScreenAd(AdFormat format);
        
        /// <summary>
        /// Attempts to show a full-screen ad, with addition parameters used for tracking
        /// </summary>
        [NotNull]
        Task<(AdResult, string)> ShowFullScreenAd(AdFormat format, Dictionary<string, object> extraParams);
    }
}