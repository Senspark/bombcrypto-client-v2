using Cysharp.Threading.Tasks;

using UnityEngine.Purchasing;

namespace Senspark.Iap {
    internal class NullPurchaseManager : IUnityPurchaseManager {
        public void Dispose() {
        }

        public UniTask Initialize(float timeOut) {
            return UniTask.CompletedTask;
        }

        public string GetPrice(string productId) {
            return "--";
        }

        public PriceValue GetPriceValue(string productId) {
            return new PriceValue(0, "--");
        }

        public SubscriptionInfo GetSubscriptionInfo(string productId) {
            return new SubscriptionInfo(productId);
        }

        public bool IsPurchased(string productId) {
            return false;
        }

        public UniTask<bool> Purchase(string productId) {
            return UniTask.FromResult(false);
        }

        public UniTask<bool> Restore() {
            return UniTask.FromResult(false);
        }

        public UniTask<bool> RemoveNonConsumable(string productId) {
            return UniTask.FromResult(false);
        }

        public UniTask<bool> RemoveAllNonConsumable() {
            return UniTask.FromResult(false);
        }
    }
}