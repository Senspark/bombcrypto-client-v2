namespace Senspark.Iap.Validators {
    public readonly struct PurchaseToken {
        public readonly string Token;
        public readonly string OrderId;

        public PurchaseToken(string token, string orderId) {
            Token = token;
            OrderId = orderId;
        }
    }

    public interface ILocalValidator {
        PurchaseToken GetPurchaseToken(PurchaseRawReceipt rawReceipt);
    }
}