using System;
using JetBrains.Annotations;
using Senspark.Iap.CheatDetection;
using UnityEngine.Purchasing.Security;

namespace Senspark.Iap.Validators {
    public class AndroidLocalReceiptValidator : ILocalValidator {
        private readonly CrossPlatformValidator _crossPlatformValidator;
        private readonly ICheatPunishment _cheatPunishment;

        public AndroidLocalReceiptValidator([CanBeNull] byte[] googleBillingLicenseCode, ICheatPunishment cheatPunishment) {
            if (googleBillingLicenseCode == null || googleBillingLicenseCode.Length == 0) {
                return;
            }

            _cheatPunishment = cheatPunishment;
            try {
                var package = UnityEngine.Application.identifier;
                var validator = new CrossPlatformValidator(googleBillingLicenseCode, null, package);
                _crossPlatformValidator = validator;
            }
            catch (Exception e) {
                FastLog.Error(e.Message);
            }
        }

        public PurchaseToken GetPurchaseToken(PurchaseRawReceipt rawReceipt) {
            if (_crossPlatformValidator == null || rawReceipt == null) {
                return new PurchaseToken();
            }

            try {
                var validated = _crossPlatformValidator.Validate(rawReceipt.receipt);
                if (validated.Length == 0) {
                    return new PurchaseToken();
                }

                var firstReceipt = validated[0];
                var parsed = (GooglePlayReceipt) firstReceipt;
                var sameProduct = parsed.productID == rawReceipt.productId;
                var validToken = !string.IsNullOrWhiteSpace(parsed.purchaseToken);
                var purchased = parsed.purchaseState == GooglePurchaseState.Purchased;
                var validateResult = sameProduct && validToken && purchased;
                var token = validateResult ? parsed.purchaseToken : string.Empty;
                return new PurchaseToken(token, parsed.orderID);
            }
            catch (InvalidSignatureException s) {
                FastLog.Error(s.Message);
                _cheatPunishment.Punish();
                return new PurchaseToken();
            }
            catch (Exception e) {
                FastLog.Error(e.Message);
                return new PurchaseToken();
            }
        }
    }
}