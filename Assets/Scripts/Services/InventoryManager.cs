using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using App;

using Constant;

using Data;

using Senspark;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Services.Server.Exceptions;

using Utils;

namespace Services {
    public class InventoryManager : IInventoryManager {
        public struct InventorySlotChestData {
            [JsonProperty("slotType")]
            public string SlotType;

            [JsonProperty("isOwner")]
            public bool IsOwner;
            
            [JsonProperty("price")]
            public int Price;
            
            [JsonProperty("slotNumber")]
            public int SlotNumber;

            [JsonProperty("chest")]
            public GetInventoryChestData Chest;
        }

        public struct GetInventoryChestData {
            [JsonProperty("chest_id")]
            public int ChestId;

            [JsonProperty("chest_type")]
            public int ChestType;

            [JsonProperty("remaining_time")]
            public long RemainingTime;

            [JsonProperty("total_open_time")]
            public long TotalOpenTime;
            
            [JsonProperty("skip_time_per_ads")]
            public long SkipTimePerAds;
            
            [JsonProperty("skip_open_time_gem_require")]
            public int SkipOpenTimeGemRequire;
            
        }

        private struct GetInventoryItemData {
            [JsonProperty("id")]
            public int Id;
            
            [JsonProperty("item_id")]
            public int ItemId;

            [JsonProperty("item_type")]
            public int ItemType;

            [JsonProperty("quantity")]
            public int Quantity;

            [JsonProperty("status")]
            public int Status;

            [JsonProperty("create_date")]
            public long CreateDate;
            
            [JsonProperty("equip_status")]
            public int EquipStatus;
            
            [JsonProperty("expirationAfter")]
            public long? ExpirationAfter;
            
            [JsonProperty("is_new")]
            public bool IsNew;

            [JsonProperty("expiry_date")]
            public long? Expired;
            
            [JsonProperty("type")]
            public string Type;
        }

        private struct SendExtensionRequestResult<T> {
            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("data")]
            public T[] Data;

            [JsonProperty("data_locked")]
            public T[] LockedData;

            [JsonProperty("es")]
            public string Message;
        }

        private readonly IGachaChestItemManager _chestItemManager;
        private readonly IGachaChestNameManager _chestNameManager;
        private IHeroAbilityManager _heroAbilityManager;
        private IHeroColorManager _heroColorManager;
        private IHeroStatsManager _heroStatsManager;
        private IEnumerable<InventoryHeroData> _heroes;
        private readonly ILogManager _logManager;
        private IProductManager _productManager;
        private readonly IServerRequester _serverRequester;
        private readonly IUserAccountManager _userAccountManager;

        public InventoryManager(
            IGachaChestItemManager chestItemManager,
            IGachaChestNameManager chestNameManager,
            ILogManager logManager,
            IServerRequester serverRequester,
            IUserAccountManager userAccountManager
        ) {
            _chestItemManager = chestItemManager;
            _chestNameManager = chestNameManager;
            _logManager = logManager;
            _serverRequester = serverRequester;
            _userAccountManager = userAccountManager;
        }

        public void Destroy() {
        }

        public Task<bool> Initialize() {
            _heroes = null;
            return Task.FromResult(true);
        }

        public void Clear() {
            _heroes = null;
        }

        public async Task<IEnumerable<InventoryChestData>> GetChestAsync() {
            return ResultOf<InventorySlotChestData>(await _serverRequester.GetGachaChests()).Select(it =>
                new InventoryChestData(it, _chestNameManager, _chestItemManager));
        }

        public async Task UnlockChestSlotAsync(int slotId) {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(
                await _serverRequester.UnlockChestSlot(slotId)
            );
            if (result.Code != 0) {
                throw new Exception($"{result.Code}: {result.Message}");
            }
        }

