#if !UNITY_WEBGL
using System;
using System.Collections.Generic;
using System.Linq;

using Cysharp.Threading.Tasks;

using Firebase.Analytics;

using JetBrains.Annotations;

namespace Senspark.Internal {
    internal class FirebaseAnalyticsBridge : IAnalyticsBridge {
        [NotNull]
        private readonly Dictionary<Type, Func<string, object, Parameter>> _parsers;

        [NotNull]
        private readonly Dictionary<Type, Action<string, string, object>> _loggers;

        public FirebaseAnalyticsBridge() {
            _parsers = new Dictionary<Type, Func<string, object, Parameter>> {
                [typeof(bool)] = (key, value) => new Parameter(key, (bool) value ? 1 : 0),
                [typeof(int)] = (key, value) => new Parameter(key, (int) value),
                [typeof(long)] = (key, value) => new Parameter(key, (long) value),
                [typeof(float)] = (key, value) => new Parameter(key, (float) value),
                [typeof(double)] = (key, value) => new Parameter(key, (double) value),
                [typeof(string)] = (key, value) => new Parameter(key, (string) value),
            };
            _loggers = new Dictionary<Type, Action<string, string, object>> {
                [typeof(bool)] = (name, key, value) => FirebaseAnalytics.LogEvent(name, key, (bool) value ? 1 : 0),
                [typeof(int)] = (name, key, value) => FirebaseAnalytics.LogEvent(name, key, (int) value),
                [typeof(long)] = (name, key, value) => FirebaseAnalytics.LogEvent(name, key, (long) value),
                [typeof(float)] = (name, key, value) => FirebaseAnalytics.LogEvent(name, key, (float) value),
                [typeof(double)] = (name, key, value) => FirebaseAnalytics.LogEvent(name, key, (double) value),
                [typeof(string)] = (name, key, value) => FirebaseAnalytics.LogEvent(name, key, (string) value),
            };
        }

        public async UniTask<bool> Initialize() {
            var status = await FirebaseInitializer.Initialize();
            var permission = await Platform.RequestTrackingAuthorization();
            if (permission == TrackingAuthorizationStatus.Authorized) {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            }
            return status;
        }

        public void LogScreen(string name) {
            if (name == null) {
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView);
            } else {
                FirebaseAnalytics.LogEvent(
                    FirebaseAnalytics.EventScreenView,
                    FirebaseAnalytics.ParameterScreenName,
                    name
                );
            }
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            if (parameters == null || parameters.Count == 0) {
                FirebaseAnalytics.LogEvent(name);
            } else if (parameters.Count == 1) {
                var key = parameters.Keys.ToList()[0];
                var value = parameters[key];
                var type = value.GetType();
                var logger = _loggers[type];
                logger(name, key, value);
            } else {
                FirebaseAnalytics.LogEvent(name, parameters
                    .Select(entry => ParseParameter(entry.Key, entry.Value))
                    .ToArray()
                );
            }
        }

        public void PushGameLevel(int levelNo, string levelMode) { }

        public void PopGameLevel(bool winGame) { }

        public void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            string currencyCode,
            AdFormat format,
            string adUnit,
            Dictionary<string, object> extraParameters
        ) {
            // https://firebase.google.com/docs/analytics/measure-ad-revenue#unity
            if (mediationNetwork == AdNetwork.AdMob) {
                // Logged automatically.
                return;
            }
            var adPlatform = mediationNetwork switch {
                AdNetwork.AppLovin => "AppLovin",
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
            
            FirebaseAnalytics.LogEvent(evName,
                new Parameter("ad_platform", adPlatform),
                new Parameter("ad_source", monetizationNetwork),
                new Parameter("ad_unit", adUnit),
                new Parameter("ad_format", adFormat),
                new Parameter("value", revenue),
                new Parameter("currency", currencyCode)
            );
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
            FirebaseAnalytics.LogEvent(eventName,
                new Parameter("package_name", packageName)
            );
        }

        public void LogEarnResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
        }

        public void LogSpendResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
        }

        private Parameter ParseParameter([NotNull] string key, [NotNull] object value) {
            var type = value.GetType();
            var parser = _parsers[type];
            return parser(key, value);
        }
    }
}
#endif