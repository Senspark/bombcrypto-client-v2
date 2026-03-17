using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using DynamicScrollRect;
using Game.Dialog;
using Scenes.FarmingScene.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI {
    [RequireComponent(typeof(ScrollRect))]
    public class InfiniteInventoryScroller : MonoBehaviour {
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;
        [SerializeField] private DynamicInventoryItem _itemPrefab;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private RectTransform _viewport;

        private List<PlayerData> _dataList = new List<PlayerData>();
        private List<DynamicInventoryItem> _activePool = new List<DynamicInventoryItem>();
        private Dictionary<DynamicInventoryItem, PlayerData> _currentItemMap = new Dictionary<DynamicInventoryItem, PlayerData>();

        private int _totalItems;
        private int _columns;
        private float _itemWidth;
        private float _itemHeight;
        private float _spacingY;
        private float _spacingX;

        private int _topVisibleRow;
        private int _bottomVisibleRow;
        private int _visibleRowCount;

        private Action<int> _onScrollToBottom;
        private Action<DynamicInventoryItem> _onItemClicked;
        private Action<DynamicInventoryItem> _onItemHover;

        private DialogInventory.ChooseMode _chooseMode;
        private List<PlayerData> _heroesIdBurn;
        private InventoryItem.InventoryItemCallback _inventoryCallback;
        private Canvas _dialogCanvas;
        private HeroDetailsDisplay _heroDescriptionPanel;
        private bool _isShowLockHero;
        private DialogInventory.ActiveFilter _curActiveFilter;
        private List<int> _heroesBurnIds;

        private bool _isInitialized = false;

        public void Init(ScrollRect scrollRect, RectTransform content, DynamicInventoryItem itemPrefab,
            GridLayoutGroup gridLayout, RectTransform viewport,
            Action<int> onScrollToBottom, DialogInventory.ChooseMode chooseMode,
            List<PlayerData> heroesIdBurn, List<int> heroesBurnIds, InventoryItem.InventoryItemCallback inventoryCallback,
            Canvas dialogCanvas, HeroDetailsDisplay heroDescriptionPanel, bool isShowLockHero, DialogInventory.ActiveFilter curActiveFilter) {

            _scrollRect = scrollRect;
            _content = content;
            _itemPrefab = itemPrefab;
            _gridLayout = gridLayout;
            _viewport = viewport;

            _onScrollToBottom = onScrollToBottom;
            _chooseMode = chooseMode;
            _heroesIdBurn = heroesIdBurn;
            _heroesBurnIds = heroesBurnIds;
            _inventoryCallback = inventoryCallback;
            _dialogCanvas = dialogCanvas;
            _heroDescriptionPanel = heroDescriptionPanel;
            _isShowLockHero = isShowLockHero;
            _curActiveFilter = curActiveFilter;

            if (!_isInitialized) {
                _isInitialized = true;
                if (_scrollRect == null) _scrollRect = GetComponent<ScrollRect>();
                _scrollRect.onValueChanged.AddListener(OnScroll);

                if (_gridLayout != null) {
                    _itemWidth = _gridLayout.cellSize.x;
                    _itemHeight = _gridLayout.cellSize.y;
                    _spacingX = _gridLayout.spacing.x;
                    _spacingY = _gridLayout.spacing.y;
                    _columns = Mathf.FloorToInt((_viewport.rect.width + _spacingX) / (_itemWidth + _spacingX));
                    if (_columns <= 0) _columns = 1;
                    _gridLayout.enabled = false;
                }
            }
        }

        public void SetData(List<PlayerData> data, bool isAppend = false) {
            if (!isAppend) {
                _dataList = data;
                _currentItemMap.Clear();
                _scrollRect.verticalNormalizedPosition = 1f;
            } else {
                _dataList = data; // Usually the provided data encompasses everything, pagination fetch appended at StoreManager level. So just use updated list
            }

            _totalItems = _dataList.Count;
            UpdateContentSize();
            UpdateVisibleItems();
        }

        private void UpdateContentSize() {
            int rows = Mathf.CeilToInt((float)_totalItems / _columns);
            float height = rows * _itemHeight + Mathf.Max(0, rows - 1) * _spacingY;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
        }

        private void OnScroll(Vector2 pos) {
            UpdateVisibleItems();

            if (pos.y <= 0.05f && _onScrollToBottom != null && _totalItems > 0) {
                _onScrollToBottom.Invoke(_totalItems);
            }
        }

        private void OnDestroy() {
            if (_scrollRect != null) {
                _scrollRect.onValueChanged.RemoveListener(OnScroll);
            }
        }

        private void UpdateVisibleItems() {
            if (_totalItems == 0) {
                foreach (var item in _activePool) {
                    item.gameObject.SetActive(false);
                }
                return;
            }

            float scrollY = _content.anchoredPosition.y;
            float viewportHeight = _viewport.rect.height;

            _topVisibleRow = Mathf.FloorToInt(scrollY / (_itemHeight + _spacingY));
            _visibleRowCount = Mathf.CeilToInt(viewportHeight / (_itemHeight + _spacingY)) + 1;
            _bottomVisibleRow = _topVisibleRow + _visibleRowCount;

            _topVisibleRow = Mathf.Max(0, _topVisibleRow - 1); // buffer
            _bottomVisibleRow = Mathf.Min(Mathf.CeilToInt((float)_totalItems / _columns) - 1, _bottomVisibleRow + 1);

            int startItemIndex = _topVisibleRow * _columns;
            int endItemIndex = Mathf.Min(_totalItems - 1, (_bottomVisibleRow + 1) * _columns - 1);
            int visibleItemCount = endItemIndex - startItemIndex + 1;

            while (_activePool.Count < visibleItemCount) {
                var inst = Instantiate(_itemPrefab, _content);
                inst.GetComponent<RectTransform>().pivot = new Vector2(0, 1);
                inst.GetComponent<RectTransform>().anchorMin = new Vector2(0, 1);
                inst.GetComponent<RectTransform>().anchorMax = new Vector2(0, 1);
                _activePool.Add(inst);
            }

            for (int i = 0; i < _activePool.Count; i++) {
                if (i < visibleItemCount) {
                    int itemIndex = startItemIndex + i;
                    var itemData = _dataList[itemIndex];
                    var itemObj = _activePool[i];
                    itemObj.gameObject.SetActive(true);

                    int row = itemIndex / _columns;
                    int col = itemIndex % _columns;
                    float posX = col * (_itemWidth + _spacingX);
                    float posY = -(row * (_itemHeight + _spacingY));
                    itemObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, posY);

                    // Prevent massive performance regressions on every frame of scroll
                    if (!_currentItemMap.TryGetValue(itemObj, out var currentData) || currentData != itemData) {
                        _currentItemMap[itemObj] = itemData;
                        // Awaitable operation needs to be handled cleanly in UI scroll, so wrap in Forget
                        UpdateItemInfoAsync(itemObj, itemData).Forget();
                    }
                } else {
                    _activePool[i].gameObject.SetActive(false);
                }
            }
        }

        private async UniTask UpdateItemInfoAsync(DynamicInventoryItem item, PlayerData player) {
            var heroesIdBurn = await item.SetInfo(player, _inventoryCallback, _chooseMode, _heroesIdBurn,
                        _chooseMode == ChooseMode.PvpMode,
                        _heroesBurnIds.Contains(player.heroId.Id), canvas: _dialogCanvas, heroDetailsDisplay: _heroDescriptionPanel);

            _heroesIdBurn = heroesIdBurn;

            item.UpdateLockedHeroes(_curActiveFilter == DialogInventory.ActiveFilter.Locked);
            if (_isShowLockHero) {
                item.UpdateUILockHero(player);
            }
        }

        public void ScrollTo(int index) {
            int row = index / _columns;
            float totalRows = Mathf.CeilToInt((float)_totalItems / _columns);
            float normal = 1f - ((float)row / Mathf.Max(1, totalRows - 1));
            _scrollRect.verticalNormalizedPosition = normal;
        }

        public List<DynamicInventoryItem> GetActiveItems() {
            return _activePool;
        }
    }
}
