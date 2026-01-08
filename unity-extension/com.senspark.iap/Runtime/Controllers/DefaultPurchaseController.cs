using Cysharp.Threading.Tasks;

using Senspark.Iap.Validators;

using UnityEngine.Purchasing;

namespace Senspark.Iap {
    public class DefaultPurchaseController : IPurchaseController {
        private readonly IPurchaseController _bridge;

        public DefaultPurchaseController(
            ICustomStoreListener listener,
            ILocalValidator localValidator,
            IServerValidator serverValidator,
            float subscriptionDurationSeconds
        ) {
#if UNITY_EDITOR
            _bridge = new EditorPurchaseController(listener, subscriptionDurationSeconds);
#else
            _bridge = new PurchaseController(listener, localValidator, serverValidator);
#endif
        }

        public bool IsInitialized() => _bridge.IsInitialized();

        public void Clear() => _bridge.Clear();

        public UniTask<PurchaseResult> Purchase(string productId, ProductType productType) =>
            _bridge.Purchase(productId, productType);

        public UniTask<bool> Restore() => _bridge.Restore();
        
        public bool IsPurchased(string productId) => _bridge.IsPurchased(productId);

        public bool Consume(string productId) => _bridge.Consume(productId);

        public Product FindProduct(string productId) => _bridge.FindProduct(productId);

        public Product[] FindProducts(string productId) => _bridge.FindProducts(productId);
    }
}