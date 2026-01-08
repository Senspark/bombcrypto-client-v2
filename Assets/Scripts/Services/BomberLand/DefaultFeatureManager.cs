#define FORCE_EDITOR_FEATURE

using System.Threading.Tasks;

using Services;

namespace App {
    public class DefaultFeatureManager : IFeatureManager {
        private readonly IFeatureManager _bridge;

        public DefaultFeatureManager(UserAccount acc) {
#if UNITY_EDITOR && FORCE_EDITOR_FEATURE
            _bridge = new EditorFeatureManager();
#else
            _bridge = new BomberLandFeatureManager(acc);
#endif
        }

        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() {
            _bridge.Destroy();
        }

        public bool IsUsingMetaMask => _bridge.IsUsingMetaMask;
        public bool EnableControlMining => _bridge.EnableControlMining;
        public bool EnableDeposit => _bridge.EnableDeposit;
        public bool EnableUpgrade => _bridge.EnableUpgrade;
        public bool EnableRepairShield => _bridge.EnableRepairShield;
        public bool EnableResetSkill => _bridge.EnableResetSkill;
        public bool EnableCreateAccount => _bridge.EnableCreateAccount;
        public bool EnableRename => _bridge.EnableRename;
        public bool EnableShopForUserFi => _bridge.EnableShopForUserFi;
        public bool EnablePlayForFun => _bridge.EnablePlayForFun;
        public bool EnableClaim => _bridge.EnableClaim;
        public bool EnableInvestedDetail => _bridge.EnableInvestedDetail;
        public bool EnableResetNerf => _bridge.EnableResetNerf;
        public bool EnableStake => _bridge.EnableStake;
        public bool CanStakeHero => _bridge.CanStakeHero;
        public bool EnableBHero => _bridge.EnableBHero;
        public bool EnableBHeroS => _bridge.EnableBHeroS;
        public bool EnableBuyBombToken => _bridge.EnableBuyBombToken;
        public bool EnableStoryMode => _bridge.EnableStoryMode;
        public bool EnablePvpMode => _bridge.EnablePvpMode;
        public bool EnableLaunchPadOtherTokens => _bridge.EnableLaunchPadOtherTokens;
        public bool EnableSen => _bridge.EnableSen;
        public bool WarningHeroS => _bridge.WarningHeroS;
        public bool LimitHeroDangerousEffect => _bridge.LimitHeroDangerousEffect;
        public bool WarningAutoMine => _bridge.WarningAutoMine;
        public bool EnableBuyAutoMine => _bridge.EnableBuyAutoMine;
        public bool EnableFusion => _bridge.EnableFusion;
        public bool EnableBannerHunt => _bridge.EnableBannerHunt;
        public bool EnableBannerBirthday => _bridge.EnableBannerBirthday;
        public bool EnableVoucher => _bridge.EnableVoucher;
        public bool EnableNews => _bridge.EnableNews;
        public bool EnableMarketplace => _bridge.EnableMarketplace;
        public bool EnableEmail => _bridge.EnableEmail;
        public bool EnableLuckyWheel => _bridge.EnableLuckyWheel;
        public bool EnableDailyMission => _bridge.EnableDailyMission;
        public bool EnableShopSwapGem => _bridge.EnableShopSwapGem;
        public bool EnableShopChest => _bridge.EnableShopChest;
        public bool EnableShopGem => _bridge.EnableShopGem;
        public bool EnableInventoryListingItem => _bridge.EnableInventoryListingItem;
        public bool EnableTreasureHunt => _bridge.EnableTreasureHunt;
        public bool ShowHeroSIcon => _bridge.ShowHeroSIcon;
    }
}