using JetBrains.Annotations;

using Manager;

using Senspark.Internal;

namespace Senspark {
    public static class CostCenterAnalyticsManager {
        public class Builder {
            [NotNull]
            public IRevenueValidator RevenueValidator { get; set; }

            [NotNull]
            public IAnalyticsManager Build() {
#if !UNITY_WEBGL
                UnityEngine.Assertions.Assert.IsNotNull(RevenueValidator, $"{nameof(IRevenueValidator)} is null");
                var bridge = new CostCenterAnalyticsBridge(RevenueValidator);
                return new BaseAnalyticsManager(bridge);
#else
                return new NullAnalyticsManager();
#endif
            }
        }
    }
}