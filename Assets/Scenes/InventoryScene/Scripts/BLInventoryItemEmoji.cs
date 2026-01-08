using System;
using App;
using Senspark;
using Services;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public class BLInventoryItemEmoji : BaseBLItem {
        [SerializeField]
        private Text nameText;
        
        [SerializeField]
        private GameObject tagNew;

        private IProductItemManager _productItemManager;
        private bool isNew = false;
        private int itemId = 0;
        private IServerRequester _serverRequester;

        
        public override void SetInfo<T>(int index, T itemData, Action<int> callback) {
            Index = index;
            OnClickCallback = (i) => {
                // if is new, set to old
                if (isNew) {
                    tagNew.gameObject.SetActive(false);
                    _serverRequester ??= ServiceLocator.Instance.Resolve<IServerRequester>();
                    _serverRequester.MarkItemViewed(itemId);
                }
                callback(index);
            };
            avatar.ChangeAvatar(itemData);
            if (itemData is ItemData item) {
                nameText.text = item.ItemName;
                isNew = item.IsNew;
                itemId = item.ItemId;
                tagNew.gameObject.SetActive(isNew);
            }
        }
    }
}