using JetBrains.Annotations;

namespace Senspark.Internal {
    internal interface IAdMobAdListener {
        void OnAdPaid(
            long value,
            [NotNull] string currencyCode,
            AdFormat format,
            [NotNull] string adUnit,
            [NotNull] string adSourceName
        );
    }
}