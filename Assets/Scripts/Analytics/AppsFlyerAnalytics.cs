using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;
using Senspark;

using UnityEngine;

namespace Analytics {
    public class AppsFlyerAnalytics : IAnalytics {
        private readonly IAppsFlyerAnalyticsBridge _bridge;
        private static readonly string KeyTrackedEvent = $"{nameof(AppsFlyerAnalytics)}_tracked";
        private static readonly string KeyActiveUserEvent = $"{nameof(AppsFlyerAnalytics)}_active_user";

        private readonly Dictionary<ConversionType, string> _tags = new() {
            {ConversionType.ConnectWalletBnb, "conversion_connect_wallet_bnb"},
            {ConversionType.ConnectWalletPolygon, "conversion_connect_wallet_polygon"},
            {ConversionType.ConnectAccount, "conversion_connect_account"},
            {ConversionType.ConnectAccountBnb, "conversion_connect_account_bnb"},
            {ConversionType.ConnectAccountPolygon, "conversion_connect_account_polygon"},
            {ConversionType.ConnectGuest, "conversion_guest"},
            {ConversionType.ConnectFacebook, "conversion_connect_facebook"},
            {ConversionType.ConnectApple, "conversion_connect_apple"},
            {ConversionType.ConnectTelegram, "conversion_connect_telegram"},
            {ConversionType.ConnectSolana, "conversion_connect_solana"},
            {ConversionType.ChooseBnb, null},
            {ConversionType.ChoosePolygon, null},
            {ConversionType.NewUser, "conversion_nru"},
            {ConversionType.FirstOpen, "conversion_first_open"},
            {ConversionType.BuyHeroFi, "conversion_buy_hero"},
            {ConversionType.BuyHeroFiFail, "conversion_buy_hero_fail"},
            {ConversionType.BuyHouse, "conversion_buy_house"},
            {ConversionType.BuyHouseFail, "conversion_buy_house_fail"},
            {ConversionType.BuyIap, "conversion_iap"},
            {ConversionType.BuyIapFail, null},
            {ConversionType.BuySoftCurrency, "conversion_soft_currency"},
            {ConversionType.BuySoftCurrencyFail, null},
            {ConversionType.BuyGachaChest, "conversion_buy_gacha_chest"},
            {ConversionType.BuyGachaChestFail, null},
            {ConversionType.OpenChestUseGem, "conversion_open_chest_use_gem"},
            {ConversionType.OpenChestUseGemFail, null},
            {ConversionType.OpenChestWatchAds, "conversion_open_chest_watch_ads"},
            {ConversionType.OpenChest, "conversion_open_chest"},
            {ConversionType.BuyMarketHeroTr, "conversion_buy_hero"},
            {ConversionType.BuyMarketHeroTrFail, "conversion_buy_hero_fail"},
            {ConversionType.BuyMarketItem, "conversion_buy_item_market"},
            {ConversionType.BuyMarketItemFail, null},
            {ConversionType.BuyHeroTrFail, "conversion_buy_hero_fail"},
            {ConversionType.GetHeroTrFree, "conversion_get_hero_free"},
            {ConversionType.X2GoldPveWatchAds, "conversion_x2_gold_pve_watch_ads"},
            {ConversionType.UsedBoosterBombPve, "conversion_bomb_pve"},
            {ConversionType.UsedBoosterRangePve, "conversion_range_pve"},
            {ConversionType.UsedBoosterSpeedPve, "conversion_speed_pve"},
            {ConversionType.UsedBoosterShieldPve, "conversion_shield_pve"},
            {ConversionType.X2GoldPvpWatchAds, "conversion_x2_gold_pvp_watch_ads"},
            {ConversionType.UsedBoosterBombPvp, "conversion_bomb_pvp"},
            {ConversionType.UsedBoosterRangePvp, "conversion_range_pvp"},
            {ConversionType.UsedBoosterSpeedPvp, "conversion_speed_pvp"},
            {ConversionType.UsedBoosterKeyPvp, "conversion_key_pvp"},
            {ConversionType.UsedBoosterShieldPvp, "conversion_shield_pvp"},
            {ConversionType.UsedBoosterFullConquestCardPvp, "conversion_full_cup_card_pvp"},
            {ConversionType.UsedBoosterFullRankGuardianPvp, "conversion_full_rank_gua_pvp"},
            {ConversionType.UsedBoosterConquestCardPvp, "conversion_cup_card_pvp"},
            {ConversionType.UsedBoosterRankGuardianPvp, "conversion_rank_gua_pvp"},
            {ConversionType.ClickSwapGem, "conversion_click_swap_gem"},
            {ConversionType.SwapGem, "conversion_swap_gem"},
            {ConversionType.UseAutoMine, null},
            {ConversionType.UserFiClickMarket, null},
            {ConversionType.UserTrClickMarket, null},
            {ConversionType.PlayAdventure, null},
            {ConversionType.UseUpBoosterPvp, null},
            {ConversionType.UseUpBoosterAdventure, null},
            {ConversionType.ClickActiveChest, null},
            {ConversionType.WatchAds, null},
            {ConversionType.RevivePveByAds, "conversion_revive_by_ads"},
            {ConversionType.RevivePveByGem, "conversion_revive_by_gem"},
            {ConversionType.LuckyWheelPveWatchAds, "conversion_lucky_wheel_pve_watch_ads"},
            {ConversionType.LuckyWheelPvpWatchAds, "conversion_lucky_wheel_pvp_watch_ads"},
            {ConversionType.OverwriteAccount, "conversion_overwrite_account"},
            {ConversionType.VisitGemShop, "conversion_visit_gem_shop"},
            {ConversionType.ShowStarterPack, "conversion_show_starter_pack"},
            {ConversionType.TapBtnBuyStarterPack, "conversion_tap_button_in_starter_pack"},
            {ConversionType.ShowPremiumPack, "conversion_show_premium_pack"},
            {ConversionType.TapBtnBuyPremiumPack, "conversion_tap_button_in_premium_pack"},
            {ConversionType.ShowHeroPack, "conversion_show_hero_pack"},
            {ConversionType.TapBtnBuyHeroPack, "conversion_tap_button_in_hero_pack"},
        };
        
