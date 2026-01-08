using Senspark.Iap.CheatDetection;

namespace Senspark.Iap.Validators {
    public class AppleLocalReceiptValidator : ILocalValidator {
        private readonly ICheatPunishment _cheatPunishment;

        public AppleLocalReceiptValidator(ICheatPunishment cheatPunishment) {
            _cheatPunishment = cheatPunishment;
        }

        public PurchaseToken GetPurchaseToken(PurchaseRawReceipt rawReceipt) {
            if (rawReceipt == null) {
                return new PurchaseToken();
            }

            var purchaseToken = rawReceipt.receipt;
            if (!string.IsNullOrWhiteSpace(purchaseToken)) {
                return new PurchaseToken(purchaseToken, string.Empty);
            }
            _cheatPunishment.Punish();
            return new PurchaseToken();
        }
    }
}