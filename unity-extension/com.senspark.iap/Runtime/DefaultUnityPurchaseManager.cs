using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Cysharp.Threading.Tasks;

using JetBrains.Annotations;

using Senspark.Iap.CheatDetection;
using Senspark.Iap.Validators;

using Unity.Services.Core;
using Unity.Services.Core.Environments;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public class DefaultUnityPurchaseManager : IUnityPurchaseManager {
        private readonly ICustomStoreListener _storeListener;
        private readonly IPurchaseController _purchaseController;
        private readonly IAnalyticsManager _analytics;
        private readonly Dictionary<string, IapData> _iapData;

        private const string EmptyPriceStr = "--";
        private const string TrackingStrProdFormat = "conv_buy_iap_{0}";
        private const string TrackingIapProdEvName = "conv_buy_iap";
        private const string TrackingStrTestFormat = "test_buy_iap_{0}";
        private const string TrackingIapTestEvName = "test_buy_iap";

        public DefaultUnityPurchaseManager(
            [NotNull] IAnalyticsManager analytics,
            [NotNull] IapData[] iapData,
            bool serverVerify,
            [CanBeNull] byte[] googleBillingLicenseCode,
            [CanBeNull] ICheatPunishment punishment,
            float editorSubscriptionDurationSeconds
        ) {
            _analytics = analytics;
            _iapData = new Dictionary<string, IapData>();
            foreach (var d in iapData) {
                _iapData[d.ProductId] = d;
            }
            punishment ??= new ForceQuitPunishment(analytics);
            var localValidator = new DefaultLocalReceiptValidator(googleBillingLicenseCode, punishment);
            var serverValidator = new DefaultServerValidator(serverVerify);
            _storeListener = new CustomStoreListener();
            _purchaseController = new DefaultPurchaseController(_storeListener, localValidator, serverValidator,
                editorSubscriptionDurationSeconds);
        }

        public async UniTask Initialize(float timeOut) {
            await UniTask.WhenAny(
                InitUnityService(_iapData.Values.ToArray()),
                UniTask.Delay(TimeSpan.FromSeconds(timeOut)
                ));
        }
        
        public void Dispose() {
        }

        public string GetPrice(string productId) {
            if (!IsValidProductId(productId)) {
                return EmptyPriceStr;
            }

            var p = _purchaseController.FindProduct(productId);
            return p == null ? EmptyPriceStr : p.metadata.localizedPriceString;
        }

        public PriceValue GetPriceValue(string productId) {
            if (!IsValidProductId(productId)) {
                return PriceValue.Empty;
            }

            var p = _purchaseController.FindProduct(productId);
            if (p == null) {
                return PriceValue.Empty;
            }
            var price = (float)p.metadata.localizedPrice;
            var currency = p.metadata.isoCurrencyCode;
            return new PriceValue(price, currency);
        }

        public SubscriptionInfo GetSubscriptionInfo(string productId) {
            if (!IsValidProductId(productId)) {
                return null;
            }

            var p = _purchaseController.FindProduct(productId);
            if (p == null) {
                return null;
            }

            if (p.definition.type != ProductType.Subscription) {
                return null;
            }

            try {
                var subMg = new SubscriptionManager(p, "");
                var subInfo = subMg.getSubscriptionInfo();
                return subInfo;
            } catch (Exception e) {
                Debug.LogError(e);
                return null;
            }
        }

        public bool IsPurchased(string productId) {
            if (!IsInitialized() || !IsValidProductId(productId)) {
                return false;
            }
            return _purchaseController.IsPurchased(productId);
        }

        public async UniTask<bool> Purchase(string productId) {
            if (!await TryInitialize()) {
                return false;
            }

            if (!IsValidProductId(productId)) {
                return false;
            }

            var p = _purchaseController.FindProduct(productId);
            Assert.IsNotNull(productId);
            Assert.IsNotNull(p);

            var result = await _purchaseController.Purchase(productId, p.definition.type);
            if (result.IsSuccess) {
                var d = _iapData[productId];
                TrackBuyInApp(
                    new IapData(productId, d.Type, GetPriceValue(productId)),
                    result.OrderId,
                    result.Receipt,
                    result.PurchaseFrom == PurchaseFrom.Production);
            }

            return result.IsSuccess;
        }

        public async UniTask<bool> Restore() {
            if (!await TryInitialize()) {
                return false;
            }

            return await _purchaseController.Restore();
        }
        
        public async UniTask<bool> RemoveAllNonConsumable() {
            var list = _iapData.Where(e => e.Value.Type == ProductType.NonConsumable).Select(e => e.Key).ToList();
            var result = false;
            foreach (var pId in list) {
                result |= await RemoveNonConsumable(pId);
            }
            return result;
        }

        public async UniTask<bool> RemoveNonConsumable(string productId) {
            if (!IsInitialized() || !IsValidProductId(productId)) {
                return false;
            }

            Assert.IsNotNull(productId);
            var pId = _iapData[productId];
            Assert.IsNotNull(pId);
            if (pId.Type != ProductType.NonConsumable) {
                return false;
            }

            // https://forum.unity.com/threads/how-to-consume-non-consumable-iap-on-android.569335/#post-3787099
            // Đầu tiên là khởi tạo lại IapData với convert ProductType UnConsumable -> Consumable
            // Consume xong thì khởi tạo lại IapData như ban đầu

            try {
                var ogIapData = _iapData.Values.ToArray();
                var length = ogIapData.Length;

                var tmpIapData = new IapData[length];
                for (var i = 0; i < length; i++) {
                    var data = ogIapData[i];
                    if (data.Type == ProductType.NonConsumable) {
                        tmpIapData[i] = new IapData(data.ProductId, ProductType.Consumable, 0);
                    } else {
                        tmpIapData[i] = data;
                    }
                }

                _purchaseController.Clear();
                await _storeListener.SetIapData(tmpIapData);

                _purchaseController.Consume(productId);

                _purchaseController.Clear();
                await _storeListener.SetIapData(ogIapData);

                return true;
            } catch (Exception e) {
                FastLog.Error(e.Message);
                return false;
            }
        }

        private async Task<bool> TryInitialize() {
            if (!IsInitialized()) {
                var initResult = await InitUnityService(_iapData.Values.ToArray());
                if (!initResult) {
                    return false;
                }
            }

            return true;
        }

        private void TrackBuyInApp(IapData data, string orderId, string receipt, bool isProd) {
            var pId = data.ProductId
                .Replace(".", "_")
                .Replace("-", "_");
            var pIdShorten = data.ProductId
                .Replace($"{Application.identifier}.", string.Empty)
                .Replace("com.senspark.", string.Empty)
                .Replace(".", "_")
                .Replace("-", "_");
            var evName = string.Format(isProd ? TrackingStrProdFormat : TrackingStrTestFormat, pIdShorten);
            _analytics.LogIapRevenue(evName, pId, orderId, data.Price.Price, data.Price.Currency, receipt);
            _analytics.LogIapRevenue(isProd ? TrackingIapProdEvName : TrackingIapTestEvName, pId, orderId,
                data.Price.Price, data.Price.Currency, receipt);
        }

        private bool IsValidProductId(string productId) {
            return !string.IsNullOrWhiteSpace(productId) && _iapData.ContainsKey(productId);
        }

        private bool IsInitialized() {
            return _purchaseController.IsInitialized();
        }

        private async UniTask<bool> InitUnityService(IapData[] iapData) {
            if (IsInitialized()) {
                return true;
            }
            try {
                var enviroment = "production";
#if UNITY_EDITOR
                enviroment = "development";
#endif
                await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(enviroment));
                var result = await _storeListener.SetIapData(iapData);
                if (!result) {
                    return false;
                }
            } catch (Exception e) {
                Debug.LogError(e.Message);
                if (e.InnerException != null) {
                    Debug.LogError(e.InnerException.Message);
                }
                return false;
            }
            return true;
        }
    }
}