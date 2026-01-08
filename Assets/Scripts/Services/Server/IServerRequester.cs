using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data;
using Game.Dialog.BomberLand.BLFrameShop;
using Senspark;
using Services;

namespace App {
    [Service(nameof(IServerRequester))]
    public interface IServerRequester : IService {
        Task<string> GetGachaChests();
        Task<DateTime> StartOpeningGachaChest(int chestId);
        Task<GachaChestItemData[]> OpenGachaChest(IProductItemManager productItemManager, int chestId);
        Task<GachaChestShopData[]> GetGachaChestShop();
        Task<GachaChestItemData[]> BuyGachaChest(IProductItemManager productItemManager, ChestShopType chestType,
            BlockRewardType rewardType, int quantity);
        Task<string> GetInventoryItem(int itemId);
        Task<string> GetInventorySellingItem();
        Task<string> GetTRHero();
        Task<string> ChooseTRHero(int heroId);
        Task<string> GetGemShop();
        Task<string> GetGoldShop();
        Task BuyGold(int itemId);
        Task<string> GetBooster();
        Task<string> GetTRHeroes(string type);
        Task<string> GetEarlyConfig();
        Task<string> GetRankInfo();
        Task<string> UnlockChestSlot(int slotId);
        Task<string> GetDailyMission();
        Task<string> WatchDailyMission(string missionCode, string adsToken);
        Task<string> TakeDailyMission(string missionCode);
        Task<string> GetLuckyWheelReward();
        Task<string> GetPvPServerConfig();
        Task<string> GetFreeRewardConfig();
        Task<string> GetFreeGem(string adsToken);
        Task<string> GetFreeGold(string adsToken);
        Task<string> GetCostumeShop(int itemType);
        Task<string> BuyCostumeItem(int itemId, string itemPackage, int quantity);
        Task<NewcomerGiftData[]> GetNewcomerGift();
        Task<CrystalData[]> GetCrystal();
        Task<string> GetSubscriptions();
        Task<string> SubscribeSubscription(string productId, string token);
        Task<string> CancelSubscribeSubscription(string productId);
        Task<string> GetDailyTaskConfig();
        Task<Dictionary<int, int>> ClaimDailyTask(int id);
        Task<List<int>> GetUserDailyProgress();

        /// <summary>
        /// response là số lượng crystal nhận được sau khi grind, không phải tổng  hiện có
        /// </summary>
        Task<(int ItemId, int Quantity)[]> GrindHero(int itemId, int quantity, int status);

        /// <summary>
        /// response là số lượng crystal nhận được sau khi grind, không phải tổng  hiện có
        /// </summary>
        Task<(int ItemId, int Quantity)> UpgradeCrystal(int itemId, int quantity);

        Task<ConfigUpgradeData> GetUpgradeConfig();
        Task<(long LastLogout, (int ItemId, int Quantity)[])> GetOfflineReward();
        Task<((int ItemId, int Quantity)[] Items, int OfflineHours)> ClaimOfflineReward();
        Task<((int ItemId, int Quantity)[] Items, int OfflineHours)> ClaimOfflineReward(string adsToken);

        /// <summary>
        /// TYPE:
        /// DMG,
        /// HP,
        /// RANGE,
        /// SPEED,
        /// BOMB
        /// </summary>
        Task UpgradeTRHero(int heroId, string type);

        /// <summary>
        /// Send server to keep selected hero_id (play in Pvp, Pve) 
        /// </summary>
        /// <param name="heroId"></param>
        /// <returns></returns>
        Task ActiveTRHero(int heroId);

        /// <summary>
        /// mark new item as viewed
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        Task MarkItemViewed(int itemId);
    }
}