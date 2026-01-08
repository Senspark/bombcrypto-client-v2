using System;
using System.Collections.Generic;
using Game.Dialog;

using Services.Server;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI {
    public abstract class BaseBLContent : MonoBehaviour {
        [SerializeField]
        private BLTabType type;

        [SerializeField]
        private Toggle toggle;

        [SerializeField]
        protected BaseBLItemList itemList;

        public BaseBLItemList ItemList => itemList;

        public Action<ItemData> OnShowDialogItem;
        public Action<UIHeroData> OnShowDialogHero;
        public Action<OrderDataRequest> OnShowDialogOrder;
        public Action OnOrderErrorCallback;

        public BLTabType Type => type;

        public void SetSelected(bool value) {
            toggle.isOn = value;
            gameObject.SetActive(value);
        }

        public void OnSelect(bool isSelect) {
            gameObject.SetActive(isSelect);
        }

        public void SetEnableShowMore(Action<bool> callback) {
            itemList.SetEnableShowMore(callback);
        }

        public void UpdatePage() {
            itemList.UpdatePage();
            itemList.UpdatePageData(true);
        }
        
        public void SetPageData(int quantity) {
            if (quantity > 0) {
                itemList.SetCurPage(1);
                itemList.SetMaxPage((quantity - 1) / itemList.MaxItems + 1);
            } else {
                itemList.SetCurPage(0);
                itemList.SetMaxPage(0);
            }
        }

        public abstract void SetData<T>(List<T> list, bool isUpdate);
    }
}