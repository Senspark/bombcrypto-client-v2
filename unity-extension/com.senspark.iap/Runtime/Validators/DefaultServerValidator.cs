using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;

namespace Senspark.Iap.Validators {
    public class DefaultServerValidator : IServerValidator {
        private readonly IServerValidator _serverValidator;

        public DefaultServerValidator(bool useServerVerify) {
#if UNITY_EDITOR
            _serverValidator = new NullServerValidator();
#else
            _serverValidator = useServerVerify
                ? new SensparkServerValidator()
                : new NullServerValidator();
#endif
        }

        public UniTask<ValidateResult> Validate(string productId, PurchaseToken purchaseToken, string appPackageName,
            ProductType productType) {
            return _serverValidator.Validate(productId, purchaseToken, appPackageName, productType);
        }
    }
}