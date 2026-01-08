using System.Collections.Generic;
using System.Linq;

using Constant;

using Cysharp.Threading.Tasks;

using Data;

using Senspark;

using Services;

using Share.Scripts.PrefabsManager;

using UnityEngine;

namespace Scenes.ShopScene.Scripts {
    public class DialogGachaChestInfoPad : DialogGachaChestInfo {
        
        public new static void Create(Canvas canvasDialog, GachaChestShopData data) {
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
                    .Instantiate<DialogGachaChestInfoPad>();
            
                dialogGachaChestInfo.Show(canvasDialog);
                dialogGachaChestInfo.InitData(items.ToArray());
            });
        }

        public new static void Create(Canvas canvasDialog, InventoryChestData data) {
            UniTask.Void(async () => {
                if (data?.DropRate == null) {
                    return;
                }
                var dialogGachaChestInfo = await ServiceLocator.Instance.Resolve<IPrefabLoaderManager>()
                    .Instantiate<DialogGachaChestInfoPad>();
            
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
    }
}