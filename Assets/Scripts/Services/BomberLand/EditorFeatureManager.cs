using System.Threading.Tasks;

namespace App {
    public class EditorFeatureManager : IFeatureManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public bool IsUsingMetaMask => true;
        public bool EnableControlMining => true;
        public bool EnableDeposit => true;
        public bool EnableUpgrade => false;
        public bool EnableRepairShield => true;
        public bool EnableResetSkill => true;
        public bool EnableCreateAccount => true;
        public bool EnableRename => true;
        public bool EnableShopForUserFi => true;
        public bool EnablePlayForFun => true;
        public bool EnableClaim => true;
        public bool EnableInvestedDetail => true;
        public bool EnableResetNerf => true;
        public bool EnableStake => true;
        public bool CanStakeHero => !ScreenUtils.IsIPadScreen();
        public bool EnableBHero => true;
        public bool EnableBHeroS => true;
        public bool EnableBuyBombToken => false;
        public bool EnableStoryMode => true;
        public bool EnablePvpMode => true;
        public bool EnableLaunchPadOtherTokens => true;
        public bool EnableSen => false;
        public bool WarningHeroS => false;
        public bool LimitHeroDangerousEffect => true;
        public bool WarningAutoMine => true;
        public bool EnableBuyAutoMine => true;
        public bool EnableFusion => true;
        public bool EnableBannerHunt => false;
        public bool EnableBannerBirthday => true;
        public bool EnableVoucher => true;
        public bool EnableNews => true;
        public bool EnableMarketplace => true;
        public bool EnableEmail => true;
        public bool EnableLuckyWheel => true;
        public bool EnableDailyMission => true;
        public bool EnableShopSwapGem => true;
        public bool EnableShopChest => true;
        public bool EnableShopGem => true;
        public bool EnableInventoryListingItem => true;
        public bool EnableTreasureHunt => true;
        public bool EnableInventory => true;
        public bool EnablePvE => true;
        public bool EnablePvP => true;
        public bool ShowHeroSIcon => true;
    }
}