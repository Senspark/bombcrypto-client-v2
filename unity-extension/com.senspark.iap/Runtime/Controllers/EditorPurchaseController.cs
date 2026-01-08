using System;
using System.Linq;

using Cysharp.Threading.Tasks;

using Senspark.Iap.Receipts;

using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public class EditorPurchaseController : IPurchaseController {
        private readonly IDataManager _dataManager;
        private readonly EditorReceiptManager _receiptManager;
        private readonly ICustomStoreListener _listener;

        private IStoreController _controller;
        private UniTaskCompletionSource<PurchaseRawReceipt> _purchaseTask;

        public EditorPurchaseController(ICustomStoreListener listener, float subscriptionDurationSeconds) {
            _listener = listener;
            _receiptManager = new EditorReceiptManager(new JsonDataManager(), subscriptionDurationSeconds);

            _listener.OnPurchaseHasResult += OnPurchaseHasResult;
            _listener.OnInitializeSuccess += OnInitializeSuccess;
        }

        public bool IsInitialized() {
            return _controller != null;
        }

        public void Clear() {
            _controller = null;
        }

        public async UniTask<PurchaseResult> Purchase(string productId, ProductType productType) {
            if (_controller == null) {
                return PurchaseResult.Fail;
            }

            if (_purchaseTask != null) {
                throw new Exception("Purchasing service is busy");
            }

            if (!CanPurchase(productId)) {
                return PurchaseResult.Fail;
            }

            _purchaseTask = new UniTaskCompletionSource<PurchaseRawReceipt>();
            _controller.InitiatePurchase(productId);
            var rawReceipt = await _purchaseTask.Task;
            var purchaseSuccess = rawReceipt != null && !string.IsNullOrWhiteSpace(rawReceipt.receipt);

            if (purchaseSuccess) {
                _receiptManager.AddReceipt(productId, productType);
            }
            _purchaseTask = null;
            return purchaseSuccess
                ? new PurchaseResult(true, string.Empty, string.Empty, PurchaseFrom.Tester)
                : PurchaseResult.Fail;
        }

        public UniTask<bool> Restore() {
            return UniTask.FromResult(true);
        }

        public bool IsPurchased(string productId) {
            var hasReceipt = _receiptManager.HasReceipt(productId);
            return hasReceipt;
        }

        public bool Consume(string productId) {
            if (_receiptManager.HasReceipt(productId)) {
                _receiptManager.RemoveReceipt(productId);
            }
            return true;
        }

        public Product FindProduct(string productId) {
            return _controller?.products.all.FirstOrDefault(e => e.definition.id == productId);
        }

        public Product[] FindProducts(string productId) {
            if (_controller == null) {
                return Array.Empty<Product>();
            }

            return _controller.products.all.Where(e => e.definition.id == productId).ToArray();
        }

        private bool CanPurchase(string productId) {
            var hasReceipt = _receiptManager.HasReceipt(productId);
            return !hasReceipt;
        }

        private void OnPurchaseHasResult(PurchaseRawReceipt rawReceipt) {
            _purchaseTask?.TrySetResult(rawReceipt);
        }

        private void OnInitializeSuccess(InitResult result) {
            _controller = result.Controller;
        }
    }
}