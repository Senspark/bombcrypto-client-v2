using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics;
using App;
using Constant;
using Cysharp.Threading.Tasks;
using Data;
using DG.Tweening;
using Game.Dialog.BomberLand.BLFrameShop;
using Game.Manager;
using Reconnect;
using Reconnect.Backend;
using Reconnect.View;
using Scenes.MainMenuScene.Scripts;
using Scenes.StoryModeScene.Scripts;
using Senspark;
using Services;
using Services.IapAds;
using Services.Server.Exceptions;
using Sfs2X.Entities.Data;
using Share.Scripts.Dialog;
using Share.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Logger = Utils.Logger;

namespace Scenes.ShopScene.Scripts {
    public class ShopScene : MonoBehaviour {
        public static void LoadScene(TypeMenuLeftShop typeMenuPrefer = TypeMenuLeftShop.Chest) {
            void OnLoaded(GameObject obj) {
                var shopScene = obj.GetComponent<ShopScene>();
                shopScene.TypeMenuPrefer = typeMenuPrefer;
            }
            const string sceneName = "ShopScene";
            SceneLoader.LoadSceneAsync(sceneName, OnLoaded).Forget();
        }

        private static bool _isUnityPurchaseManagerError = false;

        [SerializeField]
        public BLFrameShop frameShop;

        [SerializeField]
        private Canvas canvasDialog;

        [SerializeField]
        private Image splashFade;

        private TypeMenuLeftShop TypeMenuPrefer { get; set; } = TypeMenuLeftShop.Chest;

        private IServerManager _serverManager;
        private IServerRequester _serverRequester;
        private ISoundManager _soundManager;
        private IIAPItemManager _itemManager;
        private IUnityPurchaseManager _unityPurchaseManager;
        private IAnalytics _analytics;
        private IFeatureManager _featureManager;
        private IProductManager _productManager;
        private ILogManager _logManager;
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;
        private IUnityAdsManager _adsManager;
        private IShopManager _shopManager;
        private IProductItemManager _productItemManager;
        private IEarlyConfigManager _earlyConfigManager;
        private IInventoryManager _inventoryManager;
        private IReconnectStrategy _reconnectStrategy;
        private IInputManager _inputManager;

        private WaitingUiManager _waiting;
        
        private void Awake() {
            splashFade.gameObject.SetActive(true);
            _waiting = new WaitingUiManager(canvasDialog);
            _waiting.Begin();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _itemManager = ServiceLocator.Instance.Resolve<IIAPItemManager>();
            _unityPurchaseManager = ServiceLocator.Instance.Resolve<IUnityPurchaseManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _featureManager = ServiceLocator.Instance.Resolve<IFeatureManager>();
            _productManager = ServiceLocator.Instance.Resolve<IProductManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _adsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _shopManager = ServiceLocator.Instance.Resolve<IShopManager>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _earlyConfigManager = ServiceLocator.Instance.Resolve<IEarlyConfigManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _inputManager = ServiceLocator.Instance.Resolve<IInputManager>();
            _reconnectStrategy = new DefaultReconnectStrategy(
                _logManager,
                new MainReconnectBackend(),
                LoadSceneReconnectView.ToCurrentScene(canvasDialog)
            );
            _reconnectStrategy.Start();
        }
        
        private void OnDestroy() {
            _reconnectStrategy.Dispose();
        }

        private async void Start() {
            await _serverManager.WaitForUserInitialized();
            await frameShop.InitUi();
            if (!_featureManager.EnableShopGem) {
                frameShop.UiHideGem();
                if (TypeMenuPrefer == TypeMenuLeftShop.Gems) {
                    TypeMenuPrefer = TypeMenuLeftShop.Chest;
                }
            }
            if (!_featureManager.EnableShopSwapGem || AppConfig.IsAirDrop()) {
                frameShop.UiHideSwapGem();
            }
            await _serverManager.General.GetChestReward();
            frameShop.SetSelectTab(TypeMenuPrefer);
            frameShop.SetTrackTabMenuClick(TrackTabMenuClick);
            if (TypeMenuPrefer == TypeMenuLeftShop.Hero) {
                await InitSubSegmentHero();
                await WebGLTaskDelay.Instance.Delay(500);
            }
            if (TypeMenuPrefer == TypeMenuLeftShop.Costume) {
                await InitSubSegmentCostume();
                await WebGLTaskDelay.Instance.Delay(500);
                frameShop.ForceScrollPageAutoLayoutVertical();
            }
            splashFade.DOFade(0.0f, 0.3f).OnComplete(() => { splashFade.gameObject.SetActive(false); });
            _analytics.TrackSceneAndSub(SceneType.VisitShop, TypeMenuPrefer.ToString().ToLower());

            _waiting.End();
            await InitData();
        }

