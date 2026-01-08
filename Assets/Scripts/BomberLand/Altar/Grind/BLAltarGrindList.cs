using System;
using System.Collections.Generic;
using Data;
using Game.Dialog;
using UnityEngine;
using UnityEngine.UI;

public class BLAltarGrindList : MonoBehaviour {
    [SerializeField]
    private Transform container;

    [SerializeField]
    private BLAltarGrindItem itemPrefab;
    
    [SerializeField]
    private ScrollRect scrollRect;

    private const int PageItems = 20;
    private const float BOTTOM_OFFSET = 10f;

    private readonly Dictionary<int, BLAltarGrindItem> _dictItems = new Dictionary<int, BLAltarGrindItem>();
    private Action<int> _onSelectItem;
    private Action<bool> _onEnableShowMore;
    private bool _lastShowMoreState = false;
    
    public int CurPage { get; private set; }
    public int MaxPage { get; private set; }

    private void Awake() {
        if (scrollRect != null) {
            scrollRect.onValueChanged.AddListener(OnScroll);
        }
    }
    
    public void SetOnSelectItem(Action<int> callback) {
        _onSelectItem = callback;
    }
    
    public void LoadData(List<TRHeroData> list) {
        _dictItems.Clear();
        foreach (Transform child in container) {
            Destroy(child.gameObject);
        }

        var num = list.Count;
        for (var i = 0; i < num; i++) {
            var item = Instantiate(itemPrefab, container, false);
            var hero = list[i];
            item.SetInfo(i, hero, OnItemClicked);
            _dictItems[i] = item;
        }
        
        MaxPage = list.Count > 0 ? (list.Count - 1) / PageItems + 1 : 0;
        CurPage = list.Count > 0 ? 1 : 0;
        UpdatePageData();
        _onEnableShowMore?.Invoke(_lastShowMoreState);
    }

    public void SetSelectItem(int index) {
        OnItemClicked(index);
    }
    
    private void OnItemClicked(int index) {
        var keys = _dictItems.Keys;
        foreach (var key in keys) {
            _dictItems[key].SetSelected(false);
        }
        _dictItems[index].SetSelected(true);
        _onSelectItem?.Invoke(index);
    }
    
    private void OnScroll(Vector2 scrollPosition)
    {
        if (_lastShowMoreState != IsAtBottom()) {
            _lastShowMoreState = IsAtBottom() && CurPage < MaxPage;
            _onEnableShowMore?.Invoke(_lastShowMoreState);
        }
    }

    private bool IsAtBottom()
    {
        var contentHeight = scrollRect.content.rect.height;
        var viewportHeight = scrollRect.viewport.rect.height;
        var contentY = scrollRect.content.anchoredPosition.y;
        return (contentHeight - viewportHeight - contentY) < BOTTOM_OFFSET;
    }
    
    public void SetEnableShowMore(Action<bool> callback) {
        _onEnableShowMore = callback;
    }
        
    public void UpdatePage() {
        if (MaxPage == 0) return;
        CurPage++;
        UpdatePageData();
    }

    private void UpdatePageData() {
        var totalVisibleItems = CurPage * PageItems;
        for (var i = 0; i < _dictItems.Count; i++) {
            _dictItems[i].SetInvisible(i < totalVisibleItems);
        }
    }
}