using System.Collections.Generic;
using System.Threading.Tasks;

using Analytics.Modules;

using Senspark;

namespace Analytics {
    public enum ConversionType {
        ConnectWalletBnb,
        ConnectWalletPolygon,
        ConnectAccount,
        ConnectAccountBnb,
        ConnectAccountPolygon,
        ConnectGuest,
        ConnectFacebook,
        ConnectApple,
        ConnectTelegram,
        ConnectSolana,
        ChooseBnb,
        ChoosePolygon,
        NewUser,
        FirstOpen,
        BuyHeroFi,
        BuyHeroFiFail,
        BuyHouse,
        BuyHouseFail,
        BuyIap,
        BuyIapFail,
        BuySoftCurrency,
        BuySoftCurrencyFail,
        BuyGachaChest,
        BuyGachaChestFail,
        OpenChestUseGem,
        OpenChestUseGemFail,
        OpenChestWatchAds,
        OpenChest,
        BuyMarketHeroTr,
        BuyMarketHeroTrFail,
        BuyMarketItem,
        BuyMarketItemFail,
        BuyHeroTrFail,
        GetHeroTrFree,
        X2GoldPveWatchAds,
        UsedBoosterBombPve,
        UsedBoosterRangePve,
        UsedBoosterSpeedPve,
        UsedBoosterShieldPve,
        X2GoldPvpWatchAds,
        UsedBoosterBombPvp,
        UsedBoosterRangePvp,
        UsedBoosterSpeedPvp,
        UsedBoosterKeyPvp,
        UsedBoosterShieldPvp,
        UsedBoosterFullConquestCardPvp,
        UsedBoosterFullRankGuardianPvp,
        UsedBoosterConquestCardPvp,
        UsedBoosterRankGuardianPvp,
        ClickSwapGem,
        SwapGem,
        UseAutoMine,
        UserFiClickMarket,
        UserTrClickMarket,
        PlayAdventure,
        UseUpBoosterPvp, // Dùng hết 1 loại booster nào đó
        UseUpBoosterAdventure, // Dùng hết 1 loại booster nào đó
        ClickActiveChest,
        WatchAds,
        RevivePveByAds,
        RevivePveByGem,
        LuckyWheelPveWatchAds,
        LuckyWheelPvpWatchAds,
        OverwriteAccount,
        VisitGemShop,
        ShowStarterPack,
        TapBtnBuyStarterPack,
        ShowPremiumPack,
        TapBtnBuyPremiumPack,
        ShowHeroPack,
        TapBtnBuyHeroPack,
    }

    public enum SceneType {
        VisitMarketplace,
        VisitRanking,
        VisitAdventure,
        VisitPvp,
        VisitTreasureHunt,
        VisitShop,
        VisitChest,
        VisitDailyGift,
        
        VisitHome,
        PlayPve,
        PlayPvp,
        Rating,
        
        WelcomeTutorial,
        SkipTutorialBot,
        PlayTutorialBot,
        ControlGuideTutorial,
        FTUEConversion,
        FullPowerTutorial,
        FeatureGuideTutorial,
        RewardTutorial,
        ChooseLoginScene
    }
    
    public enum AdsCategory {
        Unknown,
        ShortenTimeBronzeChest,
        ShortenTimeSilverChest,
        X2GoldPve,
        PlayButtonPve,
        CompleteMatchPvp,
        CompleteMatchPve,
        QuitMatchPve,
        X2GoldPvp,
        GetGemToAds,
        GetGoldToAds,
        ClaimDailyGiftLevel
    }

    public enum TrackAdsResult {
        Start,
        Cancel,
        Complete,
        Error
    }
    
    public enum TrackResult {
        Error,
        Cancel,
        Done
    }

    public enum TrackPvpMatchResult {
        Win,
        Lose,
        Draw
    }

    public enum TrackPvpLoseReason {
        BlockDrop,
        Prison, 
        Quit
    }

    public enum TrackPvpCollectItemType {
        SkullHead,
        FireUp,
        Boots,
        BombUp,
        Armor,
        Gold1,
        Gold5,
        BronzeChest,
        SilverChest
    }

    public enum TrackPvpRejectChest {
        BronzeChest,
        SilverChest
    }

    public enum MarketPlaceResult {
        BeginSell,
        CancelSell,
        Sold
    }

    /// <summary>
    /// Nguyên tắc sử dụng tracking: chỉ nên tracking trong class xử lý UI
    /// </summary>
    [Service(nameof(IAnalytics))]
    public interface IAnalytics : IService {
        void SetDependencies(IAnalyticsDependency dependency);
        
        #region CONVERSION

        void TrackConversion(ConversionType type);
        void TrackConversionRetention();
        void TrackConversionPvpPlay(int matchCount, int winMatch);
        void TrackConversionAdventurePlay(int level, int stage);
        void TrackConversionActiveUser();

        #endregion

        #region SCENE

        void TrackScene(SceneType type);
        void TrackSceneAndSub(SceneType type, string sub);

        #endregion
        
        #region MISC

        void TrackData(string name, Dictionary<string, object> parameters);
        void TrackRate(int pointRating);
        void TrackClickPlayPvp(int heroId);
        void TrackClickPlayPve(int heroId, int level, int stage);
        void TrackOfflineReward(int timeOffline, string type, string[] names, string[] values);
        void TrackClaimRewardTutorial();
        
        #endregion

        void TrackGetHeroTrFree(string heroName, int heroId);

        #region TREASURE HUNT

        void TreasureHunt_TrackCompleteMap();
        void TreasureHunt_TrackPlayingTime(float seconds);

