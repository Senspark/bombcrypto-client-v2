using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using CustomSmartFox.SolCommands;
using CustomSmartFox.SolCommands.Market;

using Data;
using Senspark;
using Game.Dialog.BomberLand.BLFrameShop;
using Newtonsoft.Json;
using Server.Models;
using Services;
using Sfs2X.Entities.Data;
using Utils;

namespace App {
    public partial class NewServerRequester : IServerRequester {
        private readonly ILogManager _logManager;
        private readonly IServerManager _serverManager;
        private readonly IGachaChestItemManager _gachaChestItemManager;
        private readonly IServerDispatcher _serverDispatcher;
        private readonly IChestRewardManager _chestRewardManager;

        public NewServerRequester(
            ILogManager logManager,
            IServerManager serverManager,
            IGachaChestItemManager gachaChestItemManager,
            IServerDispatcher serverDispatcher,
            IChestRewardManager chestRewardManager
        ) {
            _logManager = logManager;
            _serverManager = serverManager;
            _gachaChestItemManager = gachaChestItemManager;
            _serverDispatcher = serverDispatcher;
            _chestRewardManager = chestRewardManager;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }
        
        public async Task<string> GetGachaChests() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetGachaChests(data));
            return result.ToJson();
        }

        public async Task<DateTime> StartOpeningGachaChest(int chestId) {
            var data = new SFSObject();
            data.PutInt("chest_id", chestId);

            var result = await _serverDispatcher.SendCmd(new CmdStartOpeningGachaChest(data));
            var deserializeResult =
                JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<int>>(result.ToJson());
            return DateTime.Now.Add(TimeSpan.FromMilliseconds(deserializeResult.Data));
        }

        public async Task<GachaChestItemData[]> OpenGachaChest(IProductItemManager productItemManager, int chestId) {
            await productItemManager.InitializeAsync();
            
            var data = new SFSObject();
            data.PutInt("chest_id", chestId);
            
            var result = await _serverDispatcher.SendCmd(new CmdOpenGachaChest(data));
            var deserializeResult =
                JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<Dictionary<string, int>[]>>(
                    result.ToJson());
            return deserializeResult.Data.Select(it => new GachaChestItemData(
                it,
                productItemManager
            )).ToArray();
        }

        public async Task<GachaChestShopData[]> GetGachaChestShop() {
            var result = await _serverManager.SendExtensionRequestAsync(new CmdGetGachaChestShop(new SFSObject()));
            var deserializeResult = JsonConvert.DeserializeObject<GetGachaChestShopResult>(result.ToJson());
            if (deserializeResult.Chests == null) {
                deserializeResult.Chests = Array.Empty<GachaChestExtensionData>();
            }
            var data = new List<GachaChestShopData>();
            foreach (var it in deserializeResult.Chests) {
                _gachaChestItemManager.SetItems((ChestShopType) it.ChestType, it.Items);
                var prices = new List<GachaChestPrice>();
                foreach (var iter in it.Prices) {
                    prices.Add(new GachaChestPrice(iter.RewardType, iter.Quantity, iter.Price));
                }
                data.Add(new GachaChestShopData(
                    it.ChestType,
                    it.ItemQuantity,
                    it.Items,
                    prices.ToArray()
                ));
            }
            return data.ToArray();
        }

        public async Task<GachaChestItemData[]> BuyGachaChest(IProductItemManager productItemManager,
            ChestShopType chestType, BlockRewardType rewardType, int quantity) {
            await productItemManager.InitializeAsync();
            
            var data = new SFSObject();
            data.PutInt("chest_type", (int) chestType);
            data.PutInt("reward_type", (int) rewardType);
            data.PutInt("quantity", quantity);
            
            var result = await _serverDispatcher.SendCmd(new CmdBuyGachaChest(data));
            var deserializeResult =
                JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<Dictionary<string, int>[]>>(
                    result.ToJson()
                );
            return deserializeResult.Data.Select(it => new GachaChestItemData(
                it,
                productItemManager
            )).ToArray();
        }

        public async Task<string> GetInventoryItem(int itemId) {
            var data = new SFSObject();
            data.PutInt("type", itemId);
            
            var result = await _serverDispatcher.SendCmd(new CmdGetDashboardMarketplace(data));
            return result.ToJson();
        }

        public async Task<string> GetInventorySellingItem() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetMyItemMarketV3(data));
            return result.ToJson();
        }

        public async Task<string> GetTRHero() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdSyncBomberMan(data));
            return result.ToJson();
        }

        public async Task<string> ChooseTRHero(int heroId) {
            var data = new SFSObject();
            data.PutInt("id", heroId);
            
            var result = await _serverDispatcher.SendCmd(new CmdGetFreeHeroTraditional(data));
            return result.ToJson();
        }

        public async Task<string> GetGemShop() {
            var data = new SFSObject();
            
            var result = await _serverManager.SendExtensionRequestAsync(new CmdGetGemShop(data));
            return result.ToJson();
        }

        public async Task<string> GetGoldShop() {
            var data = new SFSObject();
            
            var result = await _serverManager.SendExtensionRequestAsync(new CmdGetGoldShop(data));
            return result.ToJson();
        }

        public async Task BuyGold(int itemId) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            
            var result = await _serverDispatcher.SendCmd(new CmdBuyGold(data));
        }

        public async Task<string> GetBooster() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetUserPvpBoosters(data));
            return result.ToJson();
        }

        public async Task<string> GetTRHeroes(string type) {
            var data = new SFSObject();
            data.PutUtfString("type", type);
            
            var result = await _serverDispatcher.SendCmd(new CmdGetHeroesTraditional(data));
            return result.ToJson();
        }

        public async Task<string> GetEarlyConfig() {
            var data = new SFSObject();
            
            var result = await _serverManager.SendExtensionRequestAsync(new CmdGetStartGameConfig(data));
            return result.ToJson();
        }

        public async Task<string> GetRankInfo() {
            _logManager.Log();
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetRankInfo(data));
            return result.ToJson();
        }

        public async Task<string> UnlockChestSlot(int slotId) {
            var data = new SFSObject();
            data.PutInt("slotNumber", slotId);
            
            var result = await _serverDispatcher.SendCmd(new CmdBuyGachaChestSlot(data));
            return result.ToJson();
        }

        public async Task<string> GetDailyMission() {
            if(!AppConfig.IsMobile())
                return await Task.FromResult("");

            var data = new SFSObject();
            var result = await _serverDispatcher.SendCmd(new CmdGetDailyMission(data));
            return result.ToJson();

        }

        public async Task<string> WatchDailyMission(string missionCode, string adsToken) {
            var data = new SFSObject();
            data.PutUtfString("mission_code", missionCode);
            data.PutUtfString("ads_token", adsToken);
            
            var result = await _serverDispatcher.SendCmd(new CmdWatchingDailyMissionAds(data));
            return result.ToJson();
        }

        public async Task<string> TakeDailyMission(string missionCode) {
            var data = new SFSObject();
            data.PutUtfString("mission_code", missionCode);
            
            var result = await _serverDispatcher.SendCmd(new CmdTakeDailyMissionReward(data));
            return result.ToJson();
        }

        public async Task<string> GetLuckyWheelReward() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetLuckyWheelReward(data));
            return result.ToJson();
        }

        public async Task<string> GetPvPServerConfig() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetPvpServerConfigs(data));
            return result.ToJson();
        }

        public async Task<string> GetFreeRewardConfig() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetFreeRewardConfigs(data));
            return result.ToJson();
        }

        public async Task<string> GetFreeGem(string adsToken) {
            var data = new SFSObject();
            data.PutUtfString("token", adsToken);
            
            var result = await _serverDispatcher.SendCmd(new CmdGetFreeGems(data));
            return result.ToJson();
        }

        public async Task<string> GetFreeGold(string adsToken) {
            var data = new SFSObject();
            data.PutUtfString("token", adsToken);
            
            var result = await _serverDispatcher.SendCmd(new CmdGetFreeGolds(data));
            return result.ToJson();
        }

        public async Task<string> GetCostumeShop(int itemType) {
            var data = new SFSObject();
            data.PutInt("item_type", itemType);
            
            var result = await _serverDispatcher.SendCmd(new CmdGetCostumeShop(data));
            return result.ToJson();
        }

        public async Task<string> BuyCostumeItem(int itemId, string itemPackage, int quantity) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutUtfString("item_package", itemPackage);
            data.PutInt("quantity", quantity);
            
            var result = await _serverDispatcher.SendCmd(new CmdBuyCostumeItem(data));
            return result.ToJson();
        }

        public async Task<NewcomerGiftData[]> GetNewcomerGift() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetNewcomerGifts(data));
            ParseChestReward(result);
            var deserializeResult = JsonConvert.DeserializeObject<GetNewcomerGiftResult>(result.ToJson());
            return deserializeResult.Items.Select(it => new NewcomerGiftData {
                ItemId = it.ItemId,
                Quantity = it.Quantity
            }).ToArray();
        }

        public async Task<CrystalData[]> GetCrystal() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetCrystals(data));
            var deserializeResult = JsonConvert.DeserializeObject<SendExtensionRequestResult<Crystal[]>>(result.ToJson());
            return deserializeResult.Data.Select(it => new CrystalData {
                ItemId = it.ItemId,
                Quantity = it.Quantity
            }).ToArray();
        }

        public async Task<string> GetSubscriptions() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdSubscriptions(data));
            return result.ToJson();
        }

        public async Task<string> SubscribeSubscription(string productId, string token) {
            var data = new SFSObject();
            data.PutUtfString("product_id", productId);
            data.PutUtfString("token", token);
            
            var result = await _serverDispatcher.SendCmd(new CmdSubscribeSubscription(data));
            return result.ToJson();
        }

        public async Task<string> CancelSubscribeSubscription(string productId) {
            var data = new SFSObject();
            data.PutUtfString("product_id", productId);
            
            var result = await _serverDispatcher.SendCmd(new CmdCancelSubscribeSubscription(data));
            return result.ToJson();
        }

        public async Task<string> GetDailyTaskConfig() {
            var data = new SFSObject();
            var result = await _serverDispatcher.SendCmd(new CmdGetDailyTaskConfig(data));
            return result.ToJson();
        }
        
        public async Task<Dictionary<int, int>> ClaimDailyTask(int id) {
            var data = new SFSObject();
            if (id != 0) {
                data.PutInt("task_id", id);
            }
            var result = await _serverDispatcher.SendCmd(new CmdClaimDailyTask(data));
            ParseChestReward(result);
            return ParseFinalReward(result);
        }
        
        public async Task<List<int>> GetUserDailyProgress() {
            var data = new SFSObject();
            var result = await _serverDispatcher.SendCmd(new CmdGetUserDailyProgress(data));
            var progress = result.GetIntArray("progress").ToList(); ;
            return progress;
        }
        
        private Dictionary<int, int> ParseFinalReward(ISFSObject data) {
            var rewards = new Dictionary<int, int>();
            if (data.ContainsKey("final_reward")) {
                var array = data.GetSFSArray("final_reward");
                for (var i = 0; i < array.Size(); ++i) {
                    var item = array.GetSFSObject(i);
                    var itemId = item.GetInt("item_id");
                    var itemQuantity = item.GetInt("quantity");
                    if (!rewards.TryAdd(itemId, itemQuantity)) {
                        rewards[itemId] += itemQuantity;
                    }
                }
            }
            return rewards;
        }
        
        private void ParseChestReward(ISFSObject data) {
            var rewards = new List<ITokenReward>();
            if (data.ContainsKey("rewards")) {
                var array = data.GetSFSArray("rewards");
                for (var i = 0; i < array.Size(); ++i) {
                    var item = new TokenReward(array.GetSFSObject(i));
                    rewards.Add(item);
                }
                var result = new ChestReward(rewards);
                _chestRewardManager.InitNewChestReward(result);
                _serverDispatcher.DispatchEvent(observer => observer.OnChestReward?.Invoke(result));
            }
        }

        public async Task<(int ItemId, int Quantity)[]> GrindHero(int itemId, int quantity, int status) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutInt("quantity", quantity);
            data.PutInt("status", status);
            
            var result = await _serverDispatcher.SendCmd(new CmdGrindHeroes(data));
            var deserializeResult = JsonConvert.DeserializeObject<SendExtensionRequestResult<Crystal[]>>(result.ToJson());
            return deserializeResult.Data.Select(it => (it.ItemId, it.Quantity)).ToArray();
        }

        public async Task<(int ItemId, int Quantity)> UpgradeCrystal(int itemId, int quantity) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutInt("quantity", quantity);
            
            var result = await _serverDispatcher.SendCmd(new CmdUpgradeCrystal(data));
            var deserializeResult = JsonConvert.DeserializeObject<UpgradeCrystalResult>(result.ToJson());
            return (deserializeResult.ItemId, deserializeResult.Quantity);
        }

        public async Task<ConfigUpgradeData> GetUpgradeConfig() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetUpgradeConfig(data));
            var deserializeResult = JsonConvert.DeserializeObject<ConfigUpgradeData>(result.ToJson());
            return deserializeResult;
        }

        public async Task<(long LastLogout, (int ItemId, int Quantity)[])> GetOfflineReward() {
            var data = new SFSObject();
            
            var result = await _serverDispatcher.SendCmd(new CmdGetOfflineRewards(data));
            var deserializeResult = JsonConvert.DeserializeObject<GetOfflineRewardResult>(result.ToJson());
            return (deserializeResult.LastLogout, deserializeResult.Items.Select(it => (it.ItemId, it.Quantity)).ToArray());
        }

        public Task<((int ItemId, int Quantity)[] Items, int OfflineHours)> ClaimOfflineReward() {
            var data = new SFSObject();
            
            return ClaimOfflineReward(data);
        }

        public Task<((int ItemId, int Quantity)[] Items, int OfflineHours)> ClaimOfflineReward(string adsToken) {
            var data = new SFSObject();
            data.PutUtfString("ads_token", adsToken);
            
            return ClaimOfflineReward(data);
        }

        private async Task<((int ItemId, int Quantity)[] Items, int OfflineHours)> ClaimOfflineReward(
            SFSObject parameters) {
            var result = await _serverDispatcher.SendCmd(new CmdClaimOfflineRewards(parameters));
            var deserializeResult = JsonConvert.DeserializeObject<ClaimOfflineRewardResult>(result.ToJson());
            return (deserializeResult.Items.Select(it => (it.ItemId, it.Quantity)).ToArray(), deserializeResult.OfflineHours);
        }

        public async Task UpgradeTRHero(int heroId, string type) {
            var data = new SFSObject();
            data.PutInt("hero_id", heroId);
            data.PutUtfString("type", type);
            
            var result = await _serverDispatcher.SendCmd(new CmdUpgradeHeroTr(data));
        }

        public async Task ActiveTRHero(int heroId) {
            var data = new SFSObject();
            data.PutInt("hero_id", heroId);
            
            var result = await _serverDispatcher.SendCmd(new CmdActiveHeroTr(data));
        }
        
        public async Task MarkItemViewed(int itemId) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            
            var result = await _serverDispatcher.SendCmd(new CmdMarkItemViewed(data));
        }
    }
}