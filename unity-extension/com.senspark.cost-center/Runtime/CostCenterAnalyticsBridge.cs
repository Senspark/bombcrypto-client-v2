#if !UNITY_WEBGL
using System;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using Firebase.Analytics;

using Manager;

namespace Senspark.Internal {
    /// <summary>
    /// Class này sẽ sử dụng Firebase & AppsFlyer để tổng hợp data & gửi
    /// https://support.costcenter.net/en/articles/7325139-tracking-level-analytics
    /// </summary>
    internal class CostCenterAnalyticsBridge : IAnalyticsBridge {
        private GameLevelData _levelData;
        private const string LogTag = "[Senspark][CostCenter]";
        private PendingIapData _pendingIapData;

        public CostCenterAnalyticsBridge(IRevenueValidator revenueValidator) {
            var handle = new ObserverHandle();
            handle.AddObserver(revenueValidator, new RevenueValidatorObserver {
                OnIapRevenueValidated = OnReceivedIapValidatedInfo
            });
            // FIXME: dont have Dispose to remove observer
        }

        public async UniTask<bool> Initialize() {
            await FirebaseInitializer.Initialize();
            return true;
        }

        public void LogScreen(string name) {
        }

        public void LogEvent(string name, Dictionary<string, object> parameters) {
        }

        public void PushGameLevel(int levelNo, string levelMode) {
            _levelData = new GameLevelData(levelNo, levelMode);
            var parameters = new List<Parameter>();
            AddLevelData(parameters);
            FastLog.Info($"{LogTag} PushGameLevel: {levelNo} - {levelMode}");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelStart, parameters.ToArray());
        }

        public void PopGameLevel(bool winGame) {
            var parameters = new List<Parameter> {
                new(FirebaseAnalytics.ParameterSuccess, winGame ? "True" : "False")
            };
            AddLevelData(parameters);
            FastLog.Info($"{LogTag} PopGameLevel: {winGame}");
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd, parameters.ToArray());
            _levelData = null;
        }

        public void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            string currencyCode,
            AdFormat format,
            string adUnit,
            Dictionary<string, object> extraParameters
        ) {
            // Log ad revenue dùng Firebase (khác với ad_impression đã log bên Firebase)
            var adFormat = format switch {
                AdFormat.Banner => "banner",
                AdFormat.Rect => "banner",
                AdFormat.AppOpen => "app_open",
                AdFormat.Interstitial => "inter",
                AdFormat.RewardedInterstitial => "rewarded",
                AdFormat.Rewarded => "rewarded",
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
            var parameters = new List<Parameter> {
                new(FirebaseAnalytics.ParameterValue, revenue),
                new(FirebaseAnalytics.ParameterCurrency, currencyCode),
                new("ad_format", adFormat),
                new("ad_unit", adUnit)
            };
            AddLevelData(parameters);
            FastLog.Info($"{LogTag} LogAdRevenue: {adFormat} {revenue} {currencyCode}");
            FirebaseAnalytics.LogEvent("ad_revenue_sdk", parameters.ToArray());
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue,
            string currencyIso, string receipt) {
            // Chờ kết quả validate từ AppsFlyer
            if (_pendingIapData != null) {
                FastLog.Warn($"{LogTag} Why still pending? {packageName} {orderId} {priceValue} {currencyIso}");
            }
            FastLog.Info($"{LogTag} Add Pending: {packageName} {orderId} {priceValue} {currencyIso}");
            _pendingIapData = new PendingIapData(packageName, orderId, priceValue, currencyIso);
        }

        private void OnReceivedIapValidatedInfo(PurchaseValidatedData data) {
            if (_pendingIapData == null) {
                FastLog.Error($"{LogTag} Why dont have pending data?");
                return;
            }
            FastLog.Info(
                $"{LogTag} Received ValidatedInfo: {data.ProductId} {data.OrderId} {data.IsSuccess} {data.IsTestPurchase}");
            if (data.OrderId != _pendingIapData.OrderId) {
                FastLog.Error($"{LogTag} OrderId not match: {data.OrderId} {_pendingIapData.OrderId}");
                return;
            }
            var evName = data.IsTestPurchase ? "iap_sdk_test" : "iap_sdk";
            var price = data.IsTestPurchase ? 0 : _pendingIapData.PriceValue;
            var parameters = new List<Parameter> {
                new(FirebaseAnalytics.ParameterValue, price),
                new(FirebaseAnalytics.ParameterCurrency, _pendingIapData.CurrencyIso),
                new("product_id", _pendingIapData.PackageName),
                new("order_id", data.OrderId),
            };
            AddLevelData(parameters);
            FastLog.Info($"{LogTag} LogIapRevenue: {evName} {price} {_pendingIapData.CurrencyIso}");
            FirebaseAnalytics.LogEvent(evName, parameters.ToArray());
            _pendingIapData = null;
        }

        private void AddLevelData(List<Parameter> parameters) {
            if (_levelData == null) {
                return;
            }
            parameters.Add(new Parameter(FirebaseAnalytics.ParameterLevel, _levelData.LevelNo));
            parameters.Add(new Parameter("level_mode", _levelData.LevelMode));
        }

        public void LogEarnResource(string playMode, int level , string resourceName,int amount, int balance, string item, string itemType, string booster = "") {
            var parameters = new List<Parameter> {
                new("play_mode", playMode),
                new("level", level),
                new("name", resourceName),
                new("amount", amount),
                new("balance", balance),
                new("item", item),
                new("item_type", itemType),
                new("booster", string.IsNullOrEmpty(booster) ? null : booster)
            };

            FirebaseAnalytics.LogEvent("resource_source", parameters.ToArray());
        }

        public void LogSpendResource(string playMode, int level , string resourceName,int amount, int balance, string item, string itemType, string booster = "") {
            var parameters = new List<Parameter> {
                new("play_mode", playMode),
                new("level", level),
                new("name", resourceName),
                new("amount", amount),
                new("balance", balance),
                new("item", item),
                new("item_type", itemType),
                new("booster", string.IsNullOrEmpty(booster) ? null : booster)
            };

            FirebaseAnalytics.LogEvent("resource_sink", parameters.ToArray());
        }

        private class PendingIapData {
            public readonly string PackageName;
            public readonly string OrderId;
            public readonly double PriceValue;
            public readonly string CurrencyIso;

            public PendingIapData(string packageName, string orderId, double priceValue, string currencyIso) {
                PackageName = packageName;
                OrderId = orderId;
                PriceValue = priceValue;
                CurrencyIso = currencyIso;
            }
        }
    }
}
#endif