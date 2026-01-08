using System;

namespace Senspark.AdCampaigns {
    public enum AdCampaignType {
        /// <summary>
        /// Chưa biết
        /// </summary>
        Unknown,

        /// <summary>
        /// Ko thuộc campaign nào
        /// </summary>
        None,

        /// <summary>
        /// Campaign thông thường
        /// </summary>
        General,

        /// <summary>
        /// Campaign hướng đến mua hàng
        /// </summary>
        Iap
    }

    public interface IAdCampaignManager : IDisposable {
        AdCampaignType CampaignType { get; }
    }
}