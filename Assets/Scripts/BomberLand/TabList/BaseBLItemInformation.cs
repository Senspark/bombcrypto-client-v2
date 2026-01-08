using System;

using App;

using BomberLand.Inventory;

using Data;

using Senspark;

using Game.Dialog;

using Services;
using Services.Server;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public class BaseBLItemInformation : MonoBehaviour {
        [SerializeField]
        protected Text idText;

        [SerializeField]
        protected OverlayTexture effectPremium;

        [SerializeField]
        private BLTablListAvatar avatar;

        [SerializeField]
        private Transform stats;

        [SerializeField]
        private BLInventoryStat statPrefab;

        public Action<ItemData> OnShowDialogCallback;

        protected ItemData ItemData;
        private IProductItemManager _productItemManager;
        
        public Action<OrderDataRequest> OnShowDialogOrderCallback;
        public Action OnOrderErrorCallback;

        private IProductItemManager ProductItemManager {
            get {
                if (_productItemManager != null) {
                    return _productItemManager;
                }
                _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                return _productItemManager;
            }
        }

        public virtual int GetInputAmount() {
            return 0;
        }

        public virtual void UpdateItem(ItemData data, bool showButton = true) {
            ItemData = data;
            if (data == null) {
                gameObject.SetActive(false);
                idText.text = "--";
                avatar.gameObject.SetActive(false);
                return;
            }
            gameObject.SetActive(true);
            avatar.gameObject.SetActive(true);
            // idText.text = ProductItemManager.GetItem(data.ItemId).Name;
            idText.text = data.ItemName;
            avatar.ChangeAvatar(data);

            if (stats) {
                ClearStats();
                if (data.Stats != null) {
                    foreach (var stat in data.Stats) {
                        var statItem = Instantiate(statPrefab, stats, false);
                        statItem.SetInfo(stat.StatId, stat.Value);
                    }
                }
                stats.gameObject.SetActive(data.Stats != null && data.Stats.Length > 0);
            }
            if (effectPremium) {
                var productItem = ProductItemManager.GetItem(data.ItemId);
                effectPremium.enabled = false;
                idText.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
        }

        private void ClearStats() {
            if (!stats) {
                return;
            }
            foreach (Transform child in stats) {
                Destroy(child.gameObject);
            }
        }

        public void OnButtonClicked() {
            ServiceLocator.Instance.Resolve<ISoundManager>().PlaySound(Audio.Tap);
            OnShowDialogCallback?.Invoke(ItemData);
        }
    }
}