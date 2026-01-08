using System.Collections.Generic;
using System.Threading.Tasks;

using Manager;

using Senspark;

using UnityEngine;

namespace Analytics {
    public class CostCenterAnalytics : IAnalytics {
        
        private readonly IAnalyticsBridge _bridge;

        public CostCenterAnalytics(bool production, ILogManager logManager, IRevenueValidator revenueValidator) {
            var enableLog = production;
            var platform = Application.platform;
            _bridge = platform switch {
                RuntimePlatform.Android or RuntimePlatform.IPhonePlayer => new MobileCostCenterAnalyticsBridge(revenueValidator),
                _ => new NullAnalyticsBridge(logManager, nameof(BaseFirebaseAnalytics))
            };
        }
        
        public Task<bool> Initialize() {
            return _bridge.Initialize();
        }

        public void Destroy() { }

        public void PushGameLevel(int levelNo, string levelMode) {
            _bridge.PushGameLevel(levelNo, levelMode);
        }

        public void PopGameLevel(bool winGame) {
            _bridge.PopGameLevel(winGame);
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit) {
            _bridge.LogAdRevenue(mediationNetwork, monetizationNetwork, revenue, currencyCode, format, adUnit, null);
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) {
            _bridge.LogIapRevenue(eventName, packageName, orderId, priceValue, currencyIso, receipt);
        }
        
        
        //----------------------------------------------------------------//
        #region Not Used in CostCenter
        public void SetDependencies(IAnalyticsDependency dependency) {
        }

        public void TrackConversion(ConversionType type) {
        }

        public void TrackConversionRetention() {
        }

        public void TrackConversionPvpPlay(int matchCount, int winMatch) {
        }

        public void TrackConversionAdventurePlay(int level, int stage) {
        }

        public void TrackConversionActiveUser() {
        }

        public void TrackScene(SceneType type) {
        }

        public void TrackSceneAndSub(SceneType type, string sub) {
        }

        public void TrackData(string name, Dictionary<string, object> parameters) {
        }

        public void TrackData(IAnalyticsEvent ev) {
        }

        public void TrackRate(int pointRating) {
        }

        public void TrackClickPlayPvp(int heroId) {
            PushGameLevel(0, "pvp");
        }

        public void TrackClickPlayPve(int heroId, int level, int stage) {
            PushGameLevel(level, "adventure");
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

        public void Iap_TrackBuyGachaChest(int productId, string productName, string typeSink, float coinSpending, int chestReceiving,
            string[] items, string[] values, TrackResult result) {
        }

        public void Iap_TrackSoftCurrencyBuyGachaChest(string typeSink, float coinSpending, TrackResult result) {
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

        public void Pvp_TrackPlay(int winUserId, int loseUserId, int totalTime, TrackPvpMatchResult result, TrackPvpLoseReason reason) {
            PopGameLevel(result == TrackPvpMatchResult.Win);
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

        public void MarketPlace_TrackSoftCurrency(string productName, float gemLockSpending, float gemSpending, float itemReceiving,
            TrackResult result) {
        }

        public void MarketPlace_TrackBuyHeroTr(string heroName, int[] heroIds) {
        }

        public void Inventory_TrackOpenChestByGem(string chestType, int chestId, string[] receiveProductName,
            int[] receiveProductQuantity) {
        }

        public void Inventory_TrackEquipItem(int itemType, string itemName, int itemId) {
        }

        public void Inventory_TrackOpenChestGetHeroTr(string[] heroesNames, int[] heroesIds) {
        }

        public void Adventure_TrackPlay(int heroId, int level, int stage, float totalTime, TrackPvpMatchResult result) {
            _bridge.PopGameLevel(result == TrackPvpMatchResult.Win);
        }

        public void Adventure_TrackUseBooster(int level, int stage, string boosterName) {
        }

        public void Adventure_TrackCollectItems(int level, int stage, Dictionary<TrackPvpCollectItemType, int> collected, TrackPvpMatchResult result) {
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

        public void ShopBuyCostumeUseGem_TrackSoftCurrency(string productName, float gemLockSpending, float gemSpending,
            float itemReceiving, string productType, string duration, TrackResult result) {
        }

        public void ShopBuyCostumeUseGold_TrackSoftCurrency(string productName, float goldSpending, float itemReceiving,
            string productType, string duration, TrackResult result) {
        }

        public void TrackCreateCrystal(string heroName, int heroAmount, int crap, int lesser, int rough, int pure, int perfect,
            int gold) {
        }

        public void TrackUpgradeCrystal(string rawCrystal, int rawAmount, string targetCrystal, int targetAmount, int gold,
            int gemLock, int gemUnlock) {
        }

        public void TrackUpgradeBaseIndex(int heroId, string heroName, string index, int level, int lesser, int rough, int pure,
            int perfect, int gold, int gemLock, int gemUnlock) {
        }

        public void TrackUpgradeMaxIndex(int heroId, string heroName, string index, int times, int lesser, int rough, int pure,
            int perfect, int gold, int gemLock, int gemUnlock) {
        }

        #endregion
    }
}
