using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if UNITY_ANDROID || UNITY_IOS
using Firebase.Analytics;
#endif

using Manager;

using Senspark;

using UnityEngine;

namespace Analytics {
    public class MobileCostCenterAnalyticsBridge : IAnalyticsBridge {
        private bool _initialized;
        private readonly IAnalyticsManager _bridge;

        public MobileCostCenterAnalyticsBridge(IRevenueValidator revenueValidator) {
            _bridge = new CostCenterAnalyticsManager.Builder {
                RevenueValidator = revenueValidator
            }.Build();
        }
        
        public async Task<bool> Initialize() {
            await _bridge.Initialize(2);
            return true;
        }

        public void LogScene(string sceneName) {
        }

        public void LogEvent(string name) {
        }

        public void LogEvent(string name, Parameter[] parameters) {
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
        }

        public void PushGameLevel(int levelNo, string levelMode) {
            _bridge.PushGameLevel(levelNo, levelMode);
        }

        public void PopGameLevel(bool winGame) {
            _bridge.PopGameLevel(winGame);
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit, Dictionary<string, object> extraParameters) {
            _bridge.LogAdRevenue(mediationNetwork, monetizationNetwork, revenue, currencyCode, format, adUnit);
        }

        
        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
            //_bridge.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt);
#if UNITY_ANDROID || UNITY_IOS
            // _bridge LogIapRevenue sẽ chỉ gọi log khi nhận OnReceivedIapValidatedInfo từ AppsFlyer
            // bombcrypto dùng validated iap riêng từ server 
            // => không gọi log từ _bridge mà thay bằng log trực tiếp tại đây.
            var parameters = new List<Firebase.Analytics.Parameter> {
                new(FirebaseAnalytics.ParameterValue, priceValue),
                new(FirebaseAnalytics.ParameterCurrency, currencyIso),
                new("product_id", packageName),
                new("order_id", orderId),
            };
            FirebaseAnalytics.LogEvent("iap_sdk", parameters.ToArray());
#endif
        }
    }
}