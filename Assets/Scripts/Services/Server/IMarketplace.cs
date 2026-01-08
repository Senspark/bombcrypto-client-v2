using System.Threading.Tasks;

using App;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

namespace Services.Server {
    public interface IMarketplace : IServerManagerDelegate {
        Task GetMarketConfig();
        ItemMarketConfig GetItemConfigByItemId(int itemId);
        Task<OrderDataResponse> OrderItemMarket(int itemId, int quantity, int expiration);
        Task CancelOrderItemMarket();
        Task BuyItemMarket();
        Task SellItemMarket(int itemId, float price, int quantity, int itemType, int expirationAfter);
        Task EditItemMarket(int itemId, int itemType,float oldPrice, float newPrice, int newQuantity, int oldQuantity, int expirationAfter);
        Task CancelItemMarket(int itemId, float price, int itemType, int expirationAfter);
        Task<float> GetCurrentUserMinPrice(int itemId);
        Task RefreshMinPrice();
        int MinPriceRefreshTime { get; }
        Task<int[]> BuyAsync(ProductData product, int quantity);
        Task<int[]> BuyAsync(ProductHeroData product, int quantity);

        Task<(ProductData[] Products, int Quantity)> GetProductAsync(
            int productType,
            int start,
            int length,
            int sort = 0
        );
        Task<(ProductHeroData[] Products, int Quantity)> GetProductHeroAsync(int start, int length, int sort = 0);
    }
}