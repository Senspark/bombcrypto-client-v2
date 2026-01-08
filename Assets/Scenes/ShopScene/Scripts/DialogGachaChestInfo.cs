using System;
using System.Collections.Generic;
using System.Linq;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Game.Dialog;
using Game.Dialog.BomberLand.BLFrameShop;
using Game.Dialog.BomberLand.BLGacha;

using Senspark;

using Services;

using Share.Scripts.PrefabsManager;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Scenes.ShopScene.Scripts {
    public class DialogGachaChestInfo : Dialog {
        
        public static void Create(Canvas canvasDialog, GachaChestShopData data) {
            UniTask.Void(async () => {
                var productItemManager = ServiceLocator.Instance.Resolve<ProductItemManager>();
                var items = new List<GachaChestShopItemData>();
                foreach (var it in data.Items) {
                    var product = productItemManager.GetItem(it);
                    var item = new GachaChestShopItemData(product.Name, product.ItemId, product.Description);
                    items.Add(item);
                }
                if (data?.Items == null) {
                    return;
                }

                var dialogGachaChestInfo = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>()
                    .Instantiate<DialogGachaChestInfo>();
            
                dialogGachaChestInfo.Show(canvasDialog);
                dialogGachaChestInfo.InitData(items.ToArray());
            });
        }

        public static void Create(Canvas canvasDialog, InventoryChestData data) {
            UniTask.Void(async () => {
                if (data?.DropRate == null) {
                    return;
                }
                var dialogGachaChestInfo = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>()
                    .Instantiate<DialogGachaChestInfo>();
            
                dialogGachaChestInfo.Show(canvasDialog);
                var items = new GachaChestShopItemData[data.DropRate.Length];
                for (var i = 0; i < data.DropRate.Length; i++) {
                    var d = data.DropRate[i];
                    items[i] = new GachaChestShopItemData(d.ItemName, d.ItemId, d.ItemDescription);
                }
                items = items.Where(c => c.ProductId != (int)GachaChestProductId.Loudspeaker).ToArray();
                dialogGachaChestInfo.InitData(items);
            });
        }

        [SerializeField]
        private GameObject content;

        [SerializeField]
        private GameObject segment;

        [SerializeField]
        private GameObject prefabSlot;

        [SerializeField]
        private ContentScrollSnapHorizontal scrollSnap;

        [SerializeField]
        private BLShopCostumeInfo itemInfo;

        [SerializeField]
        private BLGachaRes resource;

        [SerializeField]
        private Button buttonOK;
        
        private BLShopSlot[] _items = null;

        private IProductItemManager _productItemManager;
        private BLGachaChestReward _curItem;
        
        protected override void Awake() {
            base.Awake();
            _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
            buttonOK.onClick.AddListener(Hide);
        }

        protected void InitData(GachaChestShopItemData[] items) {
            var numSlotPerSegment = segment.GetComponentsInChildren<BLShopSlot>().Length;
            var numSegmentAdd = (int)(items.Length / numSlotPerSegment);
            if (numSegmentAdd * numSlotPerSegment < items.Length) {
                numSegmentAdd += 1;
            }
            for (var i = 0; i < numSegmentAdd - 1; i++) {
                Instantiate(segment, segment.transform.parent, false);
            }
            var slots = content.GetComponentsInChildren<BLShopSlot>();
            if (slots.Length <= 0) {
                return;
            }
            if (slots.Length < items.Length) {
                var slotLast = slots.Last();
                var numAdd = items.Length - slots.Length;
                for (var i = 0; i < numAdd; i++) {
                    Instantiate(slotLast, slotLast.transform.parent, false);
                }
                slots = content.GetComponentsInChildren<BLShopSlot>();
            }
            var numSlot = Math.Min(items.Length, slots.Length);
            var itemDes = new BLGachaChestReward[numSlot];
            for (var idx = 0; idx < slots.Length; idx++) {
                var item = slots[idx];
                item.Index = idx;
                if (idx >= numSlot) {
                    item.SetIsEmpty(true);
                    item.gameObject.SetActive(false);
                    continue;
                }
                item.SetIsEmpty(false);
                item.OnClickItem = () => { ItemSelect(item.Index); };
                var obj = item.CreateContentByPrefab(prefabSlot);
                item.gameObject.SetActive(true);
                var gachaChestReward = obj.GetComponent<BLGachaChestReward>();
                gachaChestReward.TurnOfOnPointEnter();
                itemDes[idx] = gachaChestReward;
                if (gachaChestReward) {
                    gachaChestReward.Initialize(items[idx], () => { ItemsDesSelect(gachaChestReward); });
                }
                
                // Premium Frame
                var itemId = gachaChestReward.ProductId;
                var product = _productItemManager.GetItem(itemId);
                slots[idx].SetPremium(product.ItemKind == ProductItemKind.Premium);
            }
            ItemsDesSelect(itemDes[0]);
            _items = slots;
            scrollSnap.Layout();
        }

        /*
         * Call when item onClick
         */
        private void ItemSelect(int idxSlotSelected) {
            if (_items == null) {
                return;
            }
            if (idxSlotSelected < 0) {
                return;
            }
            if (idxSlotSelected >= _items.Length) {
                return;
            }
            for (var idx = 0; idx < _items.Length; idx++) {
                var item = _items[idx];
                item.SetSelected(idx == idxSlotSelected);
            }
        }

        private void ItemsDesSelect(BLGachaChestReward itemSelect) {
            if (_curItem == null) {
                _curItem = itemSelect;
            } else {
                if (_curItem.ProductId == itemSelect.ProductId) return;
                _curItem.SetSelected(false);
                _curItem = itemSelect;
            }
            _curItem.SetSelected(true);
            var productItem = _productItemManager.GetItem(_curItem.ProductId);
            var costume = new CostumeData {ItemId = _curItem.ProductId, Prices = Array.Empty<Data.CostumeData.PriceData>()};
            itemInfo.SetData(resource, productItem, costume, false);
        }
    }
}