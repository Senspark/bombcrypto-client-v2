using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

using Senspark;

namespace Analytics {
    public class NullAnalytics : IAnalytics {
        private readonly ILogManager _logManager;

        public NullAnalytics(ILogManager logManager) {
            _logManager = logManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public void SetDependencies(IAnalyticsDependency dependency) {
        }

        public void TrackConversion(ConversionType type) {
            _logManager.Log(nameof(type));
        }

        public void TrackConversionRetention() {
            _logManager.Log();
        }

        public void TrackConversionPvpPlay(int matchCount, int winMatch) {
            _logManager.Log();
        }

        public void TrackConversionAdventurePlay(int level, int stage) {
            _logManager.Log($"{level} {stage}");
        }

        public void TrackConversionActiveUser() {
            _logManager.Log();
        }
        
        public void TrackScene(SceneType type) {
            _logManager.Log(nameof(type));
        }

        public void TrackSceneAndSub(SceneType type, string sub) {
            _logManager.Log(nameof(type));
        }

        public void TrackData(string name, Dictionary<string, object> parameters) {
            _logManager.Log(name);
        }

        public void TrackRate(int pointRating) {
            _logManager.Log($"rate with {pointRating} points");
        }

        public void TrackClickPlayPvp(int heroId) {
            _logManager.Log($"click play pvp {heroId}");
        }

        public void TrackClickPlayPve(int heroId, int level, int stage) {
            _logManager.Log($"click play pve {heroId} {level} {stage}");
        }

        public void TrackOfflineReward(int timeOffline, string type, string[] names, string[] values) {
            _logManager.Log($"offline reward {type} {names} {values}");
        }

        public void TrackClaimRewardTutorial() {
            _logManager.Log("track claim reward tutorial");
        }

        public void TrackGetHeroTrFree(string heroName, int heroId) {
            _logManager.Log($"{heroName} {heroId}");
        }

        public void TreasureHunt_TrackCompleteMap() {
            _logManager.Log();
        }

        public void TreasureHunt_TrackPlayingTime(float seconds) {
            _logManager.Log(seconds.ToString(CultureInfo.InvariantCulture));
        }

        public void Iap_TrackBuyIap(string transactionId, string productId, int value, TrackResult result) {
            _logManager.Log($"{transactionId} {productId} {value} {nameof(result)}");
        }

        public void Iap_TrackBuyGold(int productId, int gemLockSpending, int gemSpending, int goldReceiving, TrackResult result) {
            _logManager.Log($"{gemSpending} {goldReceiving} {nameof(result)}");
        }
        public void Iap_TrackGetGoldFree(string transactionId, string productId, int value, TrackResult result) {
            _logManager.Log($"{transactionId} {productId} {value} {nameof(result)}");
        }

        public void Iap_TrackBuyGachaChest(int productId, string productName, string sinkType, float coinSpending, int chestReceiving,
            string[] items, string[] values, TrackResult result) {
            _logManager.Log($"{productId} {productName} {coinSpending} {chestReceiving} {nameof(result)}");
        }

        public void Iap_TrackSoftCurrencyBuyGachaChest(string sinkType, float coinSpending, TrackResult result) {
            _logManager.Log($"{nameof(result)}");
        }

        public void Iap_TrackSoftCurrencyOpenGachaChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
            _logManager.Log($"{nameof(gemSpending)} {result}");
        }

        public void Iap_TrackSoftCurrencyBuySlotChestByGem(int gemLockSpending, int gemSpending, TrackResult result) {
            _logManager.Log($"{nameof(gemSpending)} {result}");
        }
        
        public void Iap_TrackOpenSwapGem() {
            _logManager.Log();
        }

        public void Iap_TrackSwapGem(float gemSpending, float tokenReceiving, TrackResult result) {
            _logManager.Log($"{gemSpending} {tokenReceiving} {nameof(result)}");
        }

        public void Pvp_TrackActiveBooster(string boosterName) {
            _logManager.Log($"{boosterName}");
        }

        public void Pvp_TrackPlay(int winUserId, int loseUserId, int totalTime, TrackPvpMatchResult result,
            TrackPvpLoseReason reason) {
            _logManager.Log($"{winUserId} {loseUserId} {totalTime} {result} {reason}");
        }

        public void Pvp_TrackCollectItems(Dictionary<TrackPvpCollectItemType, int> collected,
            TrackPvpMatchResult result) {
            var str = new StringBuilder();
            foreach (var kv in collected) {
                str.Append($"{kv.Key.ToString()}={kv.Value}, ");
            }
            _logManager.Log($"{str} {result}");
        }

        public void Pvp_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
            var str = new StringBuilder();
            foreach (var kv in rejected) {
                str.Append($"{kv.Key.ToString()}={kv.Value}, ");
            }
            _logManager.Log($"{rejected}");
        }