        public async Task<IEnumerable<InventoryHeroData>> GetHeroesAsync() {
            // Fix: not cache heroes data 
            // if (_heroes != null)
            //     return _heroes;
            var account = _userAccountManager.GetRememberedAccount() ?? throw new Exception("Account is null");
            _heroAbilityManager ??= ServiceLocator.Instance.Resolve<IHeroAbilityManager>();
            _heroColorManager ??= ServiceLocator.Instance.Resolve<IHeroColorManager>();
            var heroIdManager = ServiceLocator.Instance.Resolve<IHeroIdManager>();
            _heroStatsManager ??= ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            _productManager ??= ServiceLocator.Instance.Resolve<IProductManager>();
            await _heroStatsManager.InitializeAsync();
            var result = await _serverRequester.GetInventoryItem((int) InventoryItemType.Hero);
            var jResult = JToken.Parse(result);
            _heroes = jResult.Contains("data_locked")
                ? LockedResultOf<GetInventoryItemData>(result).Select(it => {
                    var heroId = heroIdManager.GetHeroId(it.Data.ItemId);
                    return new InventoryHeroData(
                        _heroAbilityManager.GetAbilities(heroId),
                        _heroColorManager.GetColor(heroId),
                        it.Data.ItemId,
                        _productManager.GetProduct(it.Data.ItemId).ProductName,
                        it.Data.ItemType,
                        it.Data.Quantity,
                        account.isUserFi && !it.Locked,
                        Array.Empty<HeroSkinData>(),
                        _heroStatsManager.GetStats(it.Data.ItemId),
                        it.Data.IsNew
                    );
                })
                : ResultOf<GetInventoryItemData>(result).Select(it => {
                    var heroId = heroIdManager.GetHeroId(it.ItemId);
                    return new InventoryHeroData(
                        _heroAbilityManager.GetAbilities(heroId),
                        _heroColorManager.GetColor(heroId),
                        it.ItemId,
                        _productManager.GetProduct(it.ItemId).ProductName,
                        it.ItemType,
                        it.Quantity,
                        account.isUserFi && it.Status == 0,
                        Array.Empty<HeroSkinData>(),
                        _heroStatsManager.GetStats(it.ItemId),
                        it.IsNew
                    );
                });
            return _heroes;
        }

        public async Task<IEnumerable<InventoryItemData>> GetItemsAsync(int itemType) {
            var account = ServiceLocator.Instance.Resolve<IUserAccountManager>().GetRememberedAccount() ??
                          throw new Exception("Account is null");
            var productManager = ServiceLocator.Instance.Resolve<IProductManager>();
            var productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            await productItemManager.InitializeAsync();
            var result = await _serverRequester.GetInventoryItem(itemType);
            var jResult = JToken.Parse(result);
            var expirationAfterDefault = (long)ServiceLocator.Instance.Resolve<IItemUseDurationManager>().GetDuration().TotalMilliseconds;
            return jResult.Contains("data_locked")
                ? LockedResultOf<GetInventoryItemData>(result).Select(it =>
                    new InventoryItemData(
                        productItemManager.GetItem(it.Data.ItemId).Abilities,
                        it.Data.Id,
                        it.Data.ItemId,
                        productManager.GetProduct(it.Data.ItemId).ProductName,
                        it.Data.ItemType,
                        it.Data.Quantity,
                        account.isUserFi && !it.Locked,
                        it.Data.CreateDate,
                        it.Data.ExpirationAfter??-1,
                        it.Data.EquipStatus == 1,
                        it.Data.IsNew,
                        it.Data.Type,
                        false
                    ))
                : ResultOf<GetInventoryItemData>(result).Select(it =>
                    new InventoryItemData(
                        productItemManager.GetItem(it.ItemId).Abilities,
                        it.Id,
                        it.ItemId,
                        productManager.GetProduct(it.ItemId).ProductName,
                        it.ItemType,
                        it.Quantity,
                        account.isUserFi && it.Status == 0,
                        it.CreateDate,
                        it.ExpirationAfter??expirationAfterDefault,
                        it.EquipStatus == 1,
                        it.IsNew,
                        it.Type,
                        false
                    ));
        }

        public async Task<IEnumerable<InventorySellingItemData>> GetSellingItemsAsync() {
            return ResultOf<InventorySellingItemData>(await _serverRequester.GetInventorySellingItem());
        }

        // private static IEnumerable<(T Data, bool Locked)> AutoResultOf<T>(string result) {
        //     var jResult = JToken.Parse(result);
        //     return jResult.Contains("data_locked") ? LockedResultOf<T>(result) : ResultOf<T>(result);
        // }

        private static IEnumerable<(T Data, bool Locked)> LockedResultOf<T>(string result) {
            return LockedResultOf(JsonConvert.DeserializeObject<SendExtensionRequestResult<T>>(result));
        }

        private static IEnumerable<(T Data, bool Locked)> LockedResultOf<T>(SendExtensionRequestResult<T> result) {
            if (result.Code == 0) {
                return new[] {
                    result.Data.Select(it => (it, false)),
                    result.LockedData.Select(it => (it, true))
                }.SelectMany(it => it);
            }
            throw new ErrorCodeException(result.Code, result.Message);
        }

        private static IEnumerable<T> ResultOf<T>(string result) {
            var deserializeResult = JsonConvert.DeserializeObject<SendExtensionRequestResult<T>>(result);
            if (deserializeResult.Code == 0) {
                return deserializeResult.Data;
            }

            var message = deserializeResult.Message;
            throw new ErrorCodeException(deserializeResult.Code,  message ?? $"Error code: {deserializeResult.Code}");
        }
        
        public async Task<int> GetCurrentAvatarTR() {
            var list = new List<InventoryItemData>();
            list.AddRange(await GetItemsAsync((int)InventoryItemType.AvatarTR));
            foreach (var item in list.Where(item => item.Equipped)) {
                return item.ItemId;
            }
            return -1;
        }
    }
}