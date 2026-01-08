using System.Threading.Tasks;

namespace App.BomberLand {
    public interface IBomberLandServerBridge : IServerManagerDelegate {
        Task DeleteUser(string accessToken);
        Task<bool> EnterPasscode(string passCode);
        Task<ValidateIapResult> ValidateBuyIapGem(ValidateIapRequest requestData);
        Task<ValidateIapResult> ValidateBuyIapOffer(ValidateIapRequest requestData);
        /// <summary>
        /// Buy Starter Pack, Hero Pack, Premium Pack 
        /// </summary>
        Task<IOfferPacksResult> GetOfferPacks();
        Task<bool> GetRemoveInterstitialAds();
    }

    public enum ValidateIapResult {
        Success, Failed, Existed
    }

    public class ValidateIapRequest {
        public readonly string TransactionId;
        public readonly string ProductId;
        public readonly string PurchaseToken;
        public readonly string PackageName;

        public ValidateIapRequest(string transactionId, string productId, string purchaseToken, string packageName) {
            TransactionId = transactionId;
            ProductId = productId;
            PurchaseToken = purchaseToken;
            PackageName = packageName;
        }
    }
}