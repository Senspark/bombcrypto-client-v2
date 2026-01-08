using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using JetBrains.Annotations;
using Senspark.Iap.Validators;

using UnityEngine;
using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public class PurchaseController : IPurchaseController {
        private UniTaskCompletionSource<bool> _restorePurchaseTask;
        private UniTaskCompletionSource<PurchaseRawReceipt> _purchaseTask;

        private IStoreController _controller;
        private IExtensionProvider _extension;

        private readonly ICustomStoreListener _listener;
        private readonly ILocalValidator _localValidator;
        private readonly IServerValidator _serverValidator;
        private const string LogPrefix = "[Senspark]";

        public PurchaseController(
            ICustomStoreListener listener,
            ILocalValidator localValidator,
            IServerValidator serverValidator
        ) {
            _listener = listener;
            _localValidator = localValidator;
            _serverValidator = serverValidator;

            _listener.OnPurchaseHasResult += OnPurchaseHasResult;
            _listener.OnInitializeSuccess += OnInitializeSuccess;
        }

        public void Clear() {
            _controller = null;
            _extension = null;
        }

        public bool IsInitialized() {
            return _controller != null && _extension != null;
        }

        public async UniTask<PurchaseResult> Purchase(string productId, ProductType productType) {
            if (_controller == null) {
                return PurchaseResult.Fail;
            }

            if (_purchaseTask != null) {
                throw new Exception("Purchasing service is busy");
            }

            var product = _controller.products.WithID(productId);
            if (product == null || !product.availableToPurchase) {
                FastLog.Error($"{LogPrefix} Purchase product {productId} failed.");
                return PurchaseResult.Fail;
            }

            _purchaseTask = new UniTaskCompletionSource<PurchaseRawReceipt>();
            _controller.InitiatePurchase(product);
            var rawReceipt = await _purchaseTask.Task;
            if (rawReceipt == null) {
                _purchaseTask = null;
                return new PurchaseResult(false, null, null, PurchaseFrom.Production);
            }
            
            FastLog.Info($"[Senspark] Raw Receipt: {rawReceipt.receipt}");
            var purchaseToken = _localValidator.GetPurchaseToken(rawReceipt);

            var serverValidated =
                await _serverValidator.Validate(productId, purchaseToken, Application.identifier, productType);
            if (serverValidated.valid) {
                Consume(productId);
            }
            else {
                FastLog.Info($"{LogPrefix} {serverValidated.errorMessage}");
            }

            _purchaseTask = null;
            var purchaseFrom = serverValidated.isTest ? PurchaseFrom.Tester : PurchaseFrom.Production;
            return new PurchaseResult(serverValidated.valid, purchaseToken.OrderId, rawReceipt.receipt, purchaseFrom);
        }

#pragma warning disable CS1998
        public async UniTask<bool> Restore() {
#pragma warning restore CS1998
            if (_extension == null) {
                return false;
            }
#if UNITY_IOS
            if (_restorePurchaseTask != null) {
                return await _restorePurchaseTask.Task;
            }

            _restorePurchaseTask = new UniTaskCompletionSource<bool>();
            var apple = _extension.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result, msg) => { _restorePurchaseTask.TrySetResult(result); });
            var result = await _restorePurchaseTask.Task;
            _restorePurchaseTask = null;
            return result;
#else
            return false;
#endif
        }

        public bool IsPurchased(string productId) {
            var p = FindProduct(productId);
            if (p == null) {
                return false;
            }
            var t = p.definition.type;
            return t switch {
                ProductType.Consumable => false,
                ProductType.NonConsumable => HasBoughtNonConsumable(productId),
                ProductType.Subscription => HasBoughtSubscription(productId),
                _ => false
            };
        }

        public bool Consume(string productId) {
            if (_controller == null) {
                return false;
            }

            var product = _controller.products.WithID(productId);
            if (product == null || !product.availableToPurchase) {
                FastLog.Error($"{LogPrefix} Failed to Consume ProductId {productId}");
                return false;
            }

            _controller.ConfirmPendingPurchase(product);
            return true;
        }

        [CanBeNull]
        public Product FindProduct(string productId) {
            return _controller?.products.all.FirstOrDefault(e => e.definition.id == productId);
        }

        public Product[] FindProducts(string productId) {
            if (_controller == null) {
                return Array.Empty<Product>();
            }

            return _controller.products.all.Where(e => e.definition.id == productId).ToArray();
        }

        private void OnPurchaseHasResult(PurchaseRawReceipt rawReceipt) {
            _purchaseTask?.TrySetResult(rawReceipt);
        }

        private void OnInitializeSuccess(InitResult result) {
            _controller = result.Controller;
            _extension = result.Extension;
        }
        
        private bool HasBoughtNonConsumable(string id) {
            var p = FindProduct(id);
            if (p == null) {
                return false;
            }

            if (p.hasReceipt) {
                return true;
            }

            if (!p.availableToPurchase) {
                return true;
            }

            return false;
        }

        private bool HasBoughtSubscription(string id) {
            var products = FindProducts(id);
            foreach (var p in products) {
                if (!p.hasReceipt) {
                    continue;
                }

                var subMg = new SubscriptionManager(p, "");
                var subInfo = subMg.getSubscriptionInfo();
                if (subInfo.isSubscribed() == Result.True) {
                    return true;
                }
            }

            return false;
        }
    }
}