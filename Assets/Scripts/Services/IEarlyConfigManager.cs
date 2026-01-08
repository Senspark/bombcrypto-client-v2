using System.Threading.Tasks;

using Constant;

using Data;

using Senspark;

namespace Services {
    [Service(nameof(IEarlyConfigManager))]
    public interface IEarlyConfigManager : IService {
        int[] DisableFeatures { get; }
        EarlyConfigHeroData[] Heroes { get; }
        EarlyConfigRankData[] Ranks { get; }
        bool IsGetTrHero { get; }
        int CurrentSeason { get; }
        EarlyConfigItemData[] Items { get; }
        int[] PvEBoosterIds { get; }
        int[] PvPBoosterIds { get; }
        int UpdateStatus { get; }
        bool IsDisableFeature(FeatureId featureId);
        void Initialize(string json);
        Task InitializeAsync();
    }
}