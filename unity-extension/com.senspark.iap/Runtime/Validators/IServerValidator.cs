using Cysharp.Threading.Tasks;
using UnityEngine.Purchasing;

namespace Senspark.Iap.Validators {
    public interface IServerValidator {
        UniTask<ValidateResult> Validate(string productId, PurchaseToken purchaseToken, string appPackageName,
            ProductType productType);
    }
}