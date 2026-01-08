using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Senspark.Iap {
    public class CustomStoreListener : IDetailedStoreListener, ICustomStoreListener {
        public event Action<PurchaseRawReceipt> OnPurchaseHasResult;
        public event Action<InitResult> OnInitializeSuccess;

        private UniTaskCompletionSource<bool> _initTask;

        public async UniTask<bool> SetIapData(
            IapData[] iapData
        ) {
            try {
                if (_initTask != null) {
                    return await _initTask.Task;
                }

                _initTask = new UniTaskCompletionSource<bool>();

                // unity iap
                var module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
                module.useFakeStoreUIMode = FakeStoreUIMode.StandardUser;
#endif
                var builder = ConfigurationBuilder.Instance(module);
                
                await GoogleExtension.ConfigGooglePlayService(builder);
                
                foreach (var c in iapData) {
                    builder.AddProduct(c.ProductId, c.Type);
                }

                UnityPurchasing.Initialize(this, builder);
                var result = await _initTask.Task;
                _initTask = null;
                return result;
            }
            catch (Exception e) {
                FastLog.Error(e.Message);
                _initTask?.TrySetResult(false);
                _initTask = null;
                return false;
            }
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions) {
            OnInitializeSuccess?.Invoke(new InitResult(controller, extensions));
            _initTask.TrySetResult(true);
        }

        public void OnInitializeFailed(InitializationFailureReason error) {
            FastLog.Error("IAP InitializationFailureReason:" + error);
            _initTask.TrySetResult(false);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message) {
            FastLog.Error("IAP InitializationFailureReason:" + error);
            _initTask.TrySetResult(false);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent) {
            var product = purchaseEvent.purchasedProduct;
            var productId = product.definition.storeSpecificId;
            var receiptStr = product.receipt;
            
            OnPurchaseHasResult?.Invoke(new PurchaseRawReceipt(productId, receiptStr));
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) {
            FastLog.Error(
                $"IAP Purchase: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureReason}");
            OnPurchaseHasResult?.Invoke(null);
        }
        
        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription) {
            FastLog.Error(
                $"IAP Purchase: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureDescription.reason} {failureDescription.message}");
            OnPurchaseHasResult?.Invoke(null);
        }
    }
}