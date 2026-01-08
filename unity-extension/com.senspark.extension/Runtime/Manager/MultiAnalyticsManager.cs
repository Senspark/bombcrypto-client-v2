using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark {
    public class MultiAnalyticsManager : IAnalyticsManager {
        [NotNull]
        private readonly IAnalyticsManager[] _items;

        public MultiAnalyticsManager([NotNull] params IAnalyticsManager[] items) {
            _items = items;
        }

        public async Task Initialize(float timeOut) {
            await Task.WhenAll(_items.Select(item => item.Initialize(timeOut)));
        }

        public void PushScreen(string screenName) {
            foreach (var item in _items) {
                item.PushScreen(screenName);
            }
        }

        public bool PopScreen() {
            var result = false;
            foreach (var item in _items) {
                if (item.PopScreen()) {
                    result = true;
                }
            }
            return result;
        }

        public void PushParameter(string key, object value) {
            foreach (var item in _items) {
                item.PushParameter(key, value);
            }
        }

        public bool PopParameter(string key) {
            var result = false;
            foreach (var item in _items) {
                if (item.PopParameter(key)) {
                    result = true;
                }
            }
            return result;
        }

        public void LogEvent(string name) {
            foreach (var item in _items) {
                item.LogEvent(name);
            }
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            foreach (var item in _items) {
                item.LogEvent(name, parameters);
            }
        }

        public void LogEvent(IAnalyticsEvent analyticsEvent) {
            foreach (var item in _items) {
                item.LogEvent(analyticsEvent);
            }
        }

        public void PushGameLevel(int levelNo, string levelMode) {
            foreach (var item in _items) {
                item.PushGameLevel(levelNo, levelMode);
            }
        }

        public void PopGameLevel(bool winGame) {
            foreach (var item in _items) {
                item.PopGameLevel(winGame);
            }
        }

        public void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            string currencyCode,
            AdFormat format,
            string adUnit
        ) {
            foreach (var item in _items) {
                item.LogAdRevenue(
                    mediationNetwork,
                    monetizationNetwork,
                    revenue,
                    currencyCode,
                    format,
                    adUnit
                );
            }
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
            foreach (var item in _items) {
                item.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt);
            }
        }

        public void LogEarnResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
            foreach (var i in _items) {
                i.LogEarnResource(playMode, level, resourceName, amount, balance, item, itemType, booster);;
            }
        }

        public void LogSpendResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
            foreach (var i in _items) {
                i.LogSpendResource(playMode, level, resourceName, amount, balance, item, itemType, booster);;
            }
        }
    }
}