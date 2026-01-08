using JetBrains.Annotations;

namespace Senspark.Internal {
    public static class AdExtensions {
        [NotNull]
        public static IAd AutoLoad([NotNull] this IAd ad) {
            return new AutoLoadAd(ad);
        }

        [NotNull]
        public static IBannerAd AutoLoad([NotNull] this IBannerAd ad) {
            return new AutoLoadBannerAd(ad);
        }

        [NotNull]
        public static IFullScreenAd AutoLoad([NotNull] this IFullScreenAd ad) {
            return new AutoLoadFullScreenAd(ad);
        }
    }
}