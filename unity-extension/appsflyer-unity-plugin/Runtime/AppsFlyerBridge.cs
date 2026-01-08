using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AppsFlyerSDK;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

namespace Senspark.Internal {
    internal class AppsFlyerBridge : IAnalyticsBridge {
        /// <summary>
        /// Code ISO 3166-1 format.
        /// </summary>
        private const string PARAM_COUNTRY = "country";

        /// <summary>
        /// ID of the ad unit for the impression.
        /// </summary>
        private const string PARAM_AD_UNIT = "ad_unit";

        /// <summary>
        /// Format of the ad.
        /// </summary>
        private const string PARAM_AD_TYPE = "ad_type";

        /// <summary>
        /// ID of the ad placement for the impression.
        /// </summary>
        private const string PARAM_PLACEMENT = "placement";
        
        /// <summary>
        /// Use for in app purchase revenue
        /// </summary>
        private const string PARAM_AF_CURRENCY = "af_currency";
        private const string PARAM_AF_REVENUE = "af_revenue";
        private const string PARAM_AF_QUANTITY = "af_quantity";
        private const string PARAM_AF_CONTENT_TYPE = "af_content_type";
        private const string PARAM_AF_CONTENT_ID = "af_content_id";

        /// <summary>
        /// Provided by Facebook Audience Network only, and will be reported to publishers
        /// approved by Facebook Audience Network within the closed beta.
        /// </summary>
        private const string PARAM_ECPM_PAYLOAD = "ecpm_payload";

        [NotNull]
        private readonly string _devKey;

        [CanBeNull]
        private readonly string _appId;

        private readonly bool _isDebug;

        [NotNull]
        private readonly Dictionary<Type, Func<object, string>> _valueParsers;

        public AppsFlyerBridge(
            [NotNull] string devKey,
            [CanBeNull] string appId,
            bool isDebug
        ) {
            _devKey = devKey;
            _appId = appId;
            _isDebug = isDebug;
            _valueParsers = new Dictionary<Type, Func<object, string>> {
                [typeof(bool)] = value => (bool) value ? "1" : "0",
                [typeof(int)] = value => ((int) value).ToString(),
                [typeof(long)] = value => ((long) value).ToString(),
                [typeof(float)] = value => ((float) value).ToString(CultureInfo.InvariantCulture),
                [typeof(double)] = value => ((double) value).ToString(CultureInfo.InvariantCulture),
                [typeof(string)] = value => (string) value,
            };
        }

        public async UniTask<bool> Initialize() {
            AppsFlyer.initSDK(_devKey, _appId);
            AppsFlyer.setIsDebug(_isDebug);
            
            AppsFlyerAdRevenue.start();
            AppsFlyerAdRevenue.setIsDebug(_isDebug);
            
            var permission = await Platform.RequestTrackingAuthorization();
            AppsFlyer.startSDK();
            return true;
        }

        public void LogScreen(string name) { }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
            var entries = parameters?.ToDictionary(
                it => it.Key,
                it => _valueParsers[it.Value.GetType()](it.Value)
            ) ?? new Dictionary<string, string>();
            AppsFlyer.sendEvent(name, entries);
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
            var mediationNetworkValue = mediationNetwork switch {
                AdNetwork.AdMob => AppsFlyerAdRevenueMediationNetworkType
                    .AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
                AdNetwork.AppLovin => AppsFlyerAdRevenueMediationNetworkType
                    .AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                AdNetwork.IronSource => AppsFlyerAdRevenueMediationNetworkType
                    .AppsFlyerAdRevenueMediationNetworkTypeIronSource,
                _ => AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeCustomMediation,
            };
            var adType = format switch {
                AdFormat.Banner => "Banner",
                AdFormat.Rect => "Rect",
                AdFormat.AppOpen => "App Open",
                AdFormat.Interstitial => "Interstitial",
                AdFormat.RewardedInterstitial => "Rewarded Interstitial",
                AdFormat.Rewarded => "Rewarded",
                _ => "Others",
            };
            var parameters = new Dictionary<string, string> {
                [PARAM_AD_TYPE] = adType, // 
                [PARAM_AD_UNIT] = adUnit,
            };
            if (extraParameters != null) {
                foreach (var (key, value) in extraParameters) {
                    parameters.TryAdd(key, _valueParsers[value.GetType()](value));
                }
            }
            var isValid = !string.IsNullOrWhiteSpace(monetizationNetwork) &&
                          !string.IsNullOrWhiteSpace(adUnit) && revenue > 0;
            if (!isValid) {
                return;
            }

            AppsFlyerAdRevenue.logAdRevenue(
                monetizationNetwork: monetizationNetwork,
                mediationNetwork: mediationNetworkValue,
                eventRevenue: revenue,
                revenueCurrency: currencyCode,
                additionalParameters: parameters
            );
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
            // Đã sử dụng thư viện track iap của AppsFlyer nên ko gửi price nữa (để tránh double revenue)
            priceValue = 0;
            
            var parameters = new Dictionary<string, string> {
                [PARAM_AF_CURRENCY] = currencyIso,
                [PARAM_AF_REVENUE] = priceValue.ToString(CultureInfo.InvariantCulture),
                [PARAM_AF_QUANTITY] = "1",
                [PARAM_AF_CONTENT_TYPE] = "iap_product",
                [PARAM_AF_CONTENT_ID] = packageName,
            };
            AppsFlyer.sendEvent(eventName, parameters);
        }

        public void LogEarnResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
        }

        public void LogSpendResource(string playMode, int level, string resourceName, int amount, int balance, string item,
            string itemType, string booster = "") {
        }
    }
}