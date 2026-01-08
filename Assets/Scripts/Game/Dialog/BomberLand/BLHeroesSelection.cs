using System;
using System.Collections.Generic;
using System.Linq;

using App;

using Engine.Input.ControllerNavigation;

using JetBrains.Annotations;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialog {
    public struct HeroGroupData {
        public PlayerData PlayerData;
        public int Quantity;
    }

    public class BLHeroesSelection : MonoBehaviour {
        [SerializeField]
        private ScrollRect scroller;

        [SerializeField]
        private Transform container;
        [SerializeField] [CanBeNull] private PopupParentNavigate popupParentNavigate;


        [SerializeField]
        private BLHeroSelectionItem itemPrefab;

        private Dictionary<int, BLHeroSelectionItem> _items;

        private Action<int, bool> _onSelectItem;

        public void SetOnSelectItem(Action<int, bool> callback) {
            _onSelectItem = callback;
        }

        public void LoadHeroes(List<HeroGroupData> heroes) {
            _items = new Dictionary<int, BLHeroSelectionItem>();
            foreach (Transform child in container) {
                Destroy(child.gameObject);
            }

            var num = heroes.Count;
            for (var i = 0; i < num; i++) {
                var item = Instantiate(itemPrefab, container, false);
                var hero = heroes[i].PlayerData;
                var quantity = heroes[i].Quantity;
                item.SetInfo(i, hero, quantity, OnHeroesItemClicked);
                _items[i] = item;
            }

            if (popupParentNavigate != null) {
                var listChild = _items
                    .Select(item => item.Value.GetComponent<ChildrenNavigate>())
                    .Where(child => child != null)
                    .ToList();
                popupParentNavigate.Register(listChild);
            }
        }

        public void SetSelectItem(HeroId heroId) {
            if (_items.Count == 0) {
                return;
            }
            var index = -1;
            var keys = _items.Keys;
            foreach (var key in keys.Where(key => heroId == _items[key].HeroId)) {
                index = _items[key].Index;
            }
            if (index < 0) {
                index = 0;
            }
            ScrollTo(index);
            OnHeroesItemClicked(index, false);
        }

        public void SetSelectedItem(HeroId heroId) {
            var index = -1;
            var keys = _items.Keys;
            foreach (var key in keys.Where(key => heroId == _items[key].HeroId)) {
                index = _items[key].Index;
            }
            if (index >= 0) {
                ScrollTo(index);
                OnHeroSelected(index);
            }
        }

        private void ScrollTo(int itemIndex) {
            var grid = scroller.content.GetComponent<GridLayoutGroup>();
            var columnRow = GridLayoutGroupUtil.GetColumnAndRow(grid);

            var itemCount = _items.Count;
            if (itemCount == 0 || columnRow.x == 0 || columnRow.y == 0) {
                return;
            }

            itemIndex = Mathf.Clamp(itemIndex, 0, itemCount - 1);
            var row = (itemIndex) / columnRow.x;
            var normal = (float) row / (columnRow.y - 1);
            scroller.verticalNormalizedPosition = 1f - normal;
        }

        private void OnHeroesItemClicked(int index, bool isSound) {
            var keys = _items.Keys;
            foreach (var key in keys) {
                _items[key].SetActive(false);
            }
            _items[index].SetActive(true);
            _onSelectItem?.Invoke(index, isSound);
        }

        private void OnHeroSelected(int index) {
            var keys = _items.Keys;
            foreach (var key in keys) {
                _items[key].SetSelected(false);
            }
            _items[index].SetSelected(true);
        }
    }
}