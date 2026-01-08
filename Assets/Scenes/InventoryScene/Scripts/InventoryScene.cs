using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Analytics;

using App;

using Constant;

using Controller;

using Cysharp.Threading.Tasks;

using Data;

using DG.Tweening;

using Engine.Entities;

using Game.Dialog;
using Game.Manager;
using Game.UI;

using Scenes.ShopScene.Scripts;

using Senspark;

using Services;
using Services.IapAds;

using Share.Scripts.Utils;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Utils;

using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace Scenes.InventoryScene.Scripts {
    public class InventoryScene : BaseTabListScene {
        private static BLItemType ConvertToBLItemType(BLTabType type) {
            return type switch {
                BLTabType.BombSkin => BLItemType.Bomb,
                BLTabType.Booster => BLItemType.Booster,
                BLTabType.Wing => BLItemType.Wing,
                BLTabType.Trail => BLItemType.Trail,
                BLTabType.OnSell => BLItemType.OnSell,
                BLTabType.FireSkin => BLItemType.Fire,
                BLTabType.Misc => BLItemType.Misc,
                BLTabType.Avatar => BLItemType.Avatar,
                _ => throw new Exception($"Invalid Item Type: {type}")
            };
        }
        
        public static void LoadScene(BLTabType tabPrefer) {
                PreferTab = tabPrefer;
                var currSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                const string sceneName = "InventoryScene";
                SceneLoader.LoadSceneAsync(sceneName);
        }

        [SerializeField]
        private BLInventoryHeroesController heroesController;

        // [SerializeField]
        // private BLInventoryChestController chestController;

        [SerializeField]
        private BLInventoryEmojiController emojiController;
        
        [SerializeField]
        private BLInventoryAvatarController avatarController;

        [SerializeField]
        private BLInventoryController[] controllers;

        // [SerializeField]
        // private GameObject prefabDialogGachaChest;

        [SerializeField]
        private Image splashFade;

        private ISkinManager _skinManager;
        private IInventoryManager _inventoryManager;
        private IUnityAdsManager _adsManager;
        private IAnalytics _analytics;
        private IChestRewardManager _chestRewardManager;
        private ILanguageManager _languageManager;
        private ILogManager _logManager;
        private IProductItemManager _productItemManager;
        private IServerManager _serverManager;
        private BLTabType _currentTabType = BLTabType.Null;

        protected override void Awake() {
            base.Awake();
            _skinManager = ServiceLocator.Instance.Resolve<ISkinManager>();
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();
            _adsManager = ServiceLocator.Instance.Resolve<IUnityAdsManager>();
            _analytics = ServiceLocator.Instance.Resolve<IAnalytics>();
            _chestRewardManager = ServiceLocator.Instance.Resolve<IChestRewardManager>();
            _languageManager = ServiceLocator.Instance.Resolve<ILanguageManager>();
            _logManager = ServiceLocator.Instance.Resolve<ILogManager>();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            _serverManager = ServiceLocator.Instance.Resolve<IServerManager>();

            splashFade.gameObject.SetActive(true);
        }

        private void Start() {
            var waiting = new WaitingUiManager(canvasDialog);
            waiting.Begin();
            InitUi();
            SelectDefaultContent();
            UniTask.Void(async () => {
                await _serverManager.WaitForUserInitialized();
                GetChestReward();
                waiting.End();
                splashFade.DOFade(0.0f, 0.3f).OnComplete(() => { splashFade.gameObject.SetActive(false); });
                // await PreLoadData();
            });
        }
        
        public void OnShowMoreUpdate() {
            CurrentContent.UpdatePage();
        }

        protected override void ClearCacheData(IEnumerable<BLTabType> tabTypes) {
            foreach (var it in tabTypes) {
                ClearCacheData(it);
            }
        }
        
        private void ClearCacheData(BLTabType tabType) {
            var dic = controllers.ToDictionary(it => it.ItemType);
            switch (tabType) {
                case BLTabType.Heroes:
                    heroesController.ClearCacheData();
                    break;
                // case BLTabType.Chest:
                //     chestController.ClearCacheData();
                //     break;
                case BLTabType.Misc: {
                    var ctrl = dic[BLItemType.Misc];
                    ctrl.ClearCacheData();
                    break;
                }
                case BLTabType.Costume: {
                    var ctrl = dic[BLItemType.Costume];
                    ctrl.ClearCacheData();
                    break;
                }
                case BLTabType.Emoji: {
                    emojiController.ClearCacheData();
                    break;
                }
                case BLTabType.Avatar: {
                    avatarController.ClearCacheData();
                    break;
                }
                default: {
                    var itemType = ConvertToBLItemType(tabType);
                    if (!dic.ContainsKey(itemType)) {
                        _logManager.Log($"Not find key: {itemType.ToString()}");
                        return;
                    }
                    var ctrl = dic[itemType];
                    ctrl.ClearCacheData();
                    break;
                }
            }
        }

        private void ClearCacheData(BLItemType itemType) {
            var dic = controllers.ToDictionary(it => it.ItemType);
            switch (itemType) {
                case BLItemType.Hero:
                    heroesController.ClearCacheData();
                    break;
                // case BLItemType.Chest:
                //     chestController.ClearCacheData();
                //     break;
                case BLItemType.Booster:
                case BLItemType.Misc:
                    var miscCtrl = dic[BLItemType.Misc];
                    miscCtrl.ClearCacheData();
                    break;
                case BLItemType.Wing:
                case BLItemType.Bomb:
                case BLItemType.Fire:
                case BLItemType.Trail:
                    var costumeCtrl = dic[BLItemType.Costume];
                    costumeCtrl.ClearCacheData();
                    break;
                case BLItemType.Emoji:
                    emojiController.ClearCacheData();
                    break;
                case BLItemType.Avatar:
                    avatarController.ClearCacheData();
                    break;
            }
        }

        
        private async Task PreLoadData() {
            var dic = controllers.ToDictionary(it => it.ItemType);
            
            // preload Heroes
            if (heroesController.Heroes == null) {
                var result = await _inventoryManager.GetHeroesAsync();
                heroesController.Heroes = result.ToList();
            }
            
            // preload Chest
            // if (chestController.Items == null) {
            //     var result = await _inventoryManager.GetChestAsync();
            //     chestController.Items = result.ToList();
            // }
            
            // preload Costume
            var ctrlCostume = dic[BLItemType.Costume];
            if (ctrlCostume.Items == null) {
                ctrlCostume.Items = new List<InventoryItemData>();
                ctrlCostume.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Avatar));
                ctrlCostume.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.BombSkin));
                ctrlCostume.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Fire));
                ctrlCostume.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Trail));
            }

            // preload Misc
            var ctrlMisc = dic[BLItemType.Misc];
            if (ctrlMisc.Items == null) {
                ctrlMisc.Items = new List<InventoryItemData>();
                ctrlMisc.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Booster));
                ctrlMisc.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Misc));
                ctrlMisc.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Altar));
            }
        
            // preload emoji
            if (emojiController.Items == null) {
                var result = await _inventoryManager.GetItemsAsync(InventoryItemType.Emoji);
                emojiController.Items = result.ToList();
            }
        
            // preload Avatar
            if (avatarController.Items == null) {
                avatarController.Items = new List<InventoryItemData>();
                avatarController.Items.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.AvatarTR));
            }
        
            // preload OnSell
            var ctrlOnSell = dic[BLItemType.OnSell];
            if (ctrlOnSell.SellingItems == null) {
                var result = await _inventoryManager.GetSellingItemsAsync();
                ctrlOnSell.SellingItems = result.ToList();
            }
        }
        
        protected override async Task LoadData(BLTabType tabType) {
            switch (tabType) {
                case BLTabType.Heroes:
                    heroesController.OnShowDialogInfo = () => ShowDialogInfo(canvasDialog, tabType);
                    await heroesController.SetMaxItems();
                    var heroes = await heroesController.LoadData();
                    break;
                // case BLTabType.Chest:
                //     if (_currentTabType != tabType) {
                //         Analytics.TrackScene(SceneType.VisitChest);
                //     }
                //     await chestController.SetMaxItems();
                //     await chestController.LoadData();
                //     chestController.SetOnActiveChest(OnActiveChest);
                //     chestController.SetOnOpenChest(OnOpenChest);
                //     chestController.SetOnUnlockSlot(OnUnlockSlot);
                //     chestController.SetOnSkipTimeByGem(OnSkipTimeByGem);
                //     chestController.SetOnSkipTimeByAds(OnSkipTimeByAds);
                //     chestController.SetOnShowInfo(OnShowInfo);
                //     break;
                case BLTabType.Misc: {
                    var dic = controllers.ToDictionary(it => it.ItemType);
                    var ctrl = dic[BLItemType.Misc];
                    await ctrl.SetMaxItems();
                    var list = ctrl.Items;
                    if (list == null) {
                        list = new List<InventoryItemData>();
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Booster));
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Misc));
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Altar));
                    }
                    ctrl.LoadDataByItemList(canvasDialog, list);
                    break;
                }
                case BLTabType.Costume: {
                    var dic = controllers.ToDictionary(it => it.ItemType);
                    var ctrl = dic[BLItemType.Costume];
                    await ctrl.SetMaxItems();
                    var list = ctrl.Items;
                    if (list == null) {
                        list = new List<InventoryItemData>();
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Avatar));
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.BombSkin));
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Fire));
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.Trail));
                    }
                    ctrl.LoadDataByItemList(canvasDialog, list);
                    break;
                }
                case BLTabType.Emoji: {
                    await emojiController.SetMaxItems();
                    await emojiController.LoadData();
                    emojiController.SetEquipCallback(SendEquip);
                    break;
                }
                case BLTabType.Avatar: {
                    var list = avatarController.Items;
                    if (list == null) {
                        list = new List<InventoryItemData>();
                        list.AddRange(await _inventoryManager.GetItemsAsync(InventoryItemType.AvatarTR));
                    }
                    await avatarController.SetMaxItems();
                    avatarController.LoadDataByItemList(canvasDialog, list);
                    avatarController.SetEquipCallback(SendEquip);
                    break;
                }
                default: {
                    var itemType = ConvertToBLItemType(tabType);
                    var dic = controllers.ToDictionary(it => it.ItemType);
                    if (!dic.ContainsKey(itemType)) {
                        _logManager.Log($"Not find key: {itemType.ToString()}");
                        return;
                    }
                    var ctrl = dic[itemType];
                    if (tabType != BLTabType.OnSell) {
                        await ctrl.SetMaxItems();
                        await ctrl.LoadData(canvasDialog);
                    } else {
                        await ctrl.SetMaxItems();
                        await ctrl.LoadDataOnSell(canvasDialog, ClearCacheData);
                    }
                    break;
                }
            }
            _currentTabType = tabType;
        }

        protected override void ShowDialogItem(ItemData item) {
            DialogItemSell.Create().ContinueWith(dialog => {
                dialog.SetInfo(item);
                dialog.OnHideDialogSell = OnHideDialogSellBuy;
                dialog.Show(canvasDialog);
            });
        }

        protected override void ShowDialogHero(UIHeroData hero) {
            DialogHeroSell.Create().ContinueWith(dialog => {
                dialog.SetInfo(hero);
                dialog.OnHideDialogSell = OnHideDialogSellBuy;
                dialog.Show(canvasDialog);
            });
        }

        protected override void ShowDialogOrder(OrderDataRequest data) {
            
        }

        protected override void ShowDialogOrderError() {
            
        }

        // private async void OnActiveChest(InventoryChestData chestData) {
        //     var waiting = new WaitingUiManager(canvasDialog);
        //     waiting.Begin();
        //     try {
        //         await ServerRequester.StartOpeningGachaChest(chestData.ChestId);
        //         chestController.ClearCacheData();
        //         await chestController.LoadData();
        //         Analytics.TrackConversion(ConversionType.ClickActiveChest);
        //     } catch (Exception e) {
        //         if (e is ErrorCodeException) {
        //             DialogError.ShowError(canvasDialog, e.Message);    
        //         } else {
        //             DialogOK.ShowError(canvasDialog, e.Message);
        //         }
        //     } finally {
        //         waiting.End();
        //     }
        // }

        // private async void OnOpenChest(InventoryChestData chestData) {
        //     var waiting = new WaitingUiManager(canvasDialog);
        //     waiting.Begin();
        //     try {
        //         var itemsReward = await ServerRequester.OpenGachaChest(_productItemManager, chestData.ChestId);
        //         TrackOpenChest(chestData, itemsReward);
        //         chestController.ClearCacheData();
        //         await chestController.LoadData();
        //         await ServerManager.General.GetChestReward();
        //         waiting.End();
        //         var dialog =
        //             BLDialogGachaChest.CreateFromChestInventory(prefabDialogGachaChest, itemsReward, chestData);
        //         dialog.Show(canvasDialog);
        //     } catch (Exception e) {
        //         if (e is ErrorCodeException) {
        //             DialogError.ShowError(canvasDialog, e.Message);    
        //         } else {
        //             DialogOK.ShowError(canvasDialog, e.Message);
        //         }
        //     } finally {
        //         waiting.End();
        //     }
        // }

        private async void SendEquip(int itemType, (int, long)[] itemList) {
            try {
                await _skinManager.EquipSkinAsync(itemType, itemList);
            } catch (Exception e) {
                _logManager.Log($"Cannot Equipped : {e.Message}");
            }
        }

        private void TrackOpenChest(InventoryChestData chestData, GachaChestItemData[] rewards) {
            var productIds = rewards.Select(e => e.ProductId.ToString()).ToArray();
            Analytics.Inventory_TrackOpenChestByGem(
                chestData.ChestName,
                chestData.ChestId,
                productIds,
                rewards.Select(e => e.Value).ToArray()
            );
            Analytics.TrackConversion(ConversionType.OpenChest);

            // FIXME: chưa làm: track các item là hero (nếu có)
            // _analytics.Inventory_TrackOpenChestGetHeroTr();
        }
        
        private void ShowDialogInfo(Canvas canvasDialog, BLTabType tabType) {
            switch (tabType) {
                case BLTabType.Chest:
                    break;
                case BLTabType.Heroes:
                    ShowHeroesInfo(canvasDialog);
                    break;
                case BLTabType.Costume:
                    break;
                case BLTabType.Misc:
                    break;
                case BLTabType.Emoji:
                    break;
                case BLTabType.Avatar:
                    break;
                case BLTabType.OnSell:
                    break;
                default: {
                    break;
                }
            }
        }

        private void ShowHeroesInfo(Canvas canvasDialog) {
            DialogChestInfo.Create().ContinueWith((dialog) => {
                dialog.SetTitle("HERO SOUL INFORMATION")
                    .SetDescription(
                        "Hero souls are hero items that you own.\nHero souls can be ground into crystals.")
                    .Show(canvasDialog);
            });
        }

        public void OnVisitShopHero() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            ShopScene.Scripts.ShopScene.LoadScene(TypeMenuLeftShop.Hero);
        }
        
        public void OnVisitShopCostume() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            ShopScene.Scripts.ShopScene.LoadScene(TypeMenuLeftShop.Costume);
        }
        
        public void OnVisitMarketBooster() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            MarketplaceScene.Scripts.MarketplaceScene.LoadScene(BLTabType.Booster);
        }
    }
}