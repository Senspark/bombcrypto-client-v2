using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace Services.IapAds {
    public enum PurchaseState {
        Error, Cancel, Done
    }

    public class PurchaseResult {
        public PurchaseState State;
        public readonly string TransactionId;
        public readonly string PurchaseToken;
        public readonly string OrderId;
        public readonly string ProductId;
        public readonly string Receipt;

        public PurchaseResult(PurchaseState state, string transactionId, string purchaseToken, string orderId, string productId, string receipt) {
            State = state;
            TransactionId = transactionId;
            PurchaseToken = purchaseToken;
            OrderId = orderId;
            ProductId = productId;
            Receipt = receipt;
        }
    }

    public readonly struct IapPrice {
        public readonly float Price;
        public readonly string Currency;
        public readonly string FullString;
        
        public IapPrice(float price, string currency, string fullString) {
            Price = price;
            Currency = currency;
            FullString = fullString;
        }
    }

    [Service(nameof(IUnityPurchaseManager))]
    public interface IUnityPurchaseManager : IService {
        Task SyncData();
        Task<PurchaseResult> PurchaseItem(string productId);
        void ConsumePurchaseItem(string productId);
        IapPrice GetItemPrice(string productId);
        bool HasSubscription(string productId);
        List<PurchaseResult> GetPendingTransactions();
    }
}