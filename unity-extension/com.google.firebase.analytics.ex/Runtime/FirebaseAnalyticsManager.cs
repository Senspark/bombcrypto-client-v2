using JetBrains.Annotations;

using Senspark.Internal;

namespace Senspark {
    public static class FirebaseAnalyticsManager {
        public class Builder {
            [NotNull]
            public IAnalyticsManager Build() {
#if !UNITY_WEBGL
                var bridge = new FirebaseAnalyticsBridge();
                return new BaseAnalyticsManager(bridge);
#else
                return new NullAnalyticsManager();
#endif
            }
        }
    }
}