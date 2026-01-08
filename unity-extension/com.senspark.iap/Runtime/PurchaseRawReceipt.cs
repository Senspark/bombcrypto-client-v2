namespace Senspark.Iap {
    public class PurchaseRawReceipt {
        public readonly string productId;
        public readonly string receipt;

        public PurchaseRawReceipt(string productId, string receipt) {
            this.productId = productId;
            this.receipt = receipt;
        }
    }
}