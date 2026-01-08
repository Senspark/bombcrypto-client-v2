using Data;
using Engine.Utils;
using Senspark;
using Game.Dialog;
using Game.Dialog.BomberLand.BLGacha;
using Game.UI.Custom;
using Services;
using UnityEngine;

namespace Game.UI {
    public class BLInventoryAvatarInformation : BaseBLItemInformation {
        [SerializeField]
        private GameObject buttonSelect;

        [SerializeField]
        private CustomContentSizeFitter contentSizeFitter;
        
        [SerializeField]
        private ImageAnimation avatarTR;
        
        [SerializeField]
        private GameObject premiumFrame;
        
        [SerializeField]
        private BLGachaRes resource;
        
        [SerializeField]
        private BLProfileCard profileCard;
        
        private IProductItemManager _productItemManager;

        private IProductItemManager ProductItemManager {
            get {
                if (_productItemManager != null) {
                    return _productItemManager;
                }
                _productItemManager = ServiceLocator.Instance.Resolve<IProductItemManager>();
                return _productItemManager;
            }
        }

        private void Awake() {
            if (profileCard != null) {
                profileCard.TryLoadData();
            }
        }

        public override async void UpdateItem(ItemData data, bool showButton = true) {
            ItemData = data;
            gameObject.SetActive(true);
            idText.text = data.ItemName;
            if (effectPremium) 
            {
                var productItem = ProductItemManager.GetItem(data.ItemId);
                effectPremium.enabled = false;
                idText.color = productItem.ItemKind == ProductItemKind.Premium
                    ? effectPremium.m_OverlayColor : Color.white;
            }
            SetEnableSelect(!data.Equipped);
            
            var sprites = await resource.GetAvatar(data.ItemId);
            avatarTR.StartAni(sprites);
            
            if (premiumFrame) {
                premiumFrame.SetActive(ProductItemManager.GetItem(data.ItemId).ItemKind == ProductItemKind.Premium);
            }
            
            if (contentSizeFitter) {
                contentSizeFitter.ForceSnapVertical();
            }
        }

        private void SetEnableSelect(bool value) {
            buttonSelect.SetActive(value);
        }
    }
}