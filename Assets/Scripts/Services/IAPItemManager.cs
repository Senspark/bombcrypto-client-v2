using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using App.BomberLand;
using Data;
using Newtonsoft.Json;
using Services.IapAds;
using UnityEngine;
using Utils;
using Task = System.Threading.Tasks.Task;

namespace Services {
    public class IAPItemManager : IIAPItemManager {
        private IAPGemItemData[] _gems;
        private IAPGoldItemData[] _golds;
        private FreeRewardConfig[] _freeRewardConfigs;
        
        private FreeRewardConfig _freeGemRewardConfig;
        private FreeRewardConfig _freeGoldRewardConfig;

        private TaskCompletionSource<bool> _packShopResultTask;
        private IOfferPacksResult _offerPacksResult;
        
        private readonly bool _isProduction;
        private readonly IServerRequester _serverRequester;
        private readonly IServerManager _serverManager;
        private readonly IUnityPurchaseManager _purchaseManager;
        private readonly string OFFER_SHOWED_KEY = $"{nameof(IAPItemManager)}_{nameof(OFFER_SHOWED_KEY)}";

        public IAPItemManager(
            IServerRequester serverRequester,
            IServerManager serverManager,
            IUnityPurchaseManager purchaseManager,
            bool isProduction
        ) {
            _serverRequester = serverRequester;
            _serverManager = serverManager;
            _purchaseManager = purchaseManager;
            _isProduction = isProduction;
        }

        public Task<bool> Initialize() {
            return Task.FromResult(true);
        }

        public void Destroy() {
        }

