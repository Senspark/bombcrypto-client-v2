using System;
using App;
using Data;
using Engine.Utils;
using Game.Dialog.BomberLand.BLGacha;
using Senspark;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLInventoryItemAvatar : BaseBLItem {
        [SerializeField]
        private Text nameText;
        
        [SerializeField]
        public OverlayTexture effectPremium;
        
        [SerializeField]
        private ImageAnimation avatarTR;
        
        [SerializeField]
        private Text eSelected;
        
        [SerializeField]
        private GameObject tagNew;
        
        [SerializeField]
        private BLGachaRes resource;
        
        private IProductItemManager _productItemManager;
        private bool _isNew = false;
        private int _itemId = 0;
        private IServerRequester _serverRequester;

        public override async void SetInfo<T>(int index, T itemData, Action<int> callback) {
            Index = index;
            OnClickCallback = (i) => {
                // if is new, set to old
                if (_isNew) {
                    tagNew.gameObject.SetActive(false);
                    _serverRequester ??= ServiceLocator.Instance.Resolve<IServerRequester>();
                    _serverRequester.MarkItemViewed(_itemId);
                }
                callback(index);
            };
            if (itemData is ItemData item) {
                if (nameText) {
                    nameText.text = item.ItemName;
                }
                if (effectPremium) {
                    _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
                    var productItem = _productItemManager.GetItem(item.ItemId);
                    effectPremium.enabled = false;
                    nameText.color = productItem.ItemKind == ProductItemKind.Premium
                        ? effectPremium.m_OverlayColor : Color.white;
                }
                _isNew = item.IsNew;
                _itemId = item.ItemId;
                tagNew.gameObject.SetActive(_isNew);
                
                var sprites = await resource.GetAvatar(item.ItemId);
                avatarTR.StartAni(sprites);
                
                eSelected.gameObject.SetActive(item.Equipped);
            }
        }
    }
}