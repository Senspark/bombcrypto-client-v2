using System;
using App;
using Data;

using JetBrains.Annotations;

using Scenes.MarketplaceScene.Scripts;

using Senspark;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLInventoryItemCostume : BaseBLItem {
        [SerializeField]
        private Text nameText;
        [SerializeField] [CanBeNull] private Text durationText;
        
        [SerializeField]
        public OverlayTexture effectPremium;

        [SerializeField]
        private GameObject iconCanNotSell;

        private IProductItemManager _productItemManager;

        [SerializeField]
        private GameObject tagNew;
        
        private bool isNew = false;
        private int itemId = 0;
        private IServerRequester _serverRequester;

        private void Awake() {
            _serverRequester = ServiceLocator.Instance.Resolve<IServerRequester>();
        }

        public override void SetInfo<T>(int index, T itemData, Action<int> callback) {
            if(durationText != null)
                durationText.gameObject.SetActive(false);
            Index = index;
            OnClickCallback = (i) => {
                // if is new, set to old
                if (isNew) {
                    tagNew.gameObject.SetActive(false);
                    _serverRequester.MarkItemViewed(itemId);
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
                    isNew = hero.IsNew;
                    itemId = hero.HeroId;
                    tagNew.gameObject.SetActive(isNew);
                    break;
                case ItemData item:
                    avatar.ChangeAvatar(item);
                    valueText.text = $"x{item.Quantity}";
                    if (durationText != null && item.ExpirationAfter > 0) {
                        durationText.gameObject.SetActive(true);
                        durationText.text =
                            $"Duration: <color=#33ff00>{MarketUtils.ExpirationToDays(item.ExpirationAfter)} days</color>";
                    }
                    iconCanNotSell.SetActive(!item.Sellable);
                    isNew = item.IsNew;
                    itemId = item.ItemId;
                    tagNew.gameObject.SetActive(isNew);
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