using System.Threading.Tasks;

using Senspark;

namespace App {
    [Service(nameof(IFeatureManager))]
    public interface IFeatureManager : IService {
        bool IsUsingMetaMask { get; }
        bool EnableControlMining { get; }
        bool EnableDeposit { get; }
        bool EnableUpgrade { get; }
        bool EnableRepairShield { get; }
        bool EnableResetSkill { get; }
        bool EnableCreateAccount { get; }
        bool EnableRename { get; }
        bool EnableShopForUserFi { get; }
        bool EnablePlayForFun { get; }
        bool EnableClaim { get; }
        bool EnableInvestedDetail { get; }
        bool EnableResetNerf { get; }
        bool EnableStake { get; }
        bool CanStakeHero { get; }
        bool EnableBHero { get; }
        bool EnableBHeroS { get; }
        bool EnableBuyBombToken { get; }
        bool EnableStoryMode { get; }
        bool EnablePvpMode { get; }
        bool EnableLaunchPadOtherTokens { get; }
        bool EnableSen { get; }
        bool WarningHeroS { get; }
        bool LimitHeroDangerousEffect { get; }
        bool WarningAutoMine { get; }
        bool EnableBuyAutoMine { get; }
        bool EnableFusion { get; }
        bool EnableBannerHunt { get; }
        bool EnableBannerBirthday { get; }
        bool EnableVoucher { get; }
        bool EnableNews { get; }
        bool EnableMarketplace { get; }
        bool EnableEmail { get; }
        bool EnableLuckyWheel { get; }
        bool EnableDailyMission { get; }
        bool EnableShopSwapGem { get; }
        bool EnableShopChest { get; }
        bool EnableShopGem { get; }
        bool EnableInventoryListingItem { get; }
        bool EnableTreasureHunt { get; }
        bool ShowHeroSIcon { get; }
    }
}