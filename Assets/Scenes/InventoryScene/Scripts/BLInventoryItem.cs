using System;
using App;
using Data;
using Senspark;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLInventoryItem : BaseBLItem {
        [SerializeField]
        private Text nameText;
        
        [SerializeField]
        public OverlayTexture effectPremium;

        [SerializeField]
        private GameObject iconCanNotSell;

        private IProductItemManager _productItemManager;

        [SerializeField]
        private GameObject tagNew;
        
        private bool _isNew = false;
        private int _itemId = 0;
        private IServerRequester _serverRequester;

        private void Awake() {
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
        }

        public override void SetInfo<T>(int index, T itemData, Action<int> callback) {
            Index = index;
            OnClickCallback = (i) => {
                // if is new, set to old
                if (_isNew) {
                    tagNew.gameObject.SetActive(false);
                    _serverRequester.MarkItemViewed(_itemId);
                }
                callback(index);
            };
            avatar.ChangeAvatar(itemData);
            iconCanNotSell.SetActive(false);
            switch (itemData) {
                case UIHeroData hero:
                    valueText.text = $"x{hero.Quantity}";
                    iconCanNotSell.SetActive(!hero.Sellable);
                    if (nameText) {
                        nameText.text = hero.HeroName;
                    }
                    if (effectPremium) {
                        _productItemManager ??= ServiceLocator.Instance.Resolve<IProductItemManager>();
                        var productItem = _productItemManager.GetItem(hero.HeroId);
                        effectPremium.enabled = false;
                        nameText.color = productItem.ItemKind == ProductItemKind.Premium
                            ? effectPremium.m_OverlayColor : Color.white;
                    }
                    _isNew = hero.IsNew;
                    _itemId = hero.HeroId;
                    tagNew.gameObject.SetActive(_isNew);
                    break;
                case ItemData item:
                    avatar.ChangeAvatar(item);
                    valueText.text = $"x{item.Quantity}";
                    iconCanNotSell.SetActive(!item.Sellable);
                    _isNew = item.IsNew;
                    _itemId = item.ItemId;
                    tagNew.gameObject.SetActive(_isNew);
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
                    break;
                case InventoryChestData chest:
                    avatar.ChangeAvatar(chest);
                    nameText.text = $"{chest.ChestName}";
                    break;
            }
        }
    }
}