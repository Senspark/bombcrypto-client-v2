using Senspark.Internal;

namespace Senspark {
    public class UnityAnalyticsManager : BaseAnalyticsManager {
        public UnityAnalyticsManager() : base(new UnityAnalyticsBridge()) { }
    }
}