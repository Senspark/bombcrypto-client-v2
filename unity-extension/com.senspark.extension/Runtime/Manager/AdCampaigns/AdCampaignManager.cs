using JetBrains.Annotations;

using Manager;

using Senspark.AdCampaigns.Internal;

using UnityEngine.Assertions;

namespace Senspark.AdCampaigns {
    public static class AdCampaignManager {
        public class Builder {
            [NotNull]
            public IRevenueValidator RevenueValidator { get; set; }

            [NotNull]
            public IRemoteConfigManager RemoteConfig { get; set; }

            [NotNull]
            public IAnalyticsManager Analytics { get; set; }

            [NotNull]
            public IDataManager DataManager { get; set; }

            public IAdCampaignManager Build() {
                Assert.IsNotNull(RevenueValidator, $"{nameof(RevenueValidator)} is null");
                Assert.IsNotNull(RemoteConfig, $"{nameof(RemoteConfig)} is null");
                Assert.IsNotNull(Analytics, $"{nameof(Analytics)} is null");
                Assert.IsNotNull(DataManager, $"{nameof(DataManager)} is null");

                return new DefaultAdCampaignManager(RevenueValidator, RemoteConfig, Analytics, DataManager);
            }
        }
    }
}