        #endregion

        #region IAP

        void Iap_TrackBuyIap(string transactionId, string productId, int value, TrackResult result);

        void Iap_TrackBuyGold(int productId, int gemLockSpending, int gemSpending, int goldReceiving, TrackResult result);

        void Iap_TrackGetGoldFree(string transactionId, string productId, int value, TrackResult result);

        void Iap_TrackBuyGachaChest(int productId, string productName, string typeSink, float coinSpending, int chestReceiving,
            string[] items, string[] values, TrackResult result);
        void Iap_TrackSoftCurrencyBuyGachaChest(string typeSink, float coinSpending, TrackResult result);
        void Iap_TrackSoftCurrencyOpenGachaChestByGem(int gemLockSpending, int gemSpending, TrackResult result);
        void Iap_TrackSoftCurrencyBuySlotChestByGem(int gemLockSpending, int gemSpending, TrackResult result);
        void Iap_TrackOpenSwapGem();
        void Iap_TrackSwapGem(float gemSpending, float tokenReceiving, TrackResult result);

        #endregion

        #region PVP

        void Pvp_TrackActiveBooster(string boosterName);

        void Pvp_TrackPlay(int winUserId, int loseUserId, int totalTime, TrackPvpMatchResult result,
            TrackPvpLoseReason reason);

        /// <summary>
        /// Track các item user nhặt được bên trong game play pvp
        /// </summary>
        void Pvp_TrackCollectItems(Dictionary<TrackPvpCollectItemType, int> collected, TrackPvpMatchResult result);

        /// <summary>
        /// Track số lượng các rương mà bị loại bỏ do full inventory
        /// </summary>
        void Pvp_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected);

        void Pve_TrackFullInventory(Dictionary<TrackPvpRejectChest, int> rejected);

        /// <summary>
        /// Track số lượng mà user (myself) bị sập tù
        /// </summary>
        void Pvp_TrackInPrison(int amount);
        void Pvp_TrackSoftCurrencyByWin(float value);
        void Pvp_TrackSoftCurrencyByX2Gold(float value);
        
        #endregion

        #region MARKETPLACE

        void MarketPlace_TrackProduct(string productName, float price, int amount, MarketPlaceResult result);

        void MarketPlace_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            TrackResult result
        );

        void MarketPlace_TrackBuyHeroTr(string heroName, int[] heroIds);

        #endregion

        #region INVENTORY

        void Inventory_TrackOpenChestByGem(
            string chestType,
            int chestId,
            string[] receiveProductName,
            int[] receiveProductQuantity
        );

        void Inventory_TrackEquipItem(int itemType, string itemName, int itemId);
        void Inventory_TrackOpenChestGetHeroTr(string[] heroesNames, int[] heroesIds);

        #endregion

        #region ADVENTURE

        void Adventure_TrackPlay(int heroId, int level, int stage, float totalTime, TrackPvpMatchResult result);
        void Adventure_TrackUseBooster(int level, int stage, string boosterName);

        void Adventure_TrackCollectItems(int level, int stage, Dictionary<TrackPvpCollectItemType, int> collected,
            TrackPvpMatchResult result);

        void Adventure_TrackSoftCurrencyByWin(int level, int stage, float value);
        void Adventure_TrackSoftCurrencyByX2Gold(int level, int stage, float value);
        void Adventure_TrackSoftCurrencyReviveByAds(int level, int stage, int reviveTimes);
        void Adventure_TrackSoftCurrencyReviveByGem(int level, int stage, float valueLock, float valueUnlock, int reviveTimes);
        
        #endregion
        
        #region ADS

        void TrackAds(AdsCategory adsCategory, TrackAdsResult result);
        void TrackAds(string category, TrackAdsResult result);
        void TrackInterstitial(AdsCategory adsCategory, TrackAdsResult result);

        #endregion

        void TrackLuckyWheel(string type, string mode, string[] names, string[] values, string matchResult, int level, int stage);
        void DailyGift_TrackCollectItems(int level);
        
        void ShopBuyCostumeUseGem_TrackSoftCurrency(
            string productName,
            float gemLockSpending,
            float gemSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        );
        
        void ShopBuyCostumeUseGold_TrackSoftCurrency(
            string productName,
            float goldSpending,
            float itemReceiving,
            string productType,
            string duration,
            TrackResult result
        );
        
        #region UPRADE HERO

        void TrackCreateCrystal(string heroName, int heroAmount, 
            int crap, int lesser, int rough, int pure, int perfect,
            int gold);

        void TrackUpgradeCrystal(string rawCrystal, int rawAmount, 
            string targetCrystal, int targetAmount,
            int gold, int gemLock, int gemUnlock);

        void TrackUpgradeBaseIndex(int heroId, string heroName, string index, int level,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock);

        void TrackUpgradeMaxIndex(int heroId, string heroName, string index, int times,
            int lesser, int rough, int pure, int perfect,
            int gold, int gemLock, int gemUnlock);

        #endregion
                
        #region GAME LEVEL
        void PushGameLevel(int levelNo, string levelMode);
        void PopGameLevel(bool winGame);
        
        #endregion 
        
        #region REVENUE
        void LogAdRevenue(
            AdNetwork mediationNetwork,
            string monetizationNetwork,
            double revenue,
            string currencyCode,
            AdFormat format,
            string adUnit
        );

        void LogIapRevenue(
            string eventName,
            string packageName,
            string orderId,
            double priceValue,
            string currencyIso,
            string receipt
        );         

        #endregion
    }
}