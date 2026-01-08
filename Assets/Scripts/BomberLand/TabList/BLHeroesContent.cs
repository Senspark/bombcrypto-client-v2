using System.Collections.Generic;
using Game.Dialog;
using UnityEngine;

using Utils;

namespace Game.UI {
    public class BLHeroesContent : BaseBLContent {
        [SerializeField]
        private BaseBLHeroInformation hero;

        private List<UIHeroData> _itemHeroes = new List<UIHeroData>();

        private void Awake() {
            if (itemList == null) {
                return;
            }
            itemList.SetOnSelectItem(ChooseItem);
            hero.OnShowDialogCallback = ShowDialogHero;
            hero.OnShowDialogOrderCallback = ShowDialogOrder;
            hero.OnOrderErrorCallback = ShowDialogError;
        }

        public void ClearCacheData() {
            _itemHeroes.Clear();
        }

        public override void SetData<T>(List<T> list, bool isUpdate) {
            if (list == null) {
                return;
            }
            
            foreach (var iter in list) {
                _itemHeroes.Add(iter as UIHeroData);
            }
            // Hide ItemInformation
            hero.gameObject.SetActive(false);
            itemList.LoadData(list, isUpdate);
        }

        public void UpdateData<T>(List<T> list) {
            foreach (var iter in list) {
                _itemHeroes.Add(iter as UIHeroData);
            }
            itemList.UpdateData(list);
        }

        public int GetInputAmount() {
            return hero.GetInputAmount();
        }

        public void RefreshMinPrice() {
            itemList.RefreshMinPrice().Forget();
        }

        private void ChooseItem(int index) {
            hero.gameObject.SetActive(true);
            hero.UpdateHero(_itemHeroes[index]);
        }
        
        private void ShowDialogHero(UIHeroData heroData) {
            OnShowDialogHero?.Invoke(heroData);
        }
        private void ShowDialogOrder(OrderDataRequest orderDataRequest) {
            OnShowDialogOrder?.Invoke(orderDataRequest);
        }
        private void ShowDialogError() {
            OnOrderErrorCallback?.Invoke();
        }
    }
}