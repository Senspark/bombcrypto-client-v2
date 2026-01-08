using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    public interface IAdListener {
        void OnAdPaid(
            AdNetwork mediationNetwork,
            [NotNull] string monetizationNetwork,
            double revenue,
            [NotNull] string currencyCode,
            AdFormat format,
            [NotNull] string adUnit
        );
    }

    public interface IAdBridge {
        [NotNull]
        Task<bool> Initialize([NotNull] IAdListener listener);

        [NotNull]
        Task OpenInspector();

        [NotNull]
        IBannerAd CreateBannerAd();

        [NotNull]
        IBannerAd CreateRectAd();

        [NotNull]
        IFullScreenAd CreateAppOpenAd();

        [NotNull]
        IFullScreenAd CreateInterstitialAd();

        [NotNull]
        IFullScreenAd CreateRewardedInterstitialAd();

        [NotNull]
        IFullScreenAd CreateRewardedAd();
    }
}