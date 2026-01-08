using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using App.BomberLand;

using Constant;
using CustomSmartFox.SolCommands;
using CustomSmartFox.SolCommands.Market;

using Cysharp.Threading.Tasks;

using Data;
using Senspark;
using Newtonsoft.Json;

using Sfs2X.Entities.Data;

using UnityEngine;

namespace Services.Server {
    public class Marketplace : IMarketplace {
        private readonly IServerDispatcher _serverDispatcher;
        private IProductItemManager _productItemManager;
        private readonly IGeneralServerBridge _generalServerBridge;
        private readonly ILogManager _logManager;
        private readonly Dictionary<int, ItemMarketConfig> _itemMarketConfig = new();
        private readonly Dictionary<int, float> _minPriceUser = new();
        private readonly List<ProductData> _products = new();
        private const int DefaultMinPriceRefresh = 60;
        private int _refreshMinPrice = DefaultMinPriceRefresh;

        public int MinPriceRefreshTime => _refreshMinPrice;

        public Marketplace(
            ILogManager logManager, 
            IServerDispatcher serverDispatcher,
            IGeneralServerBridge generalServerBridge
        ) {
            _logManager = logManager;
            _serverDispatcher = serverDispatcher;
            _generalServerBridge = generalServerBridge;
        }

        public async Task<(ProductData[] Products, int Quantity)> GetProductAsync(
            int productType,
            int start,
            int length,
            int sort
        ) {
            if(_products.Count == 0) {
                await LoadProductItem();
            }
            return _products.Where(it=> it.Type == productType).ToArray() is { Length: > 0 } products
                ? (products, products.Length)
                : (Array.Empty<ProductData>(), 0);
        }
        
        public async Task GetMarketConfig() {
            if (_itemMarketConfig.Count > 0) {
                return;
            }
            var response = await _serverDispatcher.SendCmd(new CmdGetMarketConfigV3(new SFSObject()));
            var dataArray = response.GetSFSArray("data");

            _itemMarketConfig.Clear();
            for (int i = 0; i < dataArray.Size(); i++)
            {
                var itemData = dataArray.GetSFSObject(i);
                var config = new ItemMarketConfig(itemData);
                _itemMarketConfig[config.ItemId] = config;
            }
            
            _refreshMinPrice = response.GetInt("refresh_min_price_client");
            if(_refreshMinPrice <= 0) {
                _refreshMinPrice = DefaultMinPriceRefresh;
            }
        }

        public ItemMarketConfig GetItemConfigByItemId(int itemId) {
            return _itemMarketConfig.GetValueOrDefault(itemId);
        }

        public async Task<OrderDataResponse> OrderItemMarket(int itemId, int quantity, int expiration) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutInt("quantity", quantity);
            data.PutInt("expiration", expiration);
            var response = await _serverDispatcher.SendCmd(new CmdOrderItemMarketV3(data));
            return new OrderDataResponse(response, itemId, expiration);
        }

        public async Task CancelOrderItemMarket() {
            await _serverDispatcher.SendCmd(new CmdCancelOrderItemMarketV3(new SFSObject()));
        }

        public async Task BuyItemMarket() {
            var response = await _serverDispatcher.SendCmd(new CmdBuyItemMarketV3(new SFSObject()));
            _generalServerBridge.UpdateUserReward(response);
        }

        public async Task SellItemMarket(int itemId, float price, int quantity, int itemType, int expirationAfter) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutInt("quantity", quantity);
            data.PutFloat("price", price);
            data.PutInt("item_type", itemType);
            data.PutInt("expiration", expirationAfter);
            
