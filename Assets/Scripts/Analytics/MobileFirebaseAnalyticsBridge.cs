using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

#if UNITY_ANDROID || UNITY_IOS
// using Firebase.Analytics;

using UnityEngine;
#endif

namespace Analytics {
    public class MobileFirebaseAnalyticsBridge : IAnalyticsBridge {
        private bool _initialized;
        private Task<bool> _initializationTask;
        private IAnalyticsManager _analytics;
        
        private readonly bool _enableLog;
        private readonly ILogManager _logManager;

        public MobileFirebaseAnalyticsBridge(ILogManager logManager, bool enableLog) {
            _enableLog = enableLog;
            _logManager = logManager;
            _logManager.Log("devv MobileFirebaseAnalyticsBridge");
        }

        public async Task<bool> Initialize() {
#if UNITY_ANDROID || UNITY_IOS
            if (_initialized) {
                return true;
            }
            try {
                _initialized = true;
                _analytics = new FirebaseAnalyticsManager.Builder().Build();
                await _analytics.Initialize(2);
            } catch (Exception e) {
                Debug.LogError(e.Message);
            }
#endif
            return true;
        }

        public void LogScene(string sceneName) {
            _analytics.PushScreen(sceneName);
        }

        public void LogEvent(string name) {
            _logManager.Log($"devv {name}");
            _analytics.LogEvent(name);
        }

        public void LogEvent(string name, Parameter[] parameters) {
            _logManager.Log($"devv {name}");
            _analytics.LogEvent(new AnalyticsEvent(name, parameters));
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            _logManager.Log($"devv {name}");
            _analytics.LogEvent(name, parameters);
        }

        public void PushGameLevel(int levelNo, string levelMode) { }

        public void PopGameLevel(bool winGame) { }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit, Dictionary<string, object> extraParameters) {
            if (mediationNetwork == AdNetwork.AdMob) {
                // Logged automatically.
                return;
            }
            var adPlatform = mediationNetwork switch {
                AdNetwork.IronSource => "IronSource",
                _ => "Others",
            };
            var adFormat = format switch {
                AdFormat.Banner => "Banner",
                AdFormat.Rect => "Rect",
                AdFormat.AppOpen => "App Open",
                AdFormat.Interstitial => "Interstitial",
                AdFormat.RewardedInterstitial => "Rewarded Interstitial",
                AdFormat.Rewarded => "Rewarded",
                _ => "Others",
            };
            
            var isValid = !string.IsNullOrWhiteSpace(monetizationNetwork) &&
                          !string.IsNullOrWhiteSpace(adUnit) && revenue > 0;
            var evName = isValid ? "ad_impression" : "ad_impression_error";
            Parameter[] parameters = {
                new("ad_platform", adPlatform),
                new("ad_source", monetizationNetwork),
                new("ad_unit", adUnit),
                new("ad_format", adFormat),
                new("value", revenue),
                new("currency", currencyCode)
            };
            _analytics.LogEvent(new AnalyticsEvent(evName, parameters));
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) {
            Parameter[] parameters = {
                new("package_name", packageName),
            };
            _analytics.LogEvent(new AnalyticsEvent(eventName, parameters));
        }
    }
}