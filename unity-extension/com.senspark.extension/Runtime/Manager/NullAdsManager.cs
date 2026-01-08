using JetBrains.Annotations;

using Senspark.Internal;

namespace Senspark {
    public class NullAdsManager : BaseAdsManager {
        public NullAdsManager(
            [NotNull] IAnalyticsManager analyticsManager,
            [NotNull] IAudioManager audioManager
        ) : base(
            new NullAdsBridge(),
            null,
            analyticsManager,
            audioManager
        ) { }
    }
}