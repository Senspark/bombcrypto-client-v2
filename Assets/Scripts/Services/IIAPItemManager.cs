using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using App;

using Data;

using Senspark;

using Services.IapAds;

namespace Services {
    [Service(nameof(IIAPItemManager))]
    public interface IIAPItemManager : IService {
        Task<IAPGemItemData[]> GetGemItemsAsync();
        Task<IAPGoldItemData[]> GetGoldItemsAsync();
        Task<FreeRewardConfig[]> GetFreeRewardConfigAsync();
        Task GetFreeGemAsync(string adsToken);
        Task GetFreeGoldAsync(string adsToken);
        FreeRewardConfig GetFreeGemRewardConfigs();
        FreeRewardConfig GetFreeGoldRewardConfigs();
        
        // Pack shop
        Task SyncOfferShops();
        bool CanAutoShowOffer(IOfferPacksResult.OfferType type);
        bool CanBuyOffer(IOfferPacksResult.OfferType type);
        IOfferPacksResult.IOffer GetOfferData(IOfferPacksResult.OfferType type);
        IOfferPacksResult.IOffer GetAnyOffer();
        IOfferPacksResult.IOffer GetAnyPurchasableOffer();
        
        //subscription
        Task<IAPSubscriptionItemData[]> GetSubscriptions();
        Task SubscribeSubscription(string productId, string token);
        Task CancelSubscribeSubscription(string productId);

        Task<PurchaseResult> BuyIap(string productId, Func<Task<bool>> onBeforeConsume = null);
        bool HasAnyPendingPurchases();
        Task<List<PurchaseResult>> RestoreAllPendingPurchases();
    }
}