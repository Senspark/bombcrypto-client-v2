namespace Senspark.Iap.Validators {
    public class EditorLocalReceiptValidator : ILocalValidator {
        public PurchaseToken GetPurchaseToken(PurchaseRawReceipt rawReceipt) {
            if (rawReceipt == null) {
                return new PurchaseToken();
            }

            var token = !string.IsNullOrWhiteSpace(rawReceipt.receipt) ? rawReceipt.receipt : "Editor Receipt";
            return new PurchaseToken(token, string.Empty);
        }
    }
}