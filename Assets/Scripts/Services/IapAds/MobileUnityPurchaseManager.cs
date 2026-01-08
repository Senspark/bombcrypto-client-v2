using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Analytics;

using Senspark;

using Newtonsoft.Json;

using Senspark.Iap;

using Unity.Services.Core;
using Unity.Services.Core.Environments;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace Services.IapAds {
    /// <summary>
    /// https://docs.unity3d.com/Packages/com.unity.purchasing@4.5/manual/UnityIAPInitialization.html
    /// </summary>
    public class MobileUnityPurchaseManager : IUnityPurchaseManager, IStoreListener {
        private readonly ILogManager _logManager;
        private readonly IAnalytics _analytics;
        
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private TaskCompletionSource<PurchaseResult> _purchaseTask;
        private TaskCompletionSource<bool> _synced;
        private readonly List<PurchaseResult> _pendingTransactions = new();

        public MobileUnityPurchaseManager(ILogManager logManager, IAnalytics analytics) {
            _logManager = logManager;
            _analytics = analytics;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task SyncData() {
            if (_synced == null) {
                _synced = new TaskCompletionSource<bool>();
                await SyncDataImpl();
            }
            await _synced.Task;
        }

        public bool HasSubscription(string productId) {
            ThrowIfNotSyncData();
            var product = _controller.products.WithID(productId);
            return product.hasReceipt;
        }

        public List<PurchaseResult> GetPendingTransactions() {
            return new List<PurchaseResult>(_pendingTransactions);
        }

        public async Task<PurchaseResult> PurchaseItem(string productId) {
            ThrowIfNotSyncData();
            if (_purchaseTask == null) {
                _purchaseTask = new TaskCompletionSource<PurchaseResult>();
                _controller.InitiatePurchase(productId);
            }
            var result = await _purchaseTask.Task;
            _purchaseTask = null;

            if (result.State == PurchaseState.Done) {
                TrackBuyInApp(
                    productId,
                    GetItemPrice(productId),
                    result.OrderId,
                    result. Receipt);
                
                ConsumePurchaseItem(productId);
            }
            return result;
        }

        private void TrackBuyInApp(string productId, IapPrice price, string orderId, string receipt) {
            var pId = productId
                .Replace(".", "_")
                .Replace("-", "_");
            var pIdShorten = productId
                .Replace($"{Application.identifier}.", string.Empty)
                .Replace("com.senspark.", string.Empty)
                .Replace(".", "_")
                .Replace("-", "_");
            var evName = $"conv_buy_iap_{pIdShorten}";
            _analytics.LogIapRevenue(evName, pId, orderId, price.Price, price.Currency, receipt);
        }
        
        public void ConsumePurchaseItem(string productId) {
            // if (!_pendingTransactions.Exists(e => e.ProductId == productId)) {
            //     throw new Exception("Cannot restore");
            // }
            var product = _controller.products.WithID(productId);
            _controller.ConfirmPendingPurchase(product);
            _pendingTransactions.RemoveAll(e => e.ProductId == productId);
            _logManager.Log($"devv confirmed: {productId} {product}");
        }

        public IapPrice GetItemPrice(string productId) {
            const string errStr = "ERROR";
            var error = new IapPrice(0, errStr, errStr);
            if (_controller == null || _synced == null || !_synced.Task.IsCompleted) {
                return error;
            }
            var product = _controller.products.WithID(productId);
            return product != null
                ? new IapPrice((float)product.metadata.localizedPrice, product.metadata.isoCurrencyCode,
                    product.metadata.localizedPriceString)
                : error;
        }

        private void ThrowIfNotSyncData() {
            if (_synced == null || !_synced.Task.IsCompleted) {
                throw new Exception("Data not synced");
            }
        }

        private async Task SyncDataImpl() {
            try {
                var env = Application.isEditor ? "development" : "production";
                var options = new InitializationOptions().SetEnvironmentName(env);
                await UnityServices.InitializeAsync(options);

                var instance = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
                instance.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
                var builder = ConfigurationBuilder.Instance(instance);
                foreach (var p in IapConfig.ProductsPricesUsd.Keys) {
                    builder.AddProduct(p, ProductType.Consumable);
                }
                foreach (var p in IapConfig.SubscriptionsPricesUsd.Keys) {
                    builder.AddProduct(p, ProductType.Subscription);
                }
                UnityPurchasing.Initialize(this, builder);
            } catch (Exception e) {
                // ignore
                Debug.LogWarning(e);
                _synced.SetResult(false);
            }
        }

        #region UNITY IAP EVENTS

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            _controller = controller;
            _extensions = extensions;
            _synced.SetResult(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error) {
            OnInitializeFailed(error, string.Empty);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) {
            _logManager.Log($"devv init failed: reason: {error} {message}");
            _synced.SetResult(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            try {
                var p = purchaseEvent.purchasedProduct;
                var result = ParseAndSaveReceiptData(p.definition.id, p.receipt);
                if (_purchaseTask != null && !_purchaseTask.Task.IsCompleted) {
                    _purchaseTask.SetResult(result);
                }
            } catch (Exception e) {
                Debug.LogException(e);
            }
            return PurchaseProcessingResult.Pending;
#else
            _purchaseTask?.SetResult(new PurchaseResult(PurchaseState.Error, null, null, null, null, null));
            return PurchaseProcessingResult.Complete;
#endif
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
            var receipt = product.receipt;
            var state = failureReason == PurchaseFailureReason.UserCancelled
                ? PurchaseState.Cancel
                : PurchaseState.Error;
            var result = new PurchaseResult(state, null, null, null, null, null);
            _logManager.Log($"devv purchase failed reason: {failureReason.ToString()}");
            _logManager.Log($"devv purchase failed: {receipt}");
            try {
                result = ParseAndSaveReceiptData(product.definition.id, receipt);
            } catch (Exception e) {
                _logManager.Log(e.Message);
            }
            if (_purchaseTask != null && !_purchaseTask.Task.IsCompleted) {
                _purchaseTask.SetResult(result);
            }
        }

        private PurchaseResult ParseAndSaveReceiptData(string productId, string receipt) {
            var result = GetPurchaseTokenFromReceipt(receipt);
            
            if (_pendingTransactions.Any(p => p.PurchaseToken == result.PurchaseToken)) {
                return result;
            }
            
            _logManager.Log($"devv {result.PurchaseToken}");
            _pendingTransactions.RemoveAll(e => e.ProductId == result.ProductId);
            _pendingTransactions.Add(result);
            _logManager.Log($"devv add pending: {productId}");
            _logManager.Log($"devv add pending: {receipt}");
            return result;
        }

        private PurchaseResult GetPurchaseTokenFromReceipt(string receipt) {
            _logManager.Log("devv receipt:");
            _logManager.Log(receipt);
            var validator =
                new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            var results = validator.Validate(receipt);
            if (results.Length == 0) {
                throw new Exception("Receipt Invalid");
            }
            var result = results[0];
            string purchaseToken = null;
            var orderId = string.Empty;
#if UNITY_ANDROID
            var parsed = (GooglePlayReceipt)result;
            purchaseToken = parsed.purchaseToken;
            orderId = parsed.orderID;
#elif UNITY_IOS
            var rawReceipt = JsonConvert.DeserializeObject<AppleReceipt>(receipt);
            purchaseToken = rawReceipt.Payload;
#else
            throw new Exception("Not supported");
            return null;
#endif
            if (string.IsNullOrWhiteSpace(purchaseToken)) {
                throw new Exception("Receipt Invalid");
            }
            return new PurchaseResult(PurchaseState.Done, result.transactionID, purchaseToken, orderId, result.productID, receipt);
        }
        
        private class AppleReceipt {
            public string Payload;
            public string Store;
            public string TransactionID;
        }

        #endregion
        
    }
}