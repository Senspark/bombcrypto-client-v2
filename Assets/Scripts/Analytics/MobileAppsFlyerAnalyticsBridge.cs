using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using AppsFlyerSDK;

using Senspark;

#if UNITY_ANDROID || UNITY_IOS
using System.Collections.Generic;

using AppsFlyerSDK;
#endif

namespace Analytics {
    public class MobileAppsFlyerAnalyticsBridge : IAppsFlyerAnalyticsBridge {
        private readonly ILogManager _logManager;
        private bool _initialized;
        private readonly IAnalyticsManager _bridge;
        
        public MobileAppsFlyerAnalyticsBridge(
            ILogManager logManager,
            string iosAppId,
            string devKey,
            bool enableDebug
        ) {
            _logManager = logManager;
            _bridge = new Senspark.AppsFlyerManager.Builder {
                DevKey = devKey,
                AppId = iosAppId,
                IsDebug = enableDebug
            }.Build();
        }

        public async Task<bool> Initialize() {
#if UNITY_ANDROID || UNITY_IOS
            if (!_initialized) {
                _initialized = true;
                await _bridge.Initialize(2);
            }
#endif
            return true;
        }

        public void LogScene(string sceneName) {
        }

        public void SetWalletAddress(string address) {
#if UNITY_ANDROID || UNITY_IOS
            AppsFlyer.setCustomerUserId(address);
#endif
        }

        public void LogEvent(string name) {
            _logManager.Log(name);
#if UNITY_ANDROID || UNITY_IOS
            AppsFlyer.sendEvent(name, new Dictionary<string, string>());
#endif
        }

        public void LogEvent(string name, Parameter[] parameters) {
            var data = parameters.ToDictionary(p => p.name, p => p.ToTrackingString());
            AppsFlyer.sendEvent(name, data);
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            var data = new Dictionary<string, string>();
            foreach (var (k, v) in parameters) {
                switch (v) {
                    case double d:
                        data.Add(k, d.ToString(CultureInfo.InvariantCulture));
                        break;
                    case float f:
                        data.Add(k, f.ToString(CultureInfo.InvariantCulture));
                        break;
                    default:
                        data.Add(k, v.ToString());
                        break;
                }
            }
            AppsFlyer.sendEvent(name, data);
        }

        public void PushGameLevel(int levelNo, string levelMode) {
        }

        public void PopGameLevel(bool winGame) {
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit, Dictionary<string, object> extraParameters) {
            _bridge.LogAdRevenue(mediationNetwork, monetizationNetwork, revenue, currencyCode, format, adUnit);

        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) {
            _bridge.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt);
        }
    }
}