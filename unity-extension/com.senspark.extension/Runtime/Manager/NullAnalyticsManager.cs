using System.Collections.Generic;
using System.Threading.Tasks;

namespace Senspark.Internal {
    public class NullAnalyticsManager : IAnalyticsManager {
        public Task Initialize(float timeOut) {
            return Task.CompletedTask;
        }

        public void PushScreen(string screenName) {
        }

        public bool PopScreen() {
            return false;
        }

        public void PushParameter(string key, object value) {
        }

        public bool PopParameter(string key) {
            return false;
        }

        public void LogEvent(string name) {
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
        }

        public void LogEvent(IAnalyticsEvent analyticsEvent) {
        }

        public void PushGameLevel(int levelNo, string levelMode) {
        }

        public void PopGameLevel(bool winGame) {
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue,
            string currencyCode, AdFormat format, string adUnit) {
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
        }

        public void LogEarnResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
            
        }

        public void LogSpendResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
        }
    }
}