        public async Task<IAPGemItemData[]> GetGemItemsAsync() {
#if CACHE_GEM_DATA
            if (_gems != null)
                return _gems;
#endif
            var resp = await _serverRequester.GetGemShop(); 
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<IAPGemItemData[]>>(
                resp
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
            _gems = result.Data;
            return _gems;
        }

        public async Task<IAPGoldItemData[]> GetGoldItemsAsync() {
            if (_golds != null) {
                return _golds;
            }
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<IAPGoldItemData[]>>(
                await _serverRequester.GetGoldShop()
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
            _golds = result.Data;
            return _golds;
        }
        
        public async Task<FreeRewardConfig[]> GetFreeRewardConfigAsync() {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<FreeRewardConfig[]>>(
                await _serverRequester.GetFreeRewardConfig()
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
            _freeRewardConfigs = result.Data;
            var dic = _freeRewardConfigs.ToDictionary(it => it.RewardType);
            _freeGemRewardConfig = dic[RewardUtils.ConvertToBlockRewardType(BlockRewardType.LockedGem)];
            _freeGoldRewardConfig = dic[RewardUtils.ConvertToBlockRewardType(BlockRewardType.BLGold)];
            return _freeRewardConfigs;
        }

        public async Task GetFreeGemAsync(string adsToken) {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(
                await _serverRequester.GetFreeGem(adsToken)
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
        }

        public async Task GetFreeGoldAsync(string adsToken) {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(
                await _serverRequester.GetFreeGold(adsToken)
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
        }

        public FreeRewardConfig GetFreeGemRewardConfigs() {
            return _freeGemRewardConfig;
        }

        public FreeRewardConfig GetFreeGoldRewardConfigs() {
            return _freeGoldRewardConfig;
        }

        public async Task SyncOfferShops() {
            if (_packShopResultTask == null) {
                _packShopResultTask = new TaskCompletionSource<bool>();
                try {
                    var d = await _serverManager.BomberLand.GetOfferPacks();
                    _offerPacksResult = d;
                    _packShopResultTask.SetResult(true);
                } catch (Exception e) {
                    _packShopResultTask.SetResult(false);
                }
            }
            await _packShopResultTask.Task;
            _packShopResultTask = null;
        }

        public bool CanAutoShowOffer(IOfferPacksResult.OfferType type) {
            if (!CanBuyOffer(type)) {
                return false;
            }
            var nextShow = (IOfferPacksResult.OfferType)PlayerPrefs.GetInt(OFFER_SHOWED_KEY, 0);
            if (nextShow <= type) {
                PlayerPrefs.SetInt(OFFER_SHOWED_KEY, (int)type + 1);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        public bool CanBuyOffer(IOfferPacksResult.OfferType type) {
            var offer = _offerPacksResult?.Offers.FirstOrDefault(e => e.Type == type);
            if (offer == null) {
                return false;
            }
            return !offer.IsExpired;
        }

        public IOfferPacksResult.IOffer GetOfferData(IOfferPacksResult.OfferType type) {
            return CanBuyOffer(type) ? _offerPacksResult.Offers.FirstOrDefault(e => e.Type == type) : null;
        }

        public IOfferPacksResult.IOffer GetAnyOffer() {
            return _offerPacksResult?.Offers.FirstOrDefault();
        }

        public IOfferPacksResult.IOffer GetAnyPurchasableOffer() {
            return _offerPacksResult?.Offers.FirstOrDefault(e => !e.IsExpired);
        }

        // Subscription
        public async Task<IAPSubscriptionItemData[]> GetSubscriptions() {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult<IAPSubscriptionItemData[]>>(
                await _serverRequester.GetSubscriptions()
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
            return result.Data;
        }

        public async Task SubscribeSubscription(string productId, string token) {
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(
                await _serverRequester.SubscribeSubscription(productId, token)
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
        }

        public async Task CancelSubscribeSubscription(string productId) {
#if UNITY_IOS
            Application.OpenURL("https://apps.apple.com/account/billing");
            return;
#endif
            var result = JsonConvert.DeserializeObject<EcEsSendExtensionRequestResult>(
                await _serverRequester.CancelSubscribeSubscription(productId)
            );
            if (result.Code != 0) {
                throw new Exception(result.Message);
            }
        }

        public async Task<PurchaseResult> BuyIap(string productId, Func<Task<bool>> onBeforeConsume) {
            var buyTask = await _purchaseManager.PurchaseItem(productId);
            if (buyTask.State == PurchaseState.Done) {
                var packageName = AppConfig.GetPackageName();
                var serverTask = GetValidateIapTask(productId);
                var result = await serverTask(new ValidateIapRequest(buyTask.TransactionId, buyTask.ProductId,
                    buyTask.PurchaseToken, packageName));
                if (result == ValidateIapResult.Success) {
                    // Cái này để test mua thất bại dẫn tới ko consume
                    if (!_isProduction && onBeforeConsume != null) {
                        var canContinue = await onBeforeConsume();
                        if (!canContinue) {
                            buyTask.State = PurchaseState.Error;
                            return buyTask;
                        }    
                    }
                    
                    // done
                    _purchaseManager.ConsumePurchaseItem(productId);
                    return buyTask;
                }
            } else if (buyTask.State == PurchaseState.Cancel) {
                // cancel
                return buyTask;
            }
            // error
            buyTask.State = PurchaseState.Error;
            return buyTask;
        }

        public bool HasAnyPendingPurchases() {
            return _purchaseManager.GetPendingTransactions().Count > 0;
        }

        public async Task<List<PurchaseResult>> RestoreAllPendingPurchases() {
            // Get pending transaction from IUnityPurchaseManager
            // Send request to server
            // If True or Existed, then call IUnityPurchaseManager.ConfirmPendingTransaction
            var pending = _purchaseManager.GetPendingTransactions();
            var packageName = AppConfig.GetPackageName();
            var response = new List<PurchaseResult>();
            foreach (var p in pending) {
                var requestTask = GetValidateIapTask(p.ProductId);
                var result =
                    await requestTask(
                        new ValidateIapRequest(p.TransactionId, p.ProductId, p.PurchaseToken, packageName));
                var t = result switch {
                    ValidateIapResult.Success => PurchaseState.Done,
                    ValidateIapResult.Existed => PurchaseState.Done,
                    _ => PurchaseState.Error
                };
                if (t == PurchaseState.Done) {
                    _purchaseManager.ConsumePurchaseItem(p.ProductId);
                }
                response.Add(new PurchaseResult(t, p.TransactionId, p.PurchaseToken, p.OrderId, p.ProductId, p.Receipt));
            }
            return response;
        }

        private Func<ValidateIapRequest, Task<ValidateIapResult>> GetValidateIapTask(string productName) {
            if (IapConfig.GemPackIds.Contains(productName)) {
                return _serverManager.BomberLand.ValidateBuyIapGem;
            }
            if (IapConfig.OfferPackIds.Contains(productName)) {
                return _serverManager.BomberLand.ValidateBuyIapOffer;
            }
            throw new Exception("Not supported");
        }
    }
}