        private async Task InitData() {
            try {
                await InitSubSegmentChest();
                await _itemManager.GetFreeRewardConfigAsync();
                await InitSubSegmentGem();
                await InitSubSegmentGold();
                await InitSubSegmentSwapGem();
                if (TypeMenuPrefer != TypeMenuLeftShop.Hero) {
                    await InitSubSegmentHero();
                }
                if (TypeMenuPrefer != TypeMenuLeftShop.Costume) {
                    await InitSubSegmentCostume();
                }
                await InitSubSegmentEmoji();
                await InitSubSegmentAvatar();
            } catch (Exception e) {
                if (e is ErrorCodeException) {
                    DialogError.ShowError(canvasDialog, e.Message);    
                } else {
                    DialogOK.ShowError(canvasDialog, e.Message);
                }
            } 
        }
        
        private async Task InitSubSegmentChest() {
            try {
                var chestData = await _serverRequester.GetGachaChestShop();
                if (chestData == null) {
                    DialogOK.ShowError(canvasDialog, "Chest Data is undefined!");
                    return;
                }
                chestData = RemoveChestWithoutPrices(chestData);
                if (chestData.Length > 0) {
                    var (chestDataTr, remain) = ChestDataForUserTr(chestData);
                    var chestDataP2P = remain;
                    if (!_featureManager.EnableShopChest) {
                        chestDataP2P = Array.Empty<GachaChestShopData>();
                    }
                    frameShop.UiInitChest(canvasDialog, chestDataP2P, chestDataTr, RequestBuyChest);
                } else {
                    frameShop.UiHideChest();
                }
            } catch (Exception e) {
                frameShop.UiHideChest();
                DialogOK.ShowError(canvasDialog, "Load Chest error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        private static GachaChestShopData[] RemoveChestWithoutPrices(GachaChestShopData[] chestData) {
            var chestList = new List<GachaChestShopData>();
            foreach (var ichest in chestData) {
                if (ichest.Prices is { Length: > 0 }) {
                    chestList.Add(ichest);
                }
            }
            return chestList.ToArray();
        }

        private static (GachaChestShopData[], GachaChestShopData[]) ChestDataForUserTr(GachaChestShopData[] chestData) {
            var chestList = new List<GachaChestShopData>();
            var remainList = new List<GachaChestShopData>();
            foreach (var ichest in chestData) {
                var priceList = new List<GachaChestPrice>();
                foreach (var iprice in ichest.Prices) {
                    if (iprice.RewardType != BlockRewardType.BLCoin) {
                        priceList.Add(iprice);
                    }
                }
                if (priceList.Count > 0) {
                    var chest = new GachaChestShopData((int) ichest.ChestType, ichest.ItemQuantity, ichest.Items,
                        priceList.ToArray());
                    chestList.Add(chest);
                } else {
                    remainList.Add(ichest);
                }
            }
            return (chestList.ToArray(), remainList.ToArray());
        }

        private async Task InitSubSegmentGem() {
            if (!_featureManager.EnableShopGem) {
                frameShop.UiHideGem();
                return;
            }
            try {
                var gemData = await _itemManager.GetGemItemsAsync();
                Array.Sort(gemData, (g1, g2) => g1.GemReceive - g2.GemReceive);
                if (!_isUnityPurchaseManagerError) {
                    try {
                        // IAP
                        await _unityPurchaseManager.SyncData();
                        foreach (var gem in gemData) {
                            gem.ItemPrice = _unityPurchaseManager.GetItemPrice(gem.ProductId).FullString;
                        }
                    } catch (Exception e) {
                        Debug.LogWarning(e.Message);
                        _isUnityPurchaseManagerError = true;
                    }
                }
                frameShop.UiInitGemData(
                    _itemManager,
                    gemData,
                    RequestBuyGem,
                    RequestGetFreeGem);
            } catch (Exception e) {
                frameShop.UiHideGem();
                DialogOK.ShowError(canvasDialog, "Load Gem error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        private void ReloadSubSegmentGem(IAPGemItemData[] gemData) {
            try {
                try {
                    Array.Sort(gemData, (g1, g2) => g1.GemReceive - g2.GemReceive);
                    // IAP
                    foreach (var gem in gemData) {
                        gem.ItemPrice = _unityPurchaseManager.GetItemPrice(gem.ProductId).FullString;
                    }
                    frameShop.UiInitGemData(
                        _itemManager,
                        gemData,
                        RequestBuyGem,
                        RequestGetFreeGem);
                } catch (Exception e) {
                    Debug.LogWarning(e.Message);
                }
            } catch (Exception e) {
                frameShop.UiHideGem();
                DialogOK.ShowError(canvasDialog, "Load Gem error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        private async Task InitSubSegmentGold() {
            try {
                // Gold
                var goldData = await _itemManager.GetGoldItemsAsync();
                Array.Sort(goldData, (g1, g2) => g1.Price - g2.Price);
                frameShop.UiInitGoldData(_itemManager, goldData, RequestBuyGold, RequestGetFreeGold);
            } catch (Exception e) {
                frameShop.UiHideTab(TypeMenuLeftShop.Gold);
                DialogOK.ShowError(canvasDialog, "Load Gold error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        private async Task InitSubSegmentSwapGem() {
            try {
                // SwapGem
                if (_featureManager.EnableShopSwapGem) {
                    await frameShop.UiInitSwapGem(canvasDialog);
                } else {
                    frameShop.UiHideSwapGem();
                }
            } catch (Exception e) {
                frameShop.UiHideSwapGem();
                DialogOK.ShowError(canvasDialog, "LoadSwapGem error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }
        
        private async Task InitSubSegmentHero() {
            try {
                await frameShop.UiInitHero(canvasDialog, _shopManager, _productItemManager, _earlyConfigManager,
                    RequestBuyCostume);
            } catch (Exception e) {
                frameShop.UiHideTab(TypeMenuLeftShop.Hero);
                DialogOK.ShowError(canvasDialog, "Load Hero error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }
        
        private async Task InitSubSegmentCostume() {
            try {
                await frameShop.UiInitCostume(canvasDialog, _shopManager, _productItemManager, _earlyConfigManager,
                    RequestBuyCostume);
            } catch (Exception e) {
                frameShop.UiHideTab(TypeMenuLeftShop.Costume);
                DialogOK.ShowError(canvasDialog, "Load Costume error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        private async Task InitSubSegmentEmoji() {
            try {
                await frameShop.UiInitEmoji(canvasDialog, _shopManager, _productItemManager,
                    RequestBuyEmoji);
            } catch (Exception e) {
                frameShop.UiHideTab(TypeMenuLeftShop.Emoji);
                DialogOK.ShowError(canvasDialog, "Load Emoji error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }
        
        private async Task InitSubSegmentAvatar() {
            try {
                await frameShop.UiInitAvatar(canvasDialog, _shopManager, _productItemManager,
                    RequestBuyAvatar);
            } catch (Exception e) {
                frameShop.UiHideTab(TypeMenuLeftShop.Avatar);
                DialogOK.ShowError(canvasDialog, "Load Avatar error: " + e.Message);
                Debug.LogWarning(e.StackTrace);
            }
        }

        private async void RequestBuyChest(GachaChestShopData dataChest, GachaChestPrice price, int quantity) {
            float bLCoin;
            if (price.RewardType == BlockRewardType.Gem) {
                bLCoin = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
                bLCoin += _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            } else {
                bLCoin = _chestRewardManager.GetChestReward(price.RewardType);
            }
            if (bLCoin < price.Price * quantity) {
                switch (price.RewardType) {
                    case BlockRewardType.Gem:
                        ShowDialogNotEnoughGem();
                        break;
                    case BlockRewardType.BLGold:
                        ShowDialogNotEnoughGold();
                        break;
                    default:
                        DialogOK.ShowInfo(canvasDialog, _languageManager.GetValue(LocalizeKey.ui_not_enough_coin));
                        break;
                }
                return;
            }

            // Call Buy Chest
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin(0);

            var confirm = await DialogConfirm.Create();
            var confirmDes = "Do you want to open Chest?";
            if (quantity > 1) {
                confirmDes = $"Do you want to open {quantity} Chest?";
            }
            confirm.SetInfo(
                confirmDes,
                "Yes",
                "No",
                () => UniTask.Void(async () => {
                    try {
                        var itemsReward = await _serverRequester.BuyGachaChest(_productItemManager, dataChest.ChestType,
                            price.RewardType, price.Quantity * quantity);
                        TrackBuyGachaChest(dataChest, price, itemsReward, TrackResult.Done);
                        await _serverManager.General.GetChestReward();
                        var dialog = await BLDialogGachaChest.CreateByPrefab(canvasDialog, itemsReward, dataChest);
                        dialog.Show(canvasDialog);
                        waiting.End();
                        ClearCacheDataInventory();
                    } catch (Exception e) {
                        TrackBuyGachaChest(dataChest, price, null, TrackResult.Error);
                        waiting.End();
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(canvasDialog, e.Message);    
                        } else {
                            DialogOK.ShowError(canvasDialog, e.Message);
                        }
                    } finally {
                        waiting.End();
                    }
                }),
                () => waiting.End()
            );
            confirm.Show(canvasDialog);
        }

        private void RequestBuyGem(IAPGemItemData dataGem) {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            
            var data = new SFSObject();
            data.PutUtfString("Name Iap pack", dataGem.ItemName);
            data.PutUtfString("Amount", dataGem.ItemPrice);
            data.PutFloat("Gem receive", dataGem.GemReceive);
            data.PutFloat("Gem bonus", dataGem.GemsBonus);
            #if UNITY_IOS
            data.PutUtfString("Platform", "IOS");
            #else
            data.PutUtfString("Platform", "Android");
            #endif
            
            UniTask.Void(async () => {
                try {
                    var restored = await IapHelper.TryRestore(_itemManager, _serverManager, _analytics, canvasDialog);
                    if (restored) {
                        return;
                    }
                    var result = await _itemManager.BuyIap(dataGem.ProductId,
                        () => IapHelper.OnBeforeConsume(canvasDialog));
                    TrackBuyGem(dataGem, result);
                    switch (result.State) {
                        case PurchaseState.Done: {
                            _serverManager.ClearCache(SFSDefine.SFSCommand.GET_GEM_SHOP_V2);
                            await _serverManager.General.GetChestReward();
                            var gemData = await _itemManager.GetGemItemsAsync();
                            ReloadSubSegmentGem(gemData);
                            _serverManager.General.SendMessageSlack(":dollar: User Buy IAP :white_check_mark:", data);
                            break;
                        }
                        case PurchaseState.Cancel:
                            throw new Exception();
                            break;
                        case PurchaseState.Error:
                            throw new Exception();
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                } catch (Exception e) {
                    _serverManager.General.SendMessageSlack(":dollar: User Buy IAP :x:", data);
                    var message = $"Purchase failed. Please try to buy again.";
                    DialogOK.ShowError(canvasDialog, message);
                } finally {
                    waiting.End();
                }
            });
        }

        private async void RequestGetFreeGem() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            try {
                _analytics.TrackAds(AdsCategory.GetGemToAds, TrackAdsResult.Start);
                var adsToken = await _adsManager.ShowRewarded();
                await _itemManager.GetFreeGemAsync(adsToken);
                await _itemManager.GetFreeRewardConfigAsync();
                var dataGemFree = _itemManager.GetFreeGemRewardConfigs();
                frameShop.SlotFreeGem.SetData(dataGemFree);
                frameShop.ShopGemInfo.SetData(dataGemFree);
                await _serverManager.General.GetChestReward();
                DialogOK.ShowInfo(canvasDialog, "Successfully");
                _analytics.TrackAds(AdsCategory.GetGemToAds, TrackAdsResult.Complete);
                _analytics.Iap_TrackBuyIap(adsToken, "buy_FreeGem_gem", dataGemFree.QuantityPerView, TrackResult.Done);
            } catch (Exception e) {
                if (e is AdException adException) {
                    var adsResult = adException.Result switch {
                        Services.IapAds.AdResult.Cancel => TrackAdsResult.Cancel,
                        _ => TrackAdsResult.Error
                    };
                    _analytics.TrackAds(AdsCategory.GetGemToAds, adsResult);
                }
                _logManager.Log($"Can't get free reward");
                DialogOK.ShowError(canvasDialog, e.Message);
            } finally {
                waiting.End();
            }
        }

        private async void RequestGetFreeGold() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            try {
                _analytics.TrackAds(AdsCategory.GetGoldToAds, TrackAdsResult.Start);
                var adsToken = await _adsManager.ShowRewarded();
                await _itemManager.GetFreeGoldAsync(adsToken);
                await _itemManager.GetFreeRewardConfigAsync();
                var dataGoldFree = _itemManager.GetFreeGoldRewardConfigs();
                frameShop.SlotFreeGold.SetData(dataGoldFree);
                frameShop.ShopGoldInfo.SetData(dataGoldFree);
                await _serverManager.General.GetChestReward();
                DialogOK.ShowInfo(canvasDialog, "Successfully");
                _analytics.TrackAds(AdsCategory.GetGoldToAds, TrackAdsResult.Complete);
                _analytics.Iap_TrackGetGoldFree(adsToken, "FreeGold", dataGoldFree.QuantityPerView, TrackResult.Done);
            } catch (Exception e) {
                if (e is AdException adException) {
                    var adsResult = adException.Result switch {
                        Services.IapAds.AdResult.Cancel => TrackAdsResult.Cancel,
                        _ => TrackAdsResult.Error
                    };
                    _analytics.TrackAds(AdsCategory.GetGoldToAds, adsResult);
                }
                _logManager.Log($"Can't get free reward");
                DialogOK.ShowError(canvasDialog, e.Message);
            } finally {
                waiting.End();
            }
        }

        private async void RequestBuyCostume(CostumeData costumeData, CostumeData.PriceData priceData,
            ProductItemData itemData, int quantity) {
            var rewardType = RewardUtils.ConvertToBlockRewardType(priceData.RewardType);
            float lockGemSpend = 0;
            float gemSpend = 0;
            if (rewardType == BlockRewardType.Gem || rewardType == BlockRewardType.LockedGem) {
                var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
                var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
                var bLGem = gemLock + gemUnlock;
                if (bLGem < (priceData.Price * quantity)) {
                    // DialogOK.ShowInfo(canvasDialog, _languageManager.GetValue(LocalizeKey.ui_not_enough_gem));
                    ShowDialogNotEnoughGem();
                    return;
                }
                var (lockGem, gem) = GetGemSpending((int) gemLock, (int) gemUnlock, priceData.Price * quantity);
                lockGemSpend = lockGem;
                gemSpend = gem;
            } else {
                var gold = _chestRewardManager.GetChestReward(rewardType);
                if (gold < (priceData.Price * quantity)) {
                    ShowDialogNotEnoughGold();
                    return;
                }
            }
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin(0);
            var confirm = await DialogConfirm.Create();
            confirm.SetInfo(
                "Do you want to buy?",
                "Yes",
                "No",
                () => UniTask.Void(async () => {
                    try {
                        await _shopManager.BuyCostumeAsync(costumeData.ItemId, priceData.Package, quantity);
                        await _serverManager.General.GetChestReward();
                        // DialogOK.ShowInfo(this.canvasDialog, "Successfully");
                        LuckyWheelReward.GetDialogLuckyReward(1).ContinueWith(dialogReward => {
                            dialogReward.UpdateUI(costumeData.ItemId, quantity);
                            dialogReward.Show(canvasDialog);
                        });
                        ClearCacheDataInventory();
                        TrackBuyCostume(costumeData, priceData, itemData, lockGemSpend, gemSpend, quantity);
                    } catch (Exception e) {
                        Logger.LogEditorError(e);
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(canvasDialog, e.Message);
                        } else {
                            DialogOK.ShowError(canvasDialog, e.Message);
                        }
                    } finally {
                        waiting.End();
                    }
                }),
                () => waiting.End()
            );
            confirm.Show(canvasDialog);
        }

        private async void RequestBuyEmoji(CostumeData costumeData, CostumeData.PriceData priceData,
            ProductItemData itemData, int quantity) {
            var rewardType = RewardUtils.ConvertToBlockRewardType(priceData.RewardType);
            float lockGemSpend = 0;
            float gemSpend = 0;
            if (rewardType == BlockRewardType.Gem || rewardType == BlockRewardType.LockedGem) {
                var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
                var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
                var bLGem = gemLock + gemUnlock;
                if (bLGem < (priceData.Price * quantity)) {
                    // DialogOK.ShowInfo(canvasDialog, _languageManager.GetValue(LocalizeKey.ui_not_enough_gem));
                    ShowDialogNotEnoughGem();
                    return;
                }
                var (lockGem, gem) = GetGemSpending((int)gemLock, (int)gemUnlock, priceData.Price * quantity);
                lockGemSpend = lockGem;
                gemSpend = gem;
            } else {
                var gold = _chestRewardManager.GetChestReward(rewardType);
                if (gold < (priceData.Price * quantity)) {
                    ShowDialogNotEnoughGold();
                    return;
                }
            }
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin(0);
            var confirm = await DialogConfirm.Create();
            confirm.SetInfo(
                "Do you want to buy?",
                "Yes",
                "No",
                () => UniTask.Void(async () => {
                    try {
                        await _shopManager.BuyCostumeAsync(costumeData.ItemId, priceData.Package, quantity);
                        // Load lại emoji để loại bỏ item đã mua
                        InitSubSegmentEmoji();
                        await _serverManager.General.GetChestReward();
                        // DialogOK.ShowInfo(this.canvasDialog, "Successfully");
                        LuckyWheelReward.GetDialogLuckyReward(1).ContinueWith(dialogReward => {
                            dialogReward.UpdateUI(costumeData.ItemId, quantity);
                            dialogReward.Show(canvasDialog);
                        });
                        ClearCacheDataInventory();
                        TrackBuyCostume(costumeData, priceData, itemData, lockGemSpend, gemSpend, quantity);
                    } catch (Exception e) {
                        Logger.LogEditorError(e);
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(canvasDialog, e.Message);
                        } else {
                            DialogOK.ShowError(canvasDialog, e.Message);
                        }
                    } finally {
                        waiting.End();
                    }
                }),
                () => waiting.End()
            );
            confirm.Show(canvasDialog);
        }
        
        private async void RequestBuyAvatar(CostumeData costumeData, CostumeData.PriceData priceData,
            ProductItemData itemData, int quantity) {
            var rewardType = RewardUtils.ConvertToBlockRewardType(priceData.RewardType);
            float lockGemSpend = 0;
            float gemSpend = 0;
            if (rewardType == BlockRewardType.Gem || rewardType == BlockRewardType.LockedGem) {
                var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
                var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
                var bLGem = gemLock + gemUnlock;
                if (bLGem < (priceData.Price * quantity)) {
                    // DialogOK.ShowInfo(canvasDialog, _languageManager.GetValue(LocalizeKey.ui_not_enough_gem));
                    ShowDialogNotEnoughGem();
                    return;
                }
                var (lockGem, gem) = GetGemSpending((int)gemLock, (int)gemUnlock, priceData.Price * quantity);
                lockGemSpend = lockGem;
                gemSpend = gem;
            } else {
                var gold = _chestRewardManager.GetChestReward(rewardType);
                if (gold < (priceData.Price * quantity)) {
                    ShowDialogNotEnoughGold();
                    return;
                }
            }
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin(0);
            var confirm = await DialogConfirm.Create();
            confirm.SetInfo(
                "Do you want to buy?",
                "Yes",
                "No",
                () => UniTask.Void(async () => {
                    try {
                        await _shopManager.BuyCostumeAsync(costumeData.ItemId, priceData.Package, quantity);
                        // Load lại avatar để loại bỏ item đã mua
                        await InitSubSegmentAvatar();
                        await _serverManager.General.GetChestReward();
                        // DialogOK.ShowInfo(this.canvasDialog, "Successfully");
                        BLDialogAvatarReward.ShowInfo(canvasDialog, costumeData.ItemId);
                        ClearCacheDataInventory();
                        TrackBuyCostume(costumeData, priceData, itemData, lockGemSpend, gemSpend, quantity);
                    } catch (Exception e) {
                        Logger.LogEditorError(e);
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(canvasDialog, e.Message);
                        } else {
                            DialogOK.ShowError(canvasDialog, e.Message);
                        }
                    } finally {
                        waiting.End();
                    }
                }),
                () => waiting.End()
            );
            confirm.Show(canvasDialog);
        }
        
        private (int gemLock, int gemUnlock) GetGemSpending(int gemLock, int gemUnlock, int price) {
            int gemLockSpending;
            int gemUnlockSpending;
            if (price <= gemLock) {
                gemLockSpending = price;
                gemUnlockSpending = 0;
            } else {
                gemLockSpending = (int) gemLock;
                gemUnlockSpending = (int) (price - gemLock);
            }
            return (gemLockSpending, gemUnlockSpending);
        }

        private async void RequestBuyGold(IAPGoldItemData dataGold) {
            var gemUnlock = _chestRewardManager.GetChestReward(BlockRewardType.Gem);
            var gemLock = _chestRewardManager.GetChestReward(BlockRewardType.LockedGem);
            var bLGem = gemLock + gemUnlock;
            if (bLGem < dataGold.Price) {
                // DialogOK.ShowInfo(canvasDialog, _languageManager.GetValue(LocalizeKey.ui_not_enough_gem));
                ShowDialogNotEnoughGem();
                return;
            }
            var gemSpending = GetGemSpending((int) gemLock, (int) gemUnlock, dataGold.Price);

            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin(0);
            var confirm = await DialogConfirm.Create();
            confirm.SetInfo(
                "Do you want to buy Gold?",
                "Yes",
                "No",
                () => UniTask.Void(async () => {
                    try {
                        await _serverRequester.BuyGold(dataGold.ItemId);
                        TrackBuyGold(dataGold, gemSpending.gemLock, gemSpending.gemUnlock, TrackResult.Done);
                        await _serverManager.General.GetChestReward();
                        DialogOK.ShowInfo(canvasDialog, "Successfully");
                    } catch (Exception e) {
                        TrackBuyGold(dataGold, gemSpending.gemLock, gemSpending.gemUnlock, TrackResult.Error);
                        if (e is ErrorCodeException) {
                            DialogError.ShowError(canvasDialog, e.Message);    
                        } else {
                            DialogOK.ShowError(canvasDialog, e.Message);
                        }
                    } finally {
                        waiting.End();
                    }
                }),
                () => waiting.End()
            );
            confirm.Show(canvasDialog);
        }

        public void OnButtonCloseClicked() {
            _soundManager.PlaySound(Audio.Tap);
            const string sceneName = "MainMenuScene";
            SceneLoader.LoadSceneAsync(sceneName).Forget();
        }

        private void TrackTabMenuClick(TypeMenuLeftShop tabMenuPick) {
            switch (tabMenuPick) {
                case TypeMenuLeftShop.Chest:
                    break;
                case TypeMenuLeftShop.Gems:
                    _analytics.TrackConversion(ConversionType.VisitGemShop);
                    break;
                case TypeMenuLeftShop.Gold:
                    break;
                case TypeMenuLeftShop.SwapGem:
                    _analytics.Iap_TrackOpenSwapGem();
                    _analytics.TrackConversion(ConversionType.ClickSwapGem);
                    break;
            }
            _analytics.TrackSceneAndSub(SceneType.VisitShop, tabMenuPick.ToString().ToLower());
        }

        private void TrackBuyGem(IAPGemItemData dataGem, PurchaseResult result) {
            var gemReceive = dataGem.GemReceive;
            if (dataGem.GemsBonus != (int) IAPGemItemData.EBonusType.Non) {
                gemReceive += dataGem.GemsBonus;
            }
            var productId = $"buy_{dataGem.ProductId}_gem";
            if (dataGem.BonusType == (int) IAPGemItemData.EBonusType.FirstTimePurchase) {
                productId += "_first_time";
            }
            TrackHelper.TrackBuyIap(_analytics, productId, result, gemReceive);
        }
        
        private void TrackBuyGold(IAPGoldItemData dataGold, int gemLockSpending, int gemSpending, TrackResult result) {
            _analytics.TrackConversion(result == TrackResult.Done
                ? ConversionType.BuySoftCurrency
                : ConversionType.BuySoftCurrencyFail);
            _analytics.Iap_TrackBuyGold(dataGold.ItemId, gemLockSpending, gemSpending, dataGold.Quantity, result);
        }

        private void TrackBuyGachaChest(GachaChestShopData dataChest, GachaChestPrice price,
            GachaChestItemData[] rewards,
            TrackResult result) {
            var sinkType = price.RewardType switch {
                BlockRewardType.BLGold => "sink_gold",
                BlockRewardType.Gem => "sink_gem",
                _ => "sink_coin"
            };

            if (result == TrackResult.Done) {
                var items = rewards.Select(e => e.ProductId.ToString()).ToArray();
                var values = rewards.Select(e => e.Value.ToString()).ToArray();

                _analytics.TrackConversion(ConversionType.BuyGachaChest);
                _analytics.Iap_TrackBuyGachaChest((int) dataChest.ChestType, dataChest.ChestType.ToString(),
                    sinkType, price.Price, dataChest.ItemQuantity, items, values, result);
                _analytics.Iap_TrackSoftCurrencyBuyGachaChest(sinkType, price.Price, result);

                _analytics.TrackConversion(ConversionType.OpenChest);

                var heroes = rewards.Select(it => _productManager.GetProduct((int) it.ProductId))
                    .Where(it => it.Type == (int) InventoryItemType.Hero).ToArray();
                var heroIds = heroes.Select(it => it.ItemId).ToArray();
                var heroNames = heroes.Select(it => it.ProductName).ToArray();
                _logManager.Log($"hero ids: {string.Join(", ", heroIds)}");
                _logManager.Log($"hero names: {string.Join(", ", heroNames)}");
                _analytics.Inventory_TrackOpenChestGetHeroTr(heroNames, heroIds);
            } else if (result == TrackResult.Error) {
                _analytics.TrackConversion(ConversionType.BuyGachaChestFail);
                _analytics.Iap_TrackBuyGachaChest((int) dataChest.ChestType, nameof(dataChest.ChestType),
                    sinkType, price.Price, dataChest.ItemQuantity, Array.Empty<string>(), Array.Empty<string>(),
                    result);
                _analytics.Iap_TrackSoftCurrencyBuyGachaChest(sinkType, price.Price, result);
            }
        }

        private void ClearCacheDataInventory() {
            _inventoryManager.Clear();
        }

        private void TrackBuyCostume(CostumeData costumeData, CostumeData.PriceData priceData, ProductItemData itemData,
            float lockGemSpend, float gemSpend, int quantity) {
            var rewardType = RewardUtils.ConvertToBlockRewardType(priceData.RewardType);
            if (rewardType is BlockRewardType.Gem or BlockRewardType.LockedGem) {
                // Use Gem
                _analytics.ShopBuyCostumeUseGem_TrackSoftCurrency(
                    ProductItemData.GetTrackNameProductItem(itemData),
                    lockGemSpend,
                    gemSpend,
                    quantity,
                    itemData.ItemKindStr,
                    priceData.Duration > 0 ? TimeUtil.ConvertTimeToStringFull(priceData.Duration) : "forever",
                    TrackResult.Done
                );
            } else {
                // Use Gold
                _analytics.ShopBuyCostumeUseGold_TrackSoftCurrency(
                    ProductItemData.GetTrackNameProductItem(itemData),
                    priceData.Price,
                    1,
                    itemData.ItemKindStr,
                    priceData.Duration > 0 ? TimeUtil.ConvertTimeToStringFull(priceData.Duration) : "forever",
                    TrackResult.Done
                );
            }
        }

        private void ShowDialogNotEnoughGem() {
            DialogNotificationToShop.ShowOn(canvasDialog, DialogNotificationToShop.Reason.NotEnoughGem,
                () => { frameShop.SetSelectTab(TypeMenuLeftShop.Gems); });
        }

        private void ShowDialogNotEnoughGold() {
            DialogNotificationToShop.ShowOn(canvasDialog, DialogNotificationToShop.Reason.NotEnoughGold,
                () => { frameShop.SetSelectTab(TypeMenuLeftShop.Gold); });
        }

        private void Update() {
            if(_inputManager.ReadButton(_inputManager.InputConfig.Back)) {
                OnButtonCloseClicked();
            }
        }
    }
}