using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App;
using Castle.Core.Internal;
using Constant;

using Cysharp.Threading.Tasks;

using Data;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI.Custom;
using Game.UI.FrameTabScroll;

using Scenes.ShopScene.Scripts;

using Senspark;
using Services;

using Share.Scripts.Dialog;

using UnityEngine;

public enum TypeMenuLeftShop {
    Chest,
    Gems,
    Gold,
    SwapGem,
    Costume,
    Hero,
    Wing,
    BombSkin,
    FireSkin,
    Trail,
    Emoji,
    Subscription,
    ChestP2P,
    ChestNormal,
    Avatar,
};

namespace Game.Dialog.BomberLand.BLFrameShop {
    public abstract class FrameTabScroll : FrameTabScrollBase<TypeMenuLeftShop> {
    };

    public class BLFrameShop : FrameTabScroll {
        [SerializeField]
        public BLShopResource shopResource;

        [SerializeField]
        public CustomContentSizeFitter contentSizeFitter;

        [SerializeField]
        private Transform contentSingle;

        [SerializeField]
        private GameObject rootSingle;

        [SerializeField]
        private GameObject rootScroll;

        [SerializeField]
        private BLGachaRes gachaRes;

        [SerializeField]
        private BLShopCostumeInfo shopCostumeInfo;

        private TaskCompletionSource<bool> _waitInitUi = null;
        private Action<TypeMenuLeftShop> _trackTabMenuClick;
        private BLShopItemGem _slotFreeGem;
        private BLShopItemGold _slotFreeGold;
        private BLShopGemInfo _shopGemInfo;
        private int _lastIdxGemSlotSelected = -1;
        private int _lastIdxSubscriptionSlotSelected = -1;
        private BLShopGoldInfo _shopGoldInfo;
        private Dictionary<TypeMenuLeftShop, CSegment> _dicSegmentSingles;
        private BLSwapGem _swapGem;

        public BLShopItemGem SlotFreeGem => _slotFreeGem;
        public BLShopItemGold SlotFreeGold => _slotFreeGold;
        public BLShopGemInfo ShopGemInfo => _shopGemInfo;
        public BLShopGoldInfo ShopGoldInfo => _shopGoldInfo;

        private Dictionary<TypeMenuLeftShop, CSegment> _dicSegmentFull;

        private IInventoryManager _inventoryManager;

        protected override void Awake() {
            base.Awake();
            var segments = contentSingle.GetComponentsInChildren<CSegment>();
            _dicSegmentSingles = segments.ToDictionary(it => it.TypeMenu);
            shopCostumeInfo.gameObject.SetActive(false);
            _inventoryManager = ServiceLocator.Instance.Resolve<IInventoryManager>();;
        }

        public Task<bool> InitUi() {
            var task = new TaskCompletionSource<bool>();
            _waitInitUi = task;
            return task.Task;
        }

        protected override void OnInitSegmentsDone() {
            /*
            var segmentShopWithItems =this.segmentList.GetComponentsInChildren<BLSegmentShopWithItem>();
            foreach (var segmentShopWithItem in segmentShopWithItems) {
                if (segmentShopWithItem.TypeMenu == TypeMenuLeftShop.Chest) {
                    // this.InitChest(segmentShopWithItem);
                } else {
                    segmentShopWithItem.Init(4);
                }
            }
            */
        }

        protected override void LateUpdate() {
            base.LateUpdate();
            if (_waitInitUi == null || !IsInitSegments) {
                return;
            }
            _dicSegmentFull = new Dictionary<TypeMenuLeftShop, CSegment>(DicSegments);
            foreach (var e in _dicSegmentSingles) {
                _dicSegmentFull.Add(e.Key, e.Value);
            }
            _waitInitUi.SetResult(true);
            _waitInitUi = null;
        }

