using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;

namespace Senspark.Iap.Validators {
    public class NullServerValidator : IServerValidator {
        public UniTask<ValidateResult> Validate(string productId, PurchaseToken purchaseToken, string appPackageName,
            ProductType productType) {
            var valid = !string.IsNullOrWhiteSpace(productId)
                        && !string.IsNullOrWhiteSpace(purchaseToken.Token)
                        && !string.IsNullOrWhiteSpace(appPackageName);
            return UniTask.FromResult(new ValidateResult(valid, false, "Will not validate"));
        }
    }
}