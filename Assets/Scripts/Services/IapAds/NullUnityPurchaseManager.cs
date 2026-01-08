using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.IapAds {
    public class NullUnityPurchaseManager : IUnityPurchaseManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public Task SyncData() {
            return Task.CompletedTask;
        }

        public Task<PurchaseResult> PurchaseItem(string productId) {
            throw new Exception("Not supported on your platform");
        }

        public void ConsumePurchaseItem(string productId) {
        }

        public IapPrice GetItemPrice(string productId) {
            return new IapPrice(0, "ERROR", "ERROR");
        }

        public bool HasSubscription(string productId) {
            return false;
        }

        public List<PurchaseResult> GetPendingTransactions() {
            return new List<PurchaseResult>();
        }
    }
}