        public void UiInitChest(Canvas canvasDialog,
            GachaChestShopData[] dataChestP2P, GachaChestShopData[] dataChestNormal,
            Action<GachaChestShopData, GachaChestPrice, int> requestBuyChest) {
            UiInitChestWithType(canvasDialog, TypeMenuLeftShop.ChestP2P, dataChestP2P, requestBuyChest);
            UiInitChestWithType(canvasDialog, TypeMenuLeftShop.ChestNormal, dataChestNormal, requestBuyChest);

            var segment = (BLSegmentShopWithItem)_dicSegmentFull[TypeMenuLeftShop.Chest];
            segment.AutoLayoutVertical();
            var subSegment = dataChestP2P.Length > 0
                ? (BLSegmentShopWithItem)_dicSegmentFull[TypeMenuLeftShop.ChestP2P]
                : (BLSegmentShopWithItem)_dicSegmentFull[TypeMenuLeftShop.ChestNormal];
            subSegment.SelectFirstItem();
        }

        private void UiInitChestWithType(Canvas canvasDialog, TypeMenuLeftShop type, GachaChestShopData[] dataChest,
            Action<GachaChestShopData, GachaChestPrice, int> requestBuyChest) {
            var segment = (BLSegmentShopWithItem)_dicSegmentFull[type];
            if (dataChest.Length == 0) {
                segment.gameObject.SetActive(false);
                return;
            }

            var objs = segment.InitSlot(dataChest.Length);
            for (var idx = 0; idx < objs.Length; idx++) {
                if (idx >= objs.Length) {
                    break;
                }
                var obj = objs[idx];
                if (!obj) {
                    break;
                }
                var d = dataChest[idx];
                var itemChest = obj.GetComponent<BLShopItemChest>();
                if (!itemChest) {
                    continue;
                }
                // Update Item Info
                itemChest.SetData(shopResource, d);
            }

            var segmentChest = (BLSegmentShopWithItem)_dicSegmentFull[TypeMenuLeftShop.Chest];
            var chestInfo = segmentChest.ObjInfo.GetComponent<BLShopChestInfo>();
            segment.OnSelectInfo = (int idxSlotSelected) => {
                segmentChest.ObjInfo.SetActive(true);
                var d = dataChest[idxSlotSelected];
                chestInfo.SetData(shopResource, d);
                chestInfo.SetOnBuy((price, quantity) => { requestBuyChest.Invoke(d, price, quantity); });

                if (ScreenUtils.IsIPadScreen()) {
                    chestInfo.SetOnShowInfo(() => { DialogGachaChestInfoPad.Create(canvasDialog, d); });
                    
                } else {
                    chestInfo.SetOnShowInfo(() => { DialogGachaChestInfo.Create(canvasDialog, d); });
                }

                //Unselect the other segment
                var otherType = type == TypeMenuLeftShop.ChestP2P
                    ? TypeMenuLeftShop.ChestNormal
                    : TypeMenuLeftShop.ChestP2P;
                var otherSegment = (BLSegmentShopWithItem)_dicSegmentFull[otherType];
                otherSegment.UnSelectAllItem();
            };

            segment.OnShowDialogInfo = () => ShowChestInfo(canvasDialog, type);
        }

        public Task UiInitSwapGem(Canvas canvas) {
            var segment = (BLSegmentShopWithFrame)_dicSegmentFull[TypeMenuLeftShop.SwapGem];
            _swapGem = segment.Frame.GetComponent<BLSwapGem>();
            _swapGem.Initialize(canvas);
            return Task.CompletedTask;
        }

        public async Task UiInitHero(Canvas canvasDialog, IShopManager shopManager,
            IProductItemManager productItemManager,
            IEarlyConfigManager earlyConfigManager,
            Action<CostumeData, CostumeData.PriceData, ProductItemData, int> requestBuyCostume) {
            var segment = (BLSegmentShopWithItem) _dicSegmentFull[TypeMenuLeftShop.Hero];
            try {
                var itemInfo = segment.ObjInfo.GetComponent<BLShopCostumeInfo>();
                await UiInitInventoryItem(segment, itemInfo, InventoryItemType.Hero, shopManager,
                    productItemManager, requestBuyCostume);
                segment.AutoLayoutVertical();
                segment.SelectFirstItem();
            } catch (Exception e) {
                DialogOK.ShowError(canvasDialog, e.Message);
            }
        }
        
