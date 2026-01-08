using Data;

using Senspark;

using Services;

using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Dialog.BomberLand.BLGacha {
    public class BLGachaChestInfo : MonoBehaviour {
        
        [SerializeField]
        private BLItemLoopHighlight highlight;

        [SerializeField]
        private BLGachaChestItem itemPrefab;

        [SerializeField]
        private Transform layout;

        private readonly IGachaItemManager _itemManager = ServiceLocator.Instance.Resolve<IGachaItemManager>();

        public void Initialize(params GachaChestItemData[] items) {
            foreach (var it in items) {
                var item = Instantiate(itemPrefab, layout);
                item.Initialize(
                    (instant, productId) =>
                        highlight.UpdateTarget(instant.transform, _itemManager.GetItem(productId).Description), it);
            }
        }
    }
}