        public void Pve_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected) {
            var str = new StringBuilder();
            foreach (var kv in rejected) {
                str.Append($"{kv.Key.ToString()}={kv.Value}, ");
            }
            _logManager.Log($"{rejected}");
        }
        
        public void Pvp_TrackInPrison(int amount) {
            _logManager.Log($"{amount}");
        }

        public void Pvp_TrackSoftCurrencyByWin(float value) {
            _logManager.Log($"{value}");
        }

        public void Pvp_TrackSoftCurrencyByX2Gold(float value) {
            _logManager.Log($"{value}");
        }
        
        public void MarketPlace_TrackProduct(string productName, float price, int amount, MarketPlaceResult result) {
            _logManager.Log($"{productName} {price} {result}");
        }

        public void MarketPlace_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            TrackResult result
        ) {
            _logManager.Log($"{gemSpending} {itemReceiving} {result}");
        }

        public void MarketPlace_TrackBuyHeroTr(string heroName, int[] heroIds) {
            var str = string.Join("-", heroIds);
            _logManager.Log($"{heroName} {str}");
        }

        public void Inventory_TrackOpenChestByGem(
            string chestType,
            int chestId,
            string[] receiveProductName,
            int[] receiveProductQuantity
        ) {
            _logManager.Log($"{chestType} {chestId} {receiveProductName}");
        }

        public void Inventory_TrackEquipItem(int itemType, string itemName, int itemId) {
            _logManager.Log($"{itemType} {itemName} {itemId}");
        }

        public void Inventory_TrackOpenChestGetHeroTr(string[] heroesNames, int[] heroesIds) {
            var str1 = string.Join("-", heroesNames);
            var str2 = string.Join("-", heroesIds);
            _logManager.Log($"{str1} {str2}");
        }

        public void Adventure_TrackPlay(int heroId, int level, int stage, float totalTime, TrackPvpMatchResult result) {
            _logManager.Log($"{heroId} {level} {stage} {totalTime} {result}");
        }

        public void Adventure_TrackUseBooster(int level, int stage, string boosterName) {
            _logManager.Log($"{level} {stage} {boosterName}");
        }
        
        public void Adventure_TrackCollectItems(int level, int stage,
            Dictionary<TrackPvpCollectItemType, int> collected, TrackPvpMatchResult result) {
            var str = new StringBuilder();
            foreach (var kv in collected) {
                str.Append($"{kv.Key.ToString()}={kv.Value}, ");
            }
            _logManager.Log($"{level} {stage} {str} {result}");
        }
        
        public void Adventure_TrackSoftCurrencyByWin(int level, int stage, float value) {
            _logManager.Log($"Soft Currency By Win {level} {stage} {value}");
        }

        public void Adventure_TrackSoftCurrencyByX2Gold(int level, int stage, float value) {
            _logManager.Log($"Soft Currency By X2 Gold {level} {stage} {value}");
        }
        
        public void Adventure_TrackSoftCurrencyReviveByAds(int level, int stage, int reviveTimes) {
            _logManager.Log($"Soft Currency Revive By Ads {level} {stage} {reviveTimes}");
        }

        public void Adventure_TrackSoftCurrencyReviveByGem(int level, int stage, float valueLock, float valueUnlock, int reviveTimes) {
            _logManager.Log($"Soft Currency Revive By Gem {level} {stage} {valueLock}-{valueUnlock} {reviveTimes}");
        }

        public void TrackAds(AdsCategory adsCategory, TrackAdsResult result) {
            _logManager.Log($"{adsCategory} {result}");
        }

        public void TrackAds(string category, TrackAdsResult result) {
            _logManager.Log($"{category} {result}");
        }
        
        public void TrackInterstitial(AdsCategory adsCategory, TrackAdsResult result) {
            _logManager.Log($"{adsCategory} {result}");
        }

        public void TrackLuckyWheel(string type, string mode, string[] names, string[] values, string matchResult, int level,
            int stage) {
            var str1 = string.Join("-", names);
            var str2 = string.Join("-", values);
            _logManager.Log($"{type} {mode} {str1} {str2} {matchResult} {level} {stage}");
        }

        public void DailyGift_TrackCollectItems(int level) {
            _logManager.Log($"{level}");
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
            _logManager.Log($"ShopBuyCostumeUseGem_TrackSoftCurrency {productName}");
        }

        public void ShopBuyCostumeUseGold_TrackSoftCurrency(
            string productName,
            float goldSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        ) {
            _logManager.Log($"ShopBuyCostumeUseGold_TrackSoftCurrency {productName}");
        }
        
        #region UPRADE HERO

        public void TrackCreateCrystal(string heroName, int heroAmount,
            int crap, int lesser, int rough, int pure, int perfect,
            int gold) {
            _logManager.Log($"create_crystal {heroName}");
        }

        public void TrackUpgradeCrystal(string rawCrystal, int rawAmount,
            string targetCrystal, int targetAmount,
            int gold, int gemLock, int gemUnlock) {
            _logManager.Log($"upgrade_crystal {rawAmount} - {targetCrystal}");
        }

        public void TrackUpgradeBaseIndex(int heroId, string heroName, string index, int level,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
            _logManager.Log($"upgrade_base_index {heroId} {index}");
        }

        public void TrackUpgradeMaxIndex(int heroId, string heroName, string index, int times,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock) {
            _logManager.Log($"upgrade_max_index {heroId} {index}");
        }

        public void PushGameLevel(int levelNo, string levelMode) {
            _logManager.Log($"push_game_level level:{levelNo} mode:{levelMode}");
        }

        public void PopGameLevel(bool winGame) {
            _logManager.Log($"pop_game_level result:{winGame}");
        }

        public void LogAdRevenue(AdNetwork mediationNetwork, string monetizationNetwork, double revenue, string currencyCode,
            AdFormat format, string adUnit) {
            _logManager.Log($"log_ad_revenue");
        }

        public void LogIapRevenue(string eventName, string packageName, string orderId, double priceValue, string currencyIso,
            string receipt) {
            _logManager.Log($"log_iap_revenue");
        }

        #endregion               
    }
}