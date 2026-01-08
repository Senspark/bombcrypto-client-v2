using System.Collections.Generic;
using System.Threading.Tasks;

using Analytics;

using Senspark;

namespace Services.IapAds {
    public class DefaultUnityPurchaseManager : IUnityPurchaseManager {
        private IUnityPurchaseManager _bridge;

        public DefaultUnityPurchaseManager(ILogManager logManager, IAnalytics analytics) {
#if UNITY_WEBGL
            _bridge = new NullUnityPurchaseManager();
#else
            _bridge = new MobileUnityPurchaseManager(logManager, analytics);
#endif
        }

        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() {
            _bridge.Destroy();
            _bridge = null;
        }

        public Task SyncData() {
            return _bridge.SyncData();
        }

        public Task<PurchaseResult> PurchaseItem(string productId) {
            return _bridge.PurchaseItem(productId);
        }

        public void ConsumePurchaseItem(string productId) {
            _bridge.ConsumePurchaseItem(productId);
        }

        public IapPrice GetItemPrice(string productId) {
            return _bridge.GetItemPrice(productId);
        }

        public bool HasSubscription(string productId) {
            return _bridge.HasSubscription(productId);
        }

        public List<PurchaseResult> GetPendingTransactions() {
            return _bridge.GetPendingTransactions();
        }
    }
}