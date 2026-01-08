using JetBrains.Annotations;
using Senspark.Iap.CheatDetection;

namespace Senspark.Iap.Validators {
    public class DefaultLocalReceiptValidator : ILocalValidator {
        private readonly ILocalValidator _bridge;

        public DefaultLocalReceiptValidator(
            [CanBeNull] byte[] googleBillingLicenseCode,
            ICheatPunishment cheatPunishment
        ) {
#if UNITY_EDITOR
            _bridge = new EditorLocalReceiptValidator();
#elif UNITY_ANDROID
            if (googleBillingLicenseCode == null) {
                _bridge = new EditorLocalReceiptValidator();
            } else {
                _bridge = new AndroidLocalReceiptValidator(googleBillingLicenseCode, cheatPunishment);
            }
#elif UNITY_IOS
            _bridge = new AppleLocalReceiptValidator(cheatPunishment);
#endif
        }

        public PurchaseToken GetPurchaseToken(PurchaseRawReceipt rawReceipt) {
            return _bridge.GetPurchaseToken(rawReceipt);
        }
    }
}