using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App;
using DG.Tweening;
using Game.Dialog;
using Game.UI.Custom;
using Senspark;

using Services.Server;

using UnityEngine;
using UnityEngine.UI;

using Utils;

namespace Game.UI {
    public abstract class BaseBLItemList : MonoBehaviour {
        [SerializeField]
        private GameObject noItemText;

        [SerializeField]
        protected Transform container;

        [SerializeField]
        protected BaseBLItem itemPrefab;
        
        [SerializeField]
        protected ScrollRect scrollRect;
        
        [SerializeField]
        private GameObject visitBtn;
        
        private const int PageRow = 5;
        private const float BOTTOM_OFFSET = 10f;
        
        private List<BaseBLItem> _items = new List<BaseBLItem>();
        private int _currentIndex;
        private Action<int> _onSelectItem;
        private Action<bool> _onEnableShowMore;
        private bool _lastShowMoreState = false;
        
        public int CurPage { get; private set; }
        public int MaxPage { get; private set; }
        
        public int MaxItems { get; private set; }

        private ISoundManager _soundManager;
        private IMarketplace _marketplace;
        private int _pageItems;

        private void Awake() {
            _soundManager = ServiceLocator.Instance.Resolve<ISoundManager>();
            _marketplace = ServiceLocator.Instance.Resolve<IServerManager>().Marketplace;
            
            if (scrollRect != null) {
                scrollRect.onValueChanged.AddListener(OnScroll);
            }
        }

        public void LoadData<T>(List<T> list, bool isUpdate) {
            if (_items.Count == 0 || isUpdate) {
                noItemText.SetActive(list.Count == 0);
                if (visitBtn != null) {
                    visitBtn.SetActive(list.Count == 0);
                }
                _currentIndex = 0;
                ResetScroll();
                CreateItemSlots(list, isUpdate);
                UnSelectedAll();
                UpdatePageData(false);
            }
            
            if (_items.Count > 0) {
                _items[_currentIndex].SetSelected(true);
                _onSelectItem?.Invoke(_currentIndex);
            }
            
            _onEnableShowMore?.Invoke(_lastShowMoreState);
        }

        public void UpdateData<T>(List<T> list, bool isUpdateIndex = false) {
            CreateItemSlots(list, isUpdateIndex);
            UnSelectedAll();
            
            if (_items.Count > 0) {
                _items[_currentIndex].SetSelected(true);
                _onSelectItem?.Invoke(_currentIndex);
            }
            if (!isUpdateIndex) {
                ScrollToShowNextRow();
            }
        }

        public async Task RefreshMinPrice() {
            foreach (var item in _items) {
                var minPrice = await _marketplace.GetCurrentUserMinPrice(item.itemId);
                item.UpdateMinPrice(minPrice);
            }
        }
        
        public void UpdateLocalData<T>(List<T> list) {
            for (var i = 0; i < _items.Count; i++) {
                var data = list[i];
                var index = i + (CurPage - 1) * MaxItems;
                _items[i].SetInfo(index, data, OnItemClicked);
            }
            
            if (_items.Count > 0) {
                _items[_currentIndex].SetSelected(true);
                _onSelectItem?.Invoke(_currentIndex);
            }
        }

        public void SetOnSelectItem(Action<int> callback) {
            _onSelectItem = callback;
        }
        
        public void SetEnableShowMore(Action<bool> callback) {
            _onEnableShowMore = callback;
        }

        private void OnItemClicked(int index) {
            _soundManager.PlaySound(Audio.Tap);
            _currentIndex = index;
            UnSelectedAll();
            _items[_currentIndex].SetSelected(true);
            _onSelectItem?.Invoke(_currentIndex);
        }

        private void UnSelectedAll() {
            foreach (var iter in _items) {
                iter.SetSelected(false);
            }
        }

        public void SetSelected(int index, bool value) {
            _items[index].SetSelected(value);
        }

        private void CreateItemSlots<T>(List<T> dataList, bool isUpdate) {
            if (isUpdate) {
                _items.Clear();
                foreach (Transform child in container) {
                    Destroy(child.gameObject);
                }
            }
            
            for (var i = 0; i < dataList.Count; i++) {
                var newItem = Instantiate(itemPrefab, container, false);
                var data = dataList[i];
                var index = i + (CurPage - 1) * MaxItems;
                newItem.SetInfo(index, data, OnItemClicked);
                newItem.SetInvisible(true);
                _items.Add(newItem);
            }

            RefreshMinPrice().Forget();
        }
        
        private void OnScroll(Vector2 scrollPosition) {
            if (_lastShowMoreState != IsAtBottom()) {
                _lastShowMoreState = IsAtBottom() && CurPage < MaxPage;
                _onEnableShowMore?.Invoke(_lastShowMoreState);
            }
        }

        private bool IsAtBottom() {
            var contentHeight = scrollRect.content.rect.height;
            var viewportHeight = scrollRect.viewport.rect.height;
            var contentY = scrollRect.content.anchoredPosition.y;
            return (contentHeight - viewportHeight - contentY) < BOTTOM_OFFSET;
        }
        
        private void ResetScroll() {
            if (scrollRect != null) {
                var contentX = scrollRect.content.anchoredPosition.x;
                scrollRect.content.anchoredPosition = new Vector2(contentX, 0);
            }
        }

        public void UpdatePage() {
            if (MaxPage == 0) return;
            CurPage++;
        }

        public void SetCurPage(int value) {
            CurPage = value;
        }

        public void SetMaxPage(int value) {
            MaxPage = value;
        }

        public void UpdatePageData(bool isUpdate) {
            var totalVisibleItems = CurPage * MaxItems;
            for (var i = 0; i < _items.Count; i++) {
                _items[i].SetInvisible(i < totalVisibleItems);
            }
            if (!isUpdate) {
                return;
            }
            ScrollToShowNextRow();
        }

        private void ScrollToShowNextRow() {
            var deltaY = container.GetComponent<GridLayoutGroup>().cellSize.y;
            var contentPos = container.GetComponent<RectTransform>().anchoredPosition;
            var step = contentPos.y;
            var stepTo = contentPos.y += deltaY;
            DOTween.To(() => step,
                MoveContainerTo,
                stepTo, 0.5f);
        }

        private void MoveContainerTo(float stepTo) {
            var contentPos = container.GetComponent<RectTransform>().anchoredPosition;
            contentPos.y = stepTo;
            container.GetComponent<RectTransform>().anchoredPosition = contentPos;
        }

        // Dirty fix: bỏ ko phân trang nữa
        public async Task SetMaxItems() {
            MaxItems = 999;
            // if (MaxItems != 0) {
            //     return;
            // }
            //
            // var fixGridLayoutGroup = container.GetComponent<FixGridLayoutGroup>();
            // if (fixGridLayoutGroup != null) {
            //     MaxItems = await fixGridLayoutGroup.GetColumn() * PageRow;
            // } else {
            //     var gridLayoutGroup = container.GetComponent<GridLayoutGroup>();
            //     MaxItems = gridLayoutGroup.constraintCount;
            // }
        }
    }
}