        private readonly Dictionary<int, string> _pvpMatchTags = new() {
            {1, "conversion_play_1_matches"},
            {2, "conversion_play_2_matches"},
            {3, "conversion_play_3_matches"},
            {4, "conversion_play_4_matches"},
            {5, "conversion_play_5_matches_fix_230413"},
            {10, "conversion_play_10_matches"},
            {20, "conversion_play_20_matches"},
            {30, "conversion_play_30_matches"},
        };

        private readonly Dictionary<int, string> _pvpWinMatchTags = new() {
            {20, "conversion_win_20_matches"}
        };
        
        private readonly Dictionary<(int level, int stage), string> _pveLevelStageTags = new() {
            {(1, 1), "conversion_complete_pve_stage1_level_1"},
            {(2, 1), "conversion_complete_pve_stage1_level_2"},
            {(3, 1), "conversion_complete_pve_stage1_level_3"},
            {(4, 1), "conversion_complete_pve_stage1_level_4"},
            {(5, 1), "conversion_complete_stage_1_pve"},
            {(5, 2), "conversion_complete_stage_2_pve"},
            {(5, 3), "conversion_complete_stage_3_pve"},
        };
        
        private readonly List<string> _tracked;

        public AppsFlyerAnalytics(bool production, ILogManager logManager) {
            var platform = Application.platform;
            const bool enableDebug = false;
            _bridge = (production, platform) switch {
                (true, RuntimePlatform.WebGLPlayer) => new WebGLAppsFlyerAnalyticsBridge(logManager, AppConfig.AppsFlyerWebDevKey),
                (true, RuntimePlatform.Android or RuntimePlatform.IPhonePlayer) => new MobileAppsFlyerAnalyticsBridge(
                    logManager, AppConfig.AppsFlyerIosAppId, AppConfig.AppsFlyerAndroidDevKey, enableDebug),
                _ => new NullAnalyticsBridge(logManager, nameof(AppsFlyerAnalytics))
            };

            _tracked = AnalyticsUtils.LoadTrackedEvent(KeyTrackedEvent);
        }

        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() {
        }
        