            await _serverDispatcher.SendCmd(new CmdSellItemMarketV3(data));
        }
        
        public async Task EditItemMarket(int itemId, int itemType, float oldPrice, float newPrice, int newQuantity, int oldQuantity, int expirationAfter) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutInt("item_type", itemType);
            data.PutFloat("old_price", oldPrice);
            data.PutFloat("new_price", newPrice);
            data.PutInt("new_quantity", newQuantity);
            data.PutInt("old_quantity", oldQuantity);
            data.PutInt("expiration", expirationAfter);
            
            var response = await _serverDispatcher.SendCmd(new CmdEditItemMarketV3(data));
            _generalServerBridge.UpdateUserReward(response);
        }
        
        public async Task CancelItemMarket(int itemId, float price, int itemType, int expirationAfter) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutFloat("price", price);
            data.PutInt("item_type", itemType);
            data.PutInt("expiration", expirationAfter);
            
            await _serverDispatcher.SendCmd(new CmdCancelItemMarketV3(data));
        }

        public async Task<float> GetCurrentUserMinPrice(int itemId) {
            if(_minPriceUser.Count == 0) {
                await RefreshMinPrice();
            }
            _minPriceUser.TryGetValue(itemId, out float minPrice);
            if(_minPriceUser == null || _minPriceUser.Count == 0) {
                Debug.LogError("GetCurrentUserMinPrice: No min price data");
                return 1;
            }
            return minPrice;
        }

        public async Task RefreshMinPrice() {
            var response = await _serverDispatcher.SendCmd(new CmdGetCurrentMinPriceMarketV3(new SFSObject()));
            var dataArray = response.GetSFSArray("data");
            _minPriceUser.Clear();
            for (int i = 0; i < dataArray.Size(); i++)
            {
                var itemData = dataArray.GetSFSObject(i);
                var itemId = itemData.GetInt("item_id");
                var minPrice = itemData.GetFloat("min_price");
                _minPriceUser[itemId] = minPrice;
            }
        }
        
        public Task<int[]> BuyAsync(ProductData product, int quantity) {
            return BuyAsync(product.ItemId, quantity, product.UnitPrice, product.ExpirationAfter);
        }

        public Task<int[]> BuyAsync(ProductHeroData product, int quantity) {
            return BuyAsync(product.DataBase.ItemId, quantity, product.DataBase.UnitPrice, product.DataBase.ExpirationAfter);
        }

        private async Task<int[]> BuyAsync(int itemId, int quantity, float unitPrice, int expirationAfter) {
            var data = new SFSObject();
            data.PutInt("item_id", itemId);
            data.PutInt("quantity", quantity);
            data.PutFloat("unit_price", unitPrice);
            data.PutInt("expiration_after", expirationAfter);
            
            var response = await _serverDispatcher.SendCmd(new CmdBuyItemMarketplace(data));
            return response.GetIntArray("list_id");
        }
        
        public async Task<(ProductData[] Products, int Quantity)> GetProductAsync(InventoryItemType productType, int start,
            int length, int sort) {
            return await GetProductAsync((int) productType, start, length, sort);
        }
        
        public async Task<(ProductHeroData[] Products, int Quantity)> GetProductHeroAsync(
            int start,
            int length,
            int sort = 0
        ) {
            var heroAbilityManager = ServiceLocator.Instance.Resolve<IHeroAbilityManager>();
            var heroColorManager = ServiceLocator.Instance.Resolve<IHeroColorManager>();
            var heroIdManager = ServiceLocator.Instance.Resolve<IHeroIdManager>();
            var heroStatsManager = ServiceLocator.Instance.Resolve<IHeroStatsManager>();
            var (products, quantity) = await GetProductAsync(InventoryItemType.Hero, start, length, sort);
            return (products.Select(it => {
                var heroId = heroIdManager.GetHeroId(it.ItemId);
                return new ProductHeroData(
                    it,
                    heroAbilityManager.GetAbilities(heroId),
                    heroColorManager.GetColor(heroId),
                    heroId,
                    heroStatsManager.GetStats(it.ItemId)
                );
            }).ToArray(), quantity);
        }
        
        private async UniTask LoadProductItem() {
            _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
            await _productItemManager.InitializeAsync();

            foreach (var (itemId, config) in _itemMarketConfig) {
                var itemInfo = await _productItemManager.GetItemAsync(itemId);
                _products.Add(new ProductData {
                    Abilities = itemInfo.Abilities,
                    Description = itemInfo.Description,
                    ItemId = itemId,
                    Price = new PriceData(itemInfo.ItemType, 0), // not used
                    ProductId = itemId,
                    ProductName = itemInfo.Name,
                    ProductType = itemInfo.ItemType,
                    Type = itemInfo.ItemType,
                    Quantity = 0,
                    UnitPrice = 0,
                    ExpirationAfter = 0,
                    ItemConfig = config
                });
            }
        }

        private struct GetProductResult<T> {
            [JsonProperty("ec")]
            public int Code;

            [JsonProperty("data")]
            public T Data;

            [JsonProperty("es")]
            public string Message;

            [JsonProperty("total_count")]
            public int Quantity;
        }
    }
    public class ItemMarketConfig {
        public readonly int ItemId;
        public readonly bool IsNoExpiredItem;
        public readonly float MinPrice;

        public ItemMarketConfig(ISFSObject data)
        {
            ItemId = data.GetInt("item_id");
            MinPrice = (float)data.GetDouble("min_price");
            IsNoExpiredItem = data.GetBool("is_no_expired_item");
        }
    }
    
    public class OrderDataResponse
    {
        public readonly bool IsSuccess;
        public readonly int TotalQuantity;
        public readonly int TotalPrice;
        public readonly int ItemId;
        public readonly ExpirationMarketType Type;

        public OrderDataResponse(ISFSObject data, int itemId, long expiration)
        {
            IsSuccess = data.GetBool("success");
            TotalQuantity = data.GetInt("total_quantity");
            TotalPrice = data.GetInt("total_price");
            ItemId = itemId;

            if (expiration == 0)
            {
                Type = ExpirationMarketType.NoExpiration;
            }
            else if (expiration == 60 * 60 * 24 * 7)
            {
                Type = ExpirationMarketType.Expiration7Days;
            }
            else if (expiration == 60 * 60 * 24 * 30)
            {
                Type = ExpirationMarketType.Expiration30Days;
            }
            else
            {
                Type = ExpirationMarketType.NoExpiration;
            }
        }
    }
}
public enum ExpirationMarketType {
    NoExpiration = 0,
    Expiration7Days = 1,
    Expiration30Days = 2
}