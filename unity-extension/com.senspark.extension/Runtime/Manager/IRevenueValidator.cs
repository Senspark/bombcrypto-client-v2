using System;

using Newtonsoft.Json;

using Senspark;

namespace Manager {
    /// <summary>
    /// Dịch vụ xác thực doanh thu 
    /// </summary>
    public interface IRevenueValidator : IObserverManager<RevenueValidatorObserver>, IDisposable {
        PurchaseValidatedData LastPurchaseValidatedData { get; }
        AdCampaignData LastAdCampaignData { get; }
    }

    public class RevenueValidatorObserver {
        public Action<PurchaseValidatedData> OnIapRevenueValidated;
        public Action<AdCampaignData> OnAdCampaignDataReceived;
    }

    public class PurchaseValidatedData {
        public readonly bool IsSuccess;
        public readonly bool IsTestPurchase;
        public readonly string OrderId;
        public readonly string ProductId;

        public PurchaseValidatedData(string orderId, string productId, bool isSuccess, bool isTestPurchase) {
            OrderId = orderId;
            ProductId = productId;
            IsSuccess = isSuccess;
            IsTestPurchase = isTestPurchase;
        }
    }

    [Serializable]
    public class AdCampaignData {
        [JsonProperty("media_source")]
        public readonly string MediaSource = string.Empty;

        [JsonProperty("campaign")]
        public readonly string Campaign = string.Empty;

        [JsonProperty("campaign_id")]
        public readonly string CampaignId = string.Empty;

        [JsonProperty("is_first_launch")]
        public readonly bool IsFirstLaunch = false;

        [JsonProperty("af_status")]
        public readonly string AfStatus = string.Empty;
    }
}