        public void SetDependencies(IAnalyticsDependency dependency) {
        }

        #region CONVERSION

        public void TrackConversion(ConversionType type) {
#if UNITY_EDITOR
            if (!_tags.ContainsKey(type)) {
                throw new Exception($"Invalid AppsFlyer Analytics key: {type}");
            }
#endif
            if (_tags.ContainsKey(type)) {
                TrackConversion(_tags[type]);
            }
        }

        public void TrackConversionRetention() {
        }

        public void TrackConversionPvpPlay(int matchCount, int winMatch) {
            // track conversion played match
            if (_pvpMatchTags.ContainsKey(matchCount)) {
                TrackConversion(_pvpMatchTags[matchCount]);
            }
            
            // track conversion win match
            if (_pvpWinMatchTags.ContainsKey(winMatch)) {
                TrackConversion(_pvpWinMatchTags[winMatch]);
            }
        }

        public void TrackConversionAdventurePlay(int level, int stage) {
            var d = (level, stage);
            if (_pveLevelStageTags.ContainsKey(d)) {
                TrackConversion(_pveLevelStageTags[d]);
            }
        }
        
        public void TrackConversionActiveUser() {
            if (!AnalyticsUtils.CanTrackActiveUserToday(KeyActiveUserEvent)) {
                return;
            }
            _bridge.LogEvent("active_user");
        }

        private void TrackConversion(string evName) {
            if (string.IsNullOrWhiteSpace(evName) || _tracked.Contains(evName)) {
                return;
            }
            _tracked.Add(evName);
            AnalyticsUtils.SaveTrackedEvent(KeyTrackedEvent, _tracked);
            _bridge.LogEvent(evName);
        }

        #endregion

        #region SCENE

        public void TrackScene(SceneType type) {
        }

        public void TrackSceneAndSub(SceneType type, string sub) {
        }

        public void TrackData(string name, Dictionary<string, object> parameters) {
            _bridge.LogEvent(name, parameters);
        }

        #endregion

        public void TrackRate(int pointRating) {
        }

        public void TrackClickPlayPvp(int heroId) {
        }

        public void TrackClickPlayPve(int heroId, int level, int stage) {
        }

        public void TrackOfflineReward(int timeOffline, string type, string[] names, string[] values) {
        }

        public void TrackClaimRewardTutorial() {
        }
        
        public void TrackGetHeroTrFree(string heroName, int heroId) {
        }

        public void TreasureHunt_TrackCompleteMap() {
        }

        public void TreasureHunt_TrackPlayingTime(float seconds) {
        }

        public void Iap_TrackBuyIap(string transactionId, string productId, int value, TrackResult result) {
        }

        public void Iap_TrackBuyGold(int productId, int gemLockSpending, int gemSpending, int goldReceiving, TrackResult result) {
        }

        public void Iap_TrackGetGoldFree(string transactionId, string productId, int value, TrackResult result) {
        }

        public void Iap_TrackBuyGachaChest(int productId, string productName, string sinkType, float coinSpending, int chestReceiving,
            string[] items, string[] values, TrackResult result) {
        }

        public void Iap_TrackSoftCurrencyBuyGachaChest(string sinkType, float coinSpending, TrackResult result) {
        }

        public void Iap_TrackSoftCurrencyOpenGachaChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
        }

        public void Iap_TrackSoftCurrencyBuySlotChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
        }

        public void Iap_TrackOpenSwapGem() {
        }

        public void Iap_TrackSwapGem(float gemSpending, float tokenReceiving, TrackResult result) {
        }

        public void Pvp_TrackActiveBooster(string boosterName) {
        }

        public void Pvp_TrackPlay(int winUserId, int loseUserId, int totalTime, TrackPvpMatchResult result,
            TrackPvpLoseReason reason) {
        }

        public void Pvp_TrackCollectItems(Dictionary<TrackPvpCollectItemType, int> collected, TrackPvpMatchResult result) {
        }

        public void Pvp_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
        }

        public void Pve_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
        }

        public void Pvp_TrackInPrison(int amount) {
        }
        
        public void Pvp_TrackSoftCurrencyByWin(float value) {
        }

        public void Pvp_TrackSoftCurrencyByX2Gold(float value) {
        }

        public void MarketPlace_TrackProduct(string productName, float price, int amount, MarketPlaceResult result) {
        }

        public void MarketPlace_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            TrackResult result
        ) {
        }

        public void MarketPlace_TrackBuyHeroTr(string heroName, int[] heroIds) {
        }

        public void Inventory_TrackOpenChestByGem(
            string chestType,
            int chestId,
            string[] receiveProductName,
            int[] receiveProductQuantity
        ) {
        }

        public void Inventory_TrackEquipItem(int itemType, string itemName, int itemId) {
        }

        public void Inventory_TrackOpenChestGetHeroTr(string[] heroesNames, int[] heroesIds) {
        }

        public void Adventure_TrackPlay(int heroId, int level, int stage, float totalTime, TrackPvpMatchResult result) {
        }

        public void Adventure_TrackUseBooster(int level, int stage, string boosterName) {
        }

        public void Adventure_TrackCollectItems(int level, int stage,
            Dictionary<TrackPvpCollectItemType, int> collected, TrackPvpMatchResult result) {
        }
        
        public void Adventure_TrackSoftCurrencyByWin(int level, int stage, float value) {
        }

        public void Adventure_TrackSoftCurrencyByX2Gold(int level, int stage, float value) {
        }

        public void Adventure_TrackSoftCurrencyReviveByAds(int level, int stage, int reviveTimes) {
        }

        public void Adventure_TrackSoftCurrencyReviveByGem(int level, int stage, float valueLock, float valueUnlock, int reviveTimes) {
        }
        
        public void TrackAds(AdsCategory adsCategory, TrackAdsResult result) {
        }

        public void TrackAds(string category, TrackAdsResult result) {
        }

        public void TrackInterstitial(AdsCategory adsCategory, TrackAdsResult result) {
        }

        public void TrackLuckyWheel(string type, string mode, string[] names, string[] values, string matchResult, int level,
            int stage) {
        }

        public void DailyGift_TrackCollectItems(int level) {
        }

        public void ShopBuyCostumeUseGem_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        ) {
        }

        public void ShopBuyCostumeUseGold_TrackSoftCurrency(
            string productName,
            float goldSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        ) {
        }
        
        #region UPRADE HERO

        public void TrackCreateCrystal(string heroName, int heroAmount,
            int crap, int lesser, int rough, int pure, int perfect,
            int gold) {
        }

        public void TrackUpgradeCrystal(string rawCrystal, int rawAmount,
            string targetCrystal, int targetAmount,
            int gold, int gemLock, int gemUnlock) {
        }

        public void TrackUpgradeBaseIndex(int heroId, string heroName, string index, int level,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
        }

        public void TrackUpgradeMaxIndex(int heroId, string heroName, string index, int times,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
        }

        public void PushGameLevel(int levelNo, string levelMode) {
        }

        public void PopGameLevel(bool winGame) {
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit) {
            _bridge.LogAdRevenue(mediationNetwork, monetizationNetwork, revenue, currencyCode, format, adUnit, null);
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) {
            _bridge.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt);
        }

        #endregion        
    }
}