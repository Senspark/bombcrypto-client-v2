using JetBrains.Annotations;

using Manager;

using Senspark.Internal;

namespace Senspark {
    public static class AppsFlyerManager {
        public class Builder {
            [CanBeNull]
            public string DevKey { get; set; }

            [CanBeNull]
            public string AppId { get; set; }

            public bool IsDebug { get; set; }

            [NotNull]
            public IAnalyticsManager Build() {
#if !UNITY_WEBGL
                if (DevKey == null) {
                    throw new System.ArgumentNullException(DevKey, "DevKey is null");
                }
                var bridge = new AppsFlyerBridge(DevKey, AppId, IsDebug);
                return new BaseAnalyticsManager(bridge);
#else
                return new NullAnalyticsManager();
#endif
            }
        }
    }

    public static class AppsFlyerValidator {
        public class Builder {
            public IRevenueValidator Build() {
#if !UNITY_WEBGL
                return new AppsFlyerValidatorBridge();
#else
                return new NullRevenueValidator();
#endif
            }
        }
    }
}