        public void UiHideChest() {
            DicTabs[TypeMenuLeftShop.Chest].gameObject.SetActive(false);
            _dicSegmentFull[TypeMenuLeftShop.Chest].gameObject.SetActive(false);
        }

        public void UiHideGem() {
            DicTabs[TypeMenuLeftShop.Gems].gameObject.SetActive(false);
            _dicSegmentFull[TypeMenuLeftShop.Gems].gameObject.SetActive(false);
        }

        public void UiHideSwapGem() {
            DicTabs[TypeMenuLeftShop.SwapGem].gameObject.SetActive(false);
            _dicSegmentFull[TypeMenuLeftShop.SwapGem].gameObject.SetActive(false);
        }

        public void UiHideTab(TypeMenuLeftShop typeMenu) {
            DicTabs[typeMenu].gameObject.SetActive(false);
            _dicSegmentFull[typeMenu].gameObject.SetActive(false);
        }

        public void UiInitGemData(
            IIAPItemManager itemManager,
            IAPGemItemData[] dataGem,
            Action<IAPGemItemData> requestBuyGem,
            Action requestGetFreeGem) {
            var segment = (BLSegmentShopWithItem)_dicSegmentFull[TypeMenuLeftShop.Gems];
            //var shopResource = objShopResource.GetComponent<BLShopResource>();
            var objs = segment.InitSlot(dataGem.Length + 1);
            for (var idx = 0; idx < objs.Length; idx++) {
                if (idx >= objs.Length) {
                    break;
                }
                var obj = objs[idx];
                if (!obj) {
                    break;
                }
                var itemGem = obj.GetComponent<BLShopItemGem>();
                if (idx == 0) {
                    // Free Slot
                    itemGem.SetData(itemManager.GetFreeGemRewardConfigs());
                    _slotFreeGem = itemGem;
                } else {
                    var d = dataGem[idx - 1];
                    if (!itemGem) {
                        continue;
                    }
                    segment.Items[idx].SetIsEmpty(false);
                    // Update Item Info
                    itemGem.SetData(shopResource, d);
                }
            }
            var gemInfo = segment.ObjInfo.GetComponent<BLShopGemInfo>();
            _shopGemInfo = gemInfo;
            segment.OnSelectInfo = (int idxSlotSelected) => {
                _lastIdxGemSlotSelected = idxSlotSelected;
                segment.ObjInfo.SetActive(true);
                if (idxSlotSelected == 0) {
                    gemInfo.SetData(itemManager.GetFreeGemRewardConfigs());
                    gemInfo.SetOnGetFreeGem(requestGetFreeGem);
                } else {
                    var d = dataGem[idxSlotSelected - 1];
                    gemInfo.SetData(shopResource, d);
                    gemInfo.SetCallbacks(() => requestBuyGem.Invoke(d));
                }
            };
            if (_lastIdxGemSlotSelected > 0) {
                segment.OnSelectInfo(_lastIdxGemSlotSelected);
            } else {
                // Hide item free
                segment.HideItemAt(0);
                segment.SelectFirstItem();
            }
        }

        public void UiInitGoldData(IIAPItemManager itemManager, IAPGoldItemData[] dataGold,
            Action<IAPGoldItemData> requestBuyGold, Action requestGetFreeGold) {
            var segment = (BLSegmentShopWithItem)_dicSegmentFull[TypeMenuLeftShop.Gold];
            //var shopResource = objShopResource.GetComponent<BLShopResource>();
            var objs = segment.InitSlot(dataGold.Length + 1);
            for (var idx = 0; idx < objs.Length; idx++) {
                if (idx >= objs.Length) {
                    break;
                }
                var obj = objs[idx];
                if (!obj) {
                    break;
                }
                var itemGold = obj.GetComponent<BLShopItemGold>();
                if (idx == 0) {
                    // Free Slot
                    itemGold.SetData(itemManager.GetFreeGoldRewardConfigs());
                    _slotFreeGold = itemGold;
                } else {
                    var d = dataGold[idx - 1];
                    itemGold.SetData(shopResource, d);
                }
            }
            var goldInfo = segment.ObjInfo.GetComponent<BLShopGoldInfo>();
            _shopGoldInfo = goldInfo;
            segment.OnSelectInfo = (int idxSlotSelected) => {
                segment.ObjInfo.SetActive(true);
                if (idxSlotSelected == 0) {
                    goldInfo.SetData(itemManager.GetFreeGoldRewardConfigs());
                    goldInfo.SetOnGetFreeGold(requestGetFreeGold);
                } else {
                    var d = dataGold[idxSlotSelected - 1];
                    goldInfo.SetData(shopResource, d);
                    goldInfo.SetOnBuy(() => { requestBuyGold.Invoke(d); });
                }
            };
            if (!Application.isMobilePlatform) {
                // Hide item free
                segment.HideItemAt(0);
            }
            segment.SelectFirstItem();
        }

        public async Task UiInitCostume(Canvas canvasDialog, IShopManager shopManager, IProductItemManager productItemManager,
            IEarlyConfigManager earlyConfigManager,
            Action<CostumeData, CostumeData.PriceData, ProductItemData, int> requestBuyCostume) {
            try {
                await productItemManager.InitializeAsync();
                foreach (var it in DicSegments) {
                    var segment = (BLSegmentShopWithItem) it.Value;
                    var type = segment.TypeMenu switch {
                        //TypeMenuLeftShop.Hero => InventoryItemType.Hero,
                        TypeMenuLeftShop.Wing => InventoryItemType.Avatar,
                        TypeMenuLeftShop.BombSkin => InventoryItemType.BombSkin,
                        TypeMenuLeftShop.FireSkin => InventoryItemType.Fire,
                        TypeMenuLeftShop.Trail => InventoryItemType.Trail,
                        _ => throw new Exception($"Invalid Item Type: {segment.TypeMenu}")
                    };
                    await UiInitInventoryItem(segment, shopCostumeInfo, type, shopManager, productItemManager,
                        requestBuyCostume);
                }
            } catch (Exception e) {
                DialogOK.ShowError(canvasDialog, e.Message);
            }
        }

        private async Task UiInitInventoryItem(BLSegmentShopWithItem segment, BLShopCostumeInfo itemInfo,
            InventoryItemType type, IShopManager shopManager, IProductItemManager productItemManager,
            Action<CostumeData, CostumeData.PriceData, ProductItemData, int> requestBuyCostume) {
            var costumes = await shopManager.GetCostumesAsync((int)type);
            var sortedCostumes = costumes
                .OrderByDescending(e => productItemManager.GetItem(e.ItemId).TagShop)
                .ThenBy(e => productItemManager.GetItem(e.ItemId).Name)
                .ToArray();
            var slots = segment.InitSlot(sortedCostumes.Length);
            for (var idx = 0; idx < slots.Length; idx++) {
                var c = sortedCostumes[idx];
                var itemData = productItemManager.GetItem(c.ItemId);
                var slot = slots[idx];
                var item = slot.GetComponent<BLShopItemCostume>();
                item.SetData(gachaRes, itemData, c);
            }
            // Init Info
            segment.OnSelectInfo = (int idx) => {
                foreach (var segmentShop in DicSegments.Select(s => (BLSegmentShopWithItem)s.Value)
                             .Where(segmentShop => segmentShop != segment)) {
                    segmentShop.UnSelectAllItem();
                }
                itemInfo.gameObject.SetActive(true);
                var costume = sortedCostumes[idx];
                var itemData = productItemManager.GetItem(costume.ItemId);
                itemInfo.SetData(gachaRes, itemData, costume);
                itemInfo.SetOnBuy((int idxPrice, int quantity) => {
                    requestBuyCostume?.Invoke(costume, costume.Prices[idxPrice], itemData, quantity);
                });
            };
        }

        private async Task UiInitEmojiItem(BLSegmentShopWithItem segment, BLShopCostumeInfo itemInfo, 
            InventoryItemType type, IShopManager shopManager, IProductItemManager productItemManager,
            Action<CostumeData, CostumeData.PriceData, ProductItemData, int> requestBuyCostume) {
            var costumes = await shopManager.GetCostumesAsync((int)type);

            // Loại bỏ những emoji đã sở hữu
            var ownEmojis = await _inventoryManager.GetItemsAsync((int)type);
            var ownEmojisId = ownEmojis.Select(it => it.ItemId).ToList();
            if (ownEmojisId.Any()) {
                ownEmojisId.ForEach(item => shopManager.RemoveItemShop(item, (int)type));
            }
            
            costumes = costumes.FindAll(item => !ownEmojisId.Contains(item.ItemId));
            var sortedCostumes = costumes
                .OrderByDescending(e => productItemManager.GetItem(e.ItemId).TagShop)
                .ThenBy(e => e.ItemId)
                .ToArray();
            if (sortedCostumes.Length == 0) {
                itemInfo.gameObject.SetActive(false);
            }
            var slots = segment.InitSlot(sortedCostumes.Length);
            for (var idx = 0; idx < slots.Length; idx++) {
                var c = sortedCostumes[idx];
                var itemData = productItemManager.GetItem(c.ItemId);
                var slot = slots[idx];
                var item = slot.GetComponent<BLShopItemCostume>();
                item.SetData(gachaRes, itemData, c);
            }
            // Init Info
            segment.OnSelectInfo = (int idx) => {
                foreach (var segmentShop in DicSegments.Select(s => (BLSegmentShopWithItem)s.Value)
                             .Where(segmentShop => segmentShop != segment)) {
                    segmentShop.UnSelectAllItem();
                }
                itemInfo.gameObject.SetActive(true);
                var costume = sortedCostumes[idx];
                var itemData = productItemManager.GetItem(costume.ItemId);
                itemInfo.SetData(gachaRes, itemData, costume);
                itemInfo.SetOnBuy((int idxPrice, int quantity) => {
                    requestBuyCostume?.Invoke(costume, costume.Prices[idxPrice], itemData, quantity);
                });
            };
        }
        
        private async Task UiInitAvatarItem(BLSegmentShopWithItem segment, BLShopCostumeInfo itemInfo, 
            InventoryItemType type, IShopManager shopManager, IProductItemManager productItemManager,
            Action<CostumeData, CostumeData.PriceData, ProductItemData, int> requestBuyCostume) {
            var items = await shopManager.GetCostumesAsync((int)type);
            
            // Loại bỏ những avatar đã sở hữu
            var ownItems = await _inventoryManager.GetItemsAsync((int)type);
            var ownItemsId = ownItems.Select(it => it.ItemId).ToList();
            if (ownItemsId.Any()) {
                ownItemsId.ForEach(item => shopManager.RemoveItemShop(item, (int)type));
            }
            
            items = items.FindAll(item => !ownItemsId.Contains(item.ItemId));
            var sortedItems = items
                .OrderBy(e => RewardUtils.ConvertToBlockRewardType(e.Prices[0].RewardType))
                .ThenByDescending(e => e.ItemId)
                .ToArray();
            if (sortedItems.Length == 0) {
                itemInfo.gameObject.SetActive(false);
            }
            var slots = segment.InitSlot(sortedItems.Length);
            for (var idx = 0; idx < slots.Length; idx++) {
                var c = sortedItems[idx];
                var itemData = productItemManager.GetItem(c.ItemId);
                var slot = slots[idx];
                var item = slot.GetComponent<BLShopItemAvatar>();
                item.SetData(itemData);
            }
            // Init Info
            segment.OnSelectInfo = (int idx) => {
                foreach (var segmentShop in DicSegments.Select(s => (BLSegmentShopWithItem)s.Value)
                             .Where(segmentShop => segmentShop != segment)) {
                    segmentShop.UnSelectAllItem();
                }
                itemInfo.gameObject.SetActive(true);
                var costume = sortedItems[idx];
                var itemData = productItemManager.GetItem(costume.ItemId);
                itemInfo.SetData(gachaRes, itemData, costume);
                itemInfo.SetOnBuy((int idxPrice, int quantity) => {
                    requestBuyCostume?.Invoke(costume, costume.Prices[idxPrice], itemData, quantity);
                });
            };
        }

        public void UiInitSubscription(
            IAPSubscriptionItemData[] subscriptionData,
            Action<IAPSubscriptionItemData> requestSubscribe,
            Action<IAPSubscriptionItemData> requestUnsubscribe) {
            var segment = (BLSegmentShopSubscription)_dicSegmentFull[TypeMenuLeftShop.Subscription];
            //var shopResource = objShopResource.GetComponent<BLShopResource>();
            var objs = segment.InitSlot(subscriptionData.Length);
            for (var idx = 0; idx < objs.Length; idx++) {
                if (idx >= objs.Length) {
                    break;
                }
                var obj = objs[idx];
                if (!obj) {
                    break;
                }
                var itemSubscription = obj.GetComponent<BLShopItemSubscription>();
                var d = subscriptionData[idx];
                if (!itemSubscription) {
                    continue;
                }
                segment.Items[idx].SetIsEmpty(false);
                // Update Item Info
                itemSubscription.SetData(shopResource, d);
                itemSubscription.SetCallbacks(
                    () => requestSubscribe.Invoke(d),
                    () => requestUnsubscribe.Invoke(d)
                );
            }
            var subscriptionInfo = segment.ObjInfo.GetComponent<BLShopSubscriptionInfo>();
            segment.OnSelectInfo = (int idxSlotSelected) => {
                _lastIdxSubscriptionSlotSelected = idxSlotSelected;
                segment.ObjInfo.SetActive(true);
                var d = subscriptionData[idxSlotSelected];
                subscriptionInfo.SetData(d);
            };
            if (_lastIdxSubscriptionSlotSelected > 0) {
                segment.OnSelectInfo(_lastIdxSubscriptionSlotSelected);
            } else {
                segment.SelectFirstItem();
            }
        }

        public async Task UiInitEmoji(Canvas canvasDialog, IShopManager shopManager,
            IProductItemManager productItemManager,
            Action<CostumeData, CostumeData.PriceData, ProductItemData, int> requestBuyCostume) {
            var segment = (BLSegmentShopWithItem) _dicSegmentFull[TypeMenuLeftShop.Emoji];
            try {
                var itemInfo = segment.ObjInfo.GetComponent<BLShopCostumeInfo>();
                await UiInitEmojiItem(segment, itemInfo, InventoryItemType.Emoji, shopManager,
                    productItemManager, requestBuyCostume);
                segment.AutoLayoutVertical();
                segment.SelectFirstItem();
            } catch (Exception e) {
                DialogOK.ShowError(canvasDialog, e.Message);
            }
        }

        public async Task UiInitAvatar(Canvas canvasDialog, IShopManager shopManager,
            IProductItemManager productItemManager,
            Action<CostumeData, CostumeData.PriceData, ProductItemData, int> requestBuyCostume) {
            var segment = (BLSegmentShopWithItem) _dicSegmentFull[TypeMenuLeftShop.Avatar];
            try {
                var itemInfo = segment.ObjInfo.GetComponent<BLShopCostumeInfo>();
                await UiInitAvatarItem(segment, itemInfo, InventoryItemType.AvatarTR, shopManager,
                    productItemManager, requestBuyCostume);
                segment.AutoLayoutVertical();
                segment.SelectFirstItem();
            } catch (Exception e) {
                DialogOK.ShowError(canvasDialog, e.Message);
            }
        }

        public void RequestAutoLayout() {
            contentSizeFitter.AutoLayoutVertical(ReloadSegmentsLast);
        }

        public void SetSelectTab(TypeMenuLeftShop typeMenu) {
            if (DicTabs.Count <= 0) {
                return;
            }
            if (DicTabs.ContainsKey(typeMenu) && DicTabs[typeMenu].gameObject.activeSelf) {
                OnClickMenuTab(typeMenu);
                return;
            }
            foreach (var it in DicTabs.Where(it => it.Value.gameObject.activeSelf)) {
                OnClickMenuTab(it.Key);
                return;
            }
        }

        public void SetTrackTabMenuClick(Action<TypeMenuLeftShop> trackTabMenuClick) {
            _trackTabMenuClick = trackTabMenuClick;
        }

        public void ForceScrollPageAutoLayoutVertical() {
            rootSingle.SetActive(false);
            rootScroll.SetActive(true);
            contentSizeFitter.AutoLayoutVertical();
        }
        
        protected override void OnClickMenuTab(TypeMenuLeftShop typeMenu) {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            if (_dicSegmentSingles.ContainsKey(typeMenu)) {
                //Single Page
                rootSingle.SetActive(true);
                rootScroll.SetActive(false);
                foreach (var it in _dicSegmentSingles) {
                    if (it.Key is TypeMenuLeftShop.ChestP2P or TypeMenuLeftShop.ChestNormal) {
                        continue;
                    }
                    it.Value.gameObject.SetActive(it.Key == typeMenu);
                }
                
                if (typeMenu == TypeMenuLeftShop.Hero) {
                    var segment = (BLSegmentShopWithItem)_dicSegmentFull[TypeMenuLeftShop.Hero];
                    segment.AutoLayoutVertical();
                }
                
                if (typeMenu == TypeMenuLeftShop.SwapGem) {
                    _swapGem.HideChangeToken();
                }
                
            } else {
                //Scroll Page
                rootSingle.SetActive(false);
                rootScroll.SetActive(true);
                contentSizeFitter.AutoLayoutVertical();
            }
            UpdateUiTabSelect(typeMenu);
            _trackTabMenuClick?.Invoke(typeMenu);
        }

        private void ShowChestInfo(Canvas canvasDialog, TypeMenuLeftShop type) {
            if (type == TypeMenuLeftShop.ChestP2P) {
                ShowChestP2PInfo(canvasDialog);
            } else {
                ShowChestNormalInfo(canvasDialog);
            }
        }

        private void ShowChestP2PInfo(Canvas canvasDialog) {
            DialogChestInfo.Create().ContinueWith((dialog) => {
                dialog.SetTitle("P2P CHEST INFORMATION")
                    .SetDescription(
                        "Chest contain <color #F9CD00>premium skins</color> and items that <color #00FF00>can be</color> sell on p2p market in exchange for Gems.")
                    .Show(canvasDialog);
            });
        }

        private void ShowChestNormalInfo(Canvas canvasDialog) {
            DialogChestInfo.Create().ContinueWith(dialog => {
                dialog.SetTitle("NORMAL CHEST INFORMATION")
                    .SetDescription(
                        "Chest contain <color #F9CD00>premium skins</color> and items that <color #FF0000>cannot be</color> sell on p2p market, buy and play only.")
                    .Show(canvasDialog);
            });
        }
    }
}