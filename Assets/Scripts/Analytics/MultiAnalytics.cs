using System.Collections.Generic;
using System.Threading.Tasks;

using Senspark;

namespace Analytics {
    public class MultiAnalytics : IAnalytics {
        private readonly List<IAnalytics> _analytics = new();

        public Task<bool> Initialize() {
            _analytics.ForEach(e => e.Initialize());
            return Task.FromResult(true);
        }

        public void Destroy() {
            _analytics.ForEach(e => e.Destroy());
            _analytics.Clear();
        }

        public void AddBridge(IAnalytics analytics) {
            _analytics.Add(analytics);
        }

        public void SetDependencies(IAnalyticsDependency dependency) {
            _analytics.ForEach(e => e.SetDependencies(dependency));
        }

        public void TrackConversion(ConversionType type) {
            _analytics.ForEach(e => e.TrackConversion(type));
        }

        public void TrackConversionRetention() {
            _analytics.ForEach(e => e.TrackConversionRetention());
        }

        public void TrackConversionPvpPlay(int matchCount, int winMatch) {
            _analytics.ForEach(e => e.TrackConversionPvpPlay(matchCount, winMatch));
        }

        public void TrackConversionAdventurePlay(int level, int stage) {
            _analytics.ForEach(e => e.TrackConversionAdventurePlay(level, stage));
        }

        public void TrackConversionActiveUser() {
            _analytics.ForEach(e => e.TrackConversionActiveUser());
        }

        public void TrackScene(SceneType type) {
            _analytics.ForEach(e => e.TrackScene(type));
        }

        public void TrackSceneAndSub(SceneType type, string sub) {
            _analytics.ForEach(e => e.TrackSceneAndSub(type, sub));
        }

        public void TrackData(string name, Dictionary<string, object> parameters) {
            _analytics.ForEach(e => e.TrackData(name, parameters));
        }

        public void TrackRate(int pointRating) {
            _analytics.ForEach(e => e.TrackRate(pointRating));
        }

        public void TrackClickPlayPvp(int heroId) {
            _analytics.ForEach(e => e.TrackClickPlayPvp(heroId));
        }

        public void TrackClickPlayPve(int heroId, int level, int stage) {
            _analytics.ForEach(e => e.TrackClickPlayPve(heroId, level, stage));
        }

        public void TrackOfflineReward(int timeOffline, string type, string[] names, string[] values) {
            _analytics.ForEach(e => e.TrackOfflineReward(timeOffline, type, names, values));
        }

        public void TrackClaimRewardTutorial() {
            _analytics.ForEach(e => e.TrackClaimRewardTutorial());
        }

        public void TrackGetHeroTrFree(string heroName, int heroId) {
            _analytics.ForEach(e => e.TrackGetHeroTrFree(heroName, heroId));
        }

        public void TreasureHunt_TrackCompleteMap() {
            _analytics.ForEach(e => e.TreasureHunt_TrackCompleteMap());
        }

        public void TreasureHunt_TrackPlayingTime(float seconds) {
            _analytics.ForEach(e => e.TreasureHunt_TrackPlayingTime(seconds));
        }

        public void Iap_TrackBuyIap(string transactionId, string productId, int value, TrackResult result) {
            _analytics.ForEach(e => e.Iap_TrackBuyIap(transactionId, productId, value, result));
        }

        public void Iap_TrackBuyGold(int productId, int gemLockSpending, int gemSpending, int goldReceiving, TrackResult result) {
            _analytics.ForEach(e => e.Iap_TrackBuyGold(productId, gemLockSpending, gemSpending, goldReceiving, result));
        }

        public void Iap_TrackGetGoldFree(string transactionId, string productId, int value, TrackResult result) {
            _analytics.ForEach(e => e.Iap_TrackGetGoldFree(transactionId, productId, value, result));
        }
        public void Iap_TrackBuyGachaChest(int productId, string productName, string sinkType, float coinSpending, int chestReceiving,
            string[] items, string[] values, TrackResult result) {
            _analytics.ForEach(e =>
                e.Iap_TrackBuyGachaChest(productId, productName, sinkType, coinSpending, chestReceiving, items, values, result));
        }

        public void Iap_TrackSoftCurrencyBuyGachaChest(string sinkType, float coinSpending, TrackResult result) {
            _analytics.ForEach(e => e.Iap_TrackSoftCurrencyBuyGachaChest(sinkType, coinSpending, result));
        }

        public void Iap_TrackSoftCurrencyOpenGachaChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
            _analytics.ForEach(e => e.Iap_TrackSoftCurrencyOpenGachaChestByGem(gemLockSpending, gemSpending, result));
        }

        public void Iap_TrackSoftCurrencyBuySlotChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
            _analytics.ForEach(e => e.Iap_TrackSoftCurrencyBuySlotChestByGem(gemLockSpending, gemSpending, result));
        }
        
        public void Iap_TrackOpenSwapGem() {
            _analytics.ForEach(e => e.Iap_TrackOpenSwapGem());
        }

        public void Iap_TrackSwapGem(float gemSpending, float tokenReceiving, TrackResult result) {
            _analytics.ForEach(e => e.Iap_TrackSwapGem(gemSpending, tokenReceiving, result));
        }

        public void Pvp_TrackActiveBooster(string boosterName) {
            _analytics.ForEach(e => e.Pvp_TrackActiveBooster(boosterName));
        }

        public void Pvp_TrackPlay(int winUserId, int loseUserId, int totalTime, TrackPvpMatchResult result,
            TrackPvpLoseReason reason) {
            _analytics.ForEach(e => e.Pvp_TrackPlay(winUserId, loseUserId, totalTime, result, reason));
        }

        public void Pvp_TrackCollectItems(Dictionary<TrackPvpCollectItemType, int> collected,
            TrackPvpMatchResult result) {
            _analytics.ForEach(e => e.Pvp_TrackCollectItems(collected, result));
        }

        public void Pvp_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
            _analytics.ForEach(e => e.Pvp_TrackFullInventory(rejected));
        }

        public void Pve_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
            _analytics.ForEach(e => e.Pve_TrackFullInventory(rejected));
        }

        public void Pvp_TrackInPrison(int amount) {
            _analytics.ForEach(e => e.Pvp_TrackInPrison(amount));
        }

        public void Pvp_TrackSoftCurrencyByWin(float value) {
            _analytics.ForEach(e => e.Pvp_TrackSoftCurrencyByWin(value));
        }

        public void Pvp_TrackSoftCurrencyByX2Gold(float value) {
            _analytics.ForEach(e => e.Pvp_TrackSoftCurrencyByX2Gold(value));
        }
        
        public void MarketPlace_TrackProduct(string productName, float price, int amount, MarketPlaceResult result) {
            _analytics.ForEach(e => e.MarketPlace_TrackProduct(productName, price, amount, result));
        }

        public void MarketPlace_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            TrackResult result
        ) {
            _analytics.ForEach(e => e.MarketPlace_TrackSoftCurrency(
                productName,
                gemLockSpending,
                gemSpending,
                itemReceiving,
                result
            ));
        }

        public void MarketPlace_TrackBuyHeroTr(string heroName, int[] heroIds) {
            _analytics.ForEach(e => e.MarketPlace_TrackBuyHeroTr(heroName, heroIds));
        }

        public void Inventory_TrackOpenChestByGem(
            string chestType,
            int chestId,
            string[] receiveProductName,
            int[] receiveProductQuantity
        ) {
            _analytics.ForEach(e => e.Inventory_TrackOpenChestByGem(
                chestType,
                chestId,
                receiveProductName,
                receiveProductQuantity
            ));
        }

        public void Inventory_TrackEquipItem(int itemType, string itemName, int itemId) {
            _analytics.ForEach(e => e.Inventory_TrackEquipItem(itemType, itemName, itemId));
        }

        public void Inventory_TrackOpenChestGetHeroTr(string[] heroesNames, int[] heroesIds) {
            _analytics.ForEach(e => e.Inventory_TrackOpenChestGetHeroTr(heroesNames, heroesIds));
        }

        public void Adventure_TrackPlay(int heroId, int level, int stage, float totalTime, TrackPvpMatchResult result) {
            _analytics.ForEach(e => e.Adventure_TrackPlay(heroId, level, stage, totalTime, result));
        }

        public void Adventure_TrackUseBooster(int level, int stage, string boosterName) {
            _analytics.ForEach(e => e.Adventure_TrackUseBooster(level, stage, boosterName));
        }

        public void Adventure_TrackCollectItems(int level, int stage,
            Dictionary<TrackPvpCollectItemType, int> collected, TrackPvpMatchResult result) {
            _analytics.ForEach(e => e.Adventure_TrackCollectItems(level, stage, collected, result));
        }
        
        public void Adventure_TrackSoftCurrencyByWin(int level, int stage, float value) {
            _analytics.ForEach(e => e.Adventure_TrackSoftCurrencyByWin(level, stage, value));
        }

        public void Adventure_TrackSoftCurrencyByX2Gold(int level, int stage, float value) {
            _analytics.ForEach(e => e.Adventure_TrackSoftCurrencyByX2Gold(level, stage, value));
        }

        public void Adventure_TrackSoftCurrencyReviveByAds(int level, int stage, int reviveTimes) {
            _analytics.ForEach(e => e.Adventure_TrackSoftCurrencyReviveByAds(level, stage, reviveTimes));
        }

        public void Adventure_TrackSoftCurrencyReviveByGem(int level, int stage, float valueLock, float valueUnlock, int reviveTimes) {
            _analytics.ForEach(e => e.Adventure_TrackSoftCurrencyReviveByGem(level, stage, valueLock, valueUnlock, reviveTimes));
        }
        
        public void TrackAds(AdsCategory adsCategory, TrackAdsResult result) {
            _analytics.ForEach(e => e.TrackConversion(ConversionType.WatchAds));
            _analytics.ForEach(e => e.TrackAds(adsCategory, result));
        }

        public void TrackAds(string category, TrackAdsResult result) {
            _analytics.ForEach(e => e.TrackConversion(ConversionType.WatchAds));
            _analytics.ForEach(e => e.TrackAds(category, result));
        }
        
        public void TrackInterstitial(AdsCategory adsCategory, TrackAdsResult result) {
            _analytics.ForEach(e => e.TrackConversion(ConversionType.WatchAds));
            _analytics.ForEach(e => e.TrackInterstitial(adsCategory, result));
        }

        public void TrackLuckyWheel(string type, string mode, string[] names, string[] values, string matchResult, int level,
            int stage) {
            _analytics.ForEach(e => e.TrackLuckyWheel(type, mode, names, values, matchResult, level, stage));
        }

        public void DailyGift_TrackCollectItems(int level) {
            foreach (var analytics in _analytics) {
                analytics.DailyGift_TrackCollectItems(level);
            }
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
            foreach (var analytics in _analytics) {
                analytics.ShopBuyCostumeUseGem_TrackSoftCurrency(
                    productName,
                    gemLockSpending,
                    gemSpending,
                    itemReceiving,
                    productType,
                    duration,
                    result);
            }
        }

        public void ShopBuyCostumeUseGold_TrackSoftCurrency(
            string productName,
            float goldSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        ) {
            foreach (var analytics in _analytics) {
                analytics.ShopBuyCostumeUseGold_TrackSoftCurrency(
                    productName,
                    goldSpending,
                    itemReceiving,
                    productType,
                    duration,
                    result);
            }
        }

        #region UPRADE HERO

        public void TrackCreateCrystal(string heroName, int heroAmount,
            int crap, int lesser, int rough, int pure, int perfect,
            int gold) {
            _analytics.ForEach(e => e.TrackCreateCrystal(heroName, heroAmount,
                crap, lesser, rough, pure, perfect,
                gold));
        }

        public void TrackUpgradeCrystal(string rawCrystal, int rawAmount,
            string targetCrystal, int targetAmount,
            int gold, int gemLock, int gemUnlock) {
            _analytics.ForEach(e => e.TrackUpgradeCrystal(rawCrystal, rawAmount,
                targetCrystal, targetAmount,
                gold, gemLock, gemUnlock));
        }

        public void TrackUpgradeBaseIndex(int heroId, string heroName, string index, int level,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
            _analytics.ForEach(e => e.TrackUpgradeBaseIndex(heroId, heroName, index, level,
                lesser, rough, pure, perfect,
                gold, gemLock, gemUnlock));
        }

        public void TrackUpgradeMaxIndex(int heroId, string heroName, string index, int times,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
            _analytics.ForEach(e => e.TrackUpgradeMaxIndex(heroId, heroName, index, times,
                lesser, rough, pure, perfect,
                gold, gemLock, gemUnlock));
        }

        public void PushGameLevel(int levelNo, string levelMode) {
            _analytics.ForEach(e => e.PushGameLevel(levelNo, levelMode));
        }

        public void PopGameLevel(bool winGame) {
            _analytics.ForEach(e => e.PopGameLevel(winGame));
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit) {
            _analytics.ForEach(e => e.LogAdRevenue(mediationNetwork, monetizationNetwork, revenue, currencyCode, format, adUnit));

        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) {
            _analytics.ForEach(e => e.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt));
        }

        #endregion        
    }
}