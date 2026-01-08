namespace Senspark.Iap.Receipts {
    public interface IReceipt {
        PurchaseState PurchaseState { get; }
        ConsumptionState ConsumptionState { get; }
        string TransactionId { get; }
        bool IsTestPurchase { get; }
    }
}