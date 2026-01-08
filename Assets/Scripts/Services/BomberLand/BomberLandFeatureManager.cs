using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Constant;

using Services;

using UnityEngine;

namespace App {
    public class BomberLandFeatureManager : IFeatureManager {
        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public bool IsUsingMetaMask { get; }
        public bool EnableControlMining { get; }
        public bool EnableDeposit { get; }
        public bool EnableUpgrade { get; }
        public bool EnableRepairShield { get; }
        public bool EnableResetSkill { get; }
        public bool EnableCreateAccount { get; }
        public bool EnableRename { get; }
        public bool EnableShopForUserFi { get; }
        public bool EnablePlayForFun { get; }
        public bool EnableClaim { get; }
        public bool EnableInvestedDetail { get; }
        public bool EnableResetNerf { get; }
        public bool EnableStake { get; }
        public bool CanStakeHero { get; }
        public bool EnableBHero { get; }
        public bool EnableBHeroS { get; }
        public bool EnableBuyBombToken { get; }
        public bool EnableStoryMode { get; }
        public bool EnablePvpMode { get; }
        public bool EnableMarketingTournament { get; set; }
        public bool EnableLaunchPadOtherTokens { get; }
        public bool EnableSen { get; }
        public bool WarningHeroS { get; }
        public bool LimitHeroDangerousEffect { get; }
        public bool WarningAutoMine { get; }
        public bool EnableBuyAutoMine { get; }
        public bool EnableFusion { get; }
        public bool EnableBannerHunt { get; }
        public bool EnableBannerBirthday { get; }
        public bool EnableVoucher { get; }
        public bool EnableNews { get; }
        public bool EnableMarketplace { get; }
        public bool EnableEmail { get; }
        public bool EnableLuckyWheel { get; }
        public bool EnableDailyMission { get; }
        public bool EnableShopSwapGem { get; }
        public bool EnableShopChest { get; }
        public bool EnableShopGem { get; }
        public bool EnableInventoryListingItem { get; }
        public bool EnableTreasureHunt { get; }
        public bool ShowHeroSIcon { get; }

        public BomberLandFeatureManager(UserAccount acc) {
            var userFi = acc.isUserFi;
            var walletOnly = userFi && acc.loginType == LoginType.Wallet;
            var bscOnly = userFi && acc.network == NetworkType.Binance;
            var polygonOnly = userFi && acc.network == NetworkType.Polygon;

            IsUsingMetaMask = walletOnly;
            EnableControlMining = false;
            EnableDeposit = walletOnly || AppConfig.IsAirDrop();
            EnableUpgrade = false;
            EnableRepairShield = walletOnly;
            EnableResetSkill = false;
            EnableCreateAccount = userFi;
            EnableRename = userFi;
            EnableShopForUserFi = walletOnly || AppConfig.IsAirDrop();
            EnablePlayForFun = false;
            EnableClaim = walletOnly || AppConfig.IsAirDrop();
            EnableInvestedDetail = false;
            EnableResetNerf = false;
            EnableStake = walletOnly && bscOnly;
            CanStakeHero = walletOnly;
            EnableBHero = false;
            EnableBHeroS = true;
            EnableStoryMode = true;
            EnablePvpMode = true;
            EnableMarketingTournament = false;
            EnableLaunchPadOtherTokens = false;
            EnableSen = false;
            WarningHeroS = false;
            WarningAutoMine = true;
            LimitHeroDangerousEffect = true;
            EnableBuyAutoMine = walletOnly || AppConfig.IsAirDrop();
            EnableFusion = walletOnly || AppConfig.IsAirDrop();
            EnableBannerHunt = userFi;
            EnableBannerBirthday = false;
            EnableVoucher = userFi;
            EnableNews = true;
            EnableMarketplace = true;
            EnableEmail = userFi;
            EnableLuckyWheel = false;
            EnableDailyMission = false;
            EnableShopSwapGem = userFi; // & !Application.isMobilePlatform;
            EnableShopChest = userFi; // && !Application.isMobilePlatform;
            EnableShopGem = Application.isMobilePlatform;
            EnableInventoryListingItem = userFi;
            EnableTreasureHunt = userFi;
            ShowHeroSIcon = true;
        }
    }
}