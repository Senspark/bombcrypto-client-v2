using System;
using System.Collections.Generic;

using pooling;

using DG.Tweening;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.Dialog {
    [Flags]
    public enum ScrollDirection {
        None = 0x1,
        Left = 0x2,
        Right = 0x4,
        Up = 0x8,
        Down = 0x10
    }

    public class DynamicScroll<T, T1>
        where T : class
        where T1 : DynamicScrollObject<T> {
        private float _spacingX = 0;
        private float _spacingY = 0;
        
        private readonly Pooling<T1> _objectPool = new Pooling<T1>();

        private DynamicScrollRect.DynamicScrollRect ScrollRect { set; get; }

        public Action<Vector2> OnDragEvent { set; private get; }
        public UnityEvent<PointerEventData> OnBeginDragEvent { set; private get; }
        public UnityEvent<PointerEventData> OnEndDragEvent { set; private get; }

        private VerticalLayoutGroup _verticalLayoutGroup;
        private HorizontalLayoutGroup _horizontalLayoutGroup;
        private GridLayoutGroup _gridLayoutGroup;
        private ContentSizeFitter _contentSizeFitter;

        private bool _isVertical = false;
        private bool _isHorizontal = false;
        private bool _isDragging = false;

        private ScrollDirection _lastInvalidDirections;
        private ScrollRect.MovementType _movementType;

        private Vector2 _newAnchoredPosition = Vector2.zero;
        private Vector2 _scrollVelocity = Vector2.zero;
        private Vector2 _lastPos = Vector2.zero;
        private Vector2 _clampedPosition = Vector2.zero;
        private IList<T> _infoList;
        private Tween _forceMoveTween;

        private int Column { set; get; } = 1;
        private float _firstPosX;
        public bool Initiated { get; private set; }
        public int SelectId { private get; set; } = -1;

        public void Initiate(DynamicScrollRect.DynamicScrollRect scrollRect, IList<T> infoList, int startIndex, GameObject objReference,
            bool createMoreIfNeeded = true, int? forceAmount = null) {
            ScrollRect = scrollRect;
            if (ScrollRect == null)
                throw new Exception("No scroll rect in gameObject.");

            if (objReference == null)
                throw new Exception("No Reference GameObject has set.");

            if (startIndex >= infoList.Count)
                throw new Exception("Invalid index: " + startIndex);

            _infoList = infoList;

            ScrollRect.onValueChanged.AddListener(OnScroll);
            ScrollRect.onBeginDrag.AddListener(OnBeginDrag);
            ScrollRect.onEndDrag.AddListener(OnEndDrag);
            ScrollRect.onStopMoving.AddListener(OnStopMoving);

            _movementType = ScrollRect.movementType;
            ScrollRect.realMovementType = ScrollRect.movementType;
            ScrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Unrestricted;

            if (ScrollRect.content.GetComponent<VerticalLayoutGroup>() != null) {
                _verticalLayoutGroup = ScrollRect.content.GetComponent<VerticalLayoutGroup>();
                _spacingY = _verticalLayoutGroup.spacing;
            }

            if (ScrollRect.content.GetComponent<HorizontalLayoutGroup>() != null) {
                _horizontalLayoutGroup = ScrollRect.content.GetComponent<HorizontalLayoutGroup>();
                _spacingX = _horizontalLayoutGroup.spacing;
            }

            if (ScrollRect.content.GetComponent<GridLayoutGroup>() != null) {
                _gridLayoutGroup = ScrollRect.content.GetComponent<GridLayoutGroup>();
                _spacingX = _gridLayoutGroup.spacing.x;
                _spacingY = _gridLayoutGroup.spacing.y;
            }

            if (ScrollRect.content.GetComponent<ContentSizeFitter>() != null) {
                _contentSizeFitter = ScrollRect.content.GetComponent<ContentSizeFitter>();
            }

            _isHorizontal = ScrollRect.horizontal;
            _isVertical = ScrollRect.vertical;

            _objectPool.CreateMoreIfNeeded = createMoreIfNeeded;
            _objectPool.Initialize(forceAmount ?? 0, objReference, ScrollRect.content);

            CreateList(startIndex);
            DisableGridComponents();

            if (!_isHorizontal || !_isVertical) return;
            Debug.LogError(
                "DynamicScroll doesn't support scrolling in both directions, please choose one direction (horizontal or vertical)");
            _isHorizontal = false;
        }

        public void InitiateGrid(DynamicScrollRect.DynamicScrollRect scrollRect, IList<T> infoList, int startIndex,
            Transform newContainer) {
            if (Initiated) {
                ChangeGridList(infoList);
                return;
            }
            Initiated = true;
            ScrollRect = scrollRect;

            if (ScrollRect == null)
                throw new Exception("No scroll rect in gameObject.");

            if (startIndex >= infoList.Count)
                throw new Exception("Invalid index: " + startIndex);

            _infoList = infoList;

            ScrollRect.onValueChanged.AddListener(OnGridScroll);
            ScrollRect.onBeginDrag.AddListener(OnBeginDrag);
            ScrollRect.onEndDrag.AddListener(OnEndDrag);
            ScrollRect.onStopMoving.AddListener(OnStopMoving);

            _movementType = ScrollRect.movementType;
            ScrollRect.realMovementType = ScrollRect.movementType;
            ScrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Unrestricted;

            if (ScrollRect.content.GetComponent<GridLayoutGroup>() != null) {
                _gridLayoutGroup = ScrollRect.content.GetComponent<GridLayoutGroup>();
                _spacingX = _gridLayoutGroup.spacing.x;
                _spacingY = _gridLayoutGroup.spacing.y;
            }

            if (ScrollRect.content.GetComponent<ContentSizeFitter>() != null) {
                _contentSizeFitter = ScrollRect.content.GetComponent<ContentSizeFitter>();
            }

            _isHorizontal = ScrollRect.horizontal;
            _isVertical = ScrollRect.vertical;

            _objectPool.CreateMoreIfNeeded = true; //createMoreIfNeeded;
            _objectPool.InitializeGrid(0, newContainer, ScrollRect.content);

            // Tính số column của grid theo chiểu ngang của view port 
            var rect = ScrollRect.viewport.rect;
            var width = rect.width;
            var itemWidth = _gridLayoutGroup.cellSize.x + _gridLayoutGroup.spacing.x;
            Column = (int) (width / itemWidth);

            CreateGridList();
            DisableGridComponents();

            if (!_isHorizontal || !_isVertical) return;
            Debug.LogError(
                "DynamicScroll doesn't support scrolling in both directions, please choose one direction (horizontal or vertical)");
            _isHorizontal = false;
        }

        private void ChangeGridList(IList<T> infoList) {
            ScrollRect.StopMovement();
            ScrollRect.content.anchoredPosition = Vector2.zero;

            _objectPool.ClearPool();
            _infoList = infoList;

            CreateGridList();
        }

        private void CreateGridList() {
            var totalSize = 0f;
            var currentIndex = 0;
            var canDrag = false;

            var itemHeight = 0f;
            var rect = ScrollRect.viewport.rect;
            var height = rect.height;

            if (_infoList is {Count: > 0}) {
                float lastY = 0;
                do {
                    DynamicObject prev = null;

                    // Tính vị trí x đầu tiên của 1 dòng.
                    // The first one of Row
                    var obj = _objectPool.Collect();
                    var half = Column / 2f;
                    float iHalf = (int) half;
                    var numSpace = Column % 2 > 0 ? iHalf : iHalf - 0.5f;
                    _firstPosX = -(obj.CurrentWidth * (half - 0.5f)) - (numSpace * _spacingX);
                    // Nap items cho mỗi dòng.
                    var lastX = _firstPosX;
                    for (var i = 0; i < Column; i++) {
                        if (i > 0) {
                            obj = _objectPool.Collect();
                        }
                        if (obj is DynamicObject dynamicObject) {
                            if (prev) prev.NextObject = dynamicObject;
                            dynamicObject.PrevObject = prev;
                        }
                        itemHeight = obj.CurrentHeight;
                        obj.UpdateScrollObject(this._infoList[currentIndex], currentIndex, SelectId);
                        var posX = lastX;
                        obj.RectTransform.anchoredPosition = new Vector2(posX, lastY);
                        prev = obj as DynamicObject;
                        if (prev) prev.NextObject = null;
                        lastX += obj.CurrentWidth + _spacingX;
                        currentIndex++;
                        if (currentIndex >= _infoList.Count) {
                            break;
                        }
                    }
                    lastY -= itemHeight + _spacingY;
                    totalSize += itemHeight + _spacingY;
                    
                } while (currentIndex < _infoList.Count && totalSize < (height * 2f));

                canDrag = totalSize > height;
            }

            ToggleScroll(canDrag);
        }

        private void CreateList(int startIndex) {
            var totalSize = 0f;
            var lastObjectPosition = Vector2.zero;
            startIndex = Mathf.Max(0, startIndex);
            var currentIndex = startIndex;
            var canDrag = false;

            var rect = ScrollRect.viewport.rect;
            var width = rect.width;
            var height = rect.height;

            if (_infoList != null && _infoList.Count > 0) {
                do {
                    var obj = _objectPool.Collect();
                    obj.UpdateScrollObject(this._infoList[currentIndex], currentIndex);
                    var posX = currentIndex > 0 ? lastObjectPosition.x + (_isHorizontal ? _spacingX : 0) : 0;
                    var posY = currentIndex > 0 ? lastObjectPosition.y - (_isVertical ? _spacingY : 0) : 0;
                    obj.RectTransform.anchoredPosition = new Vector2(posX, posY);
                    lastObjectPosition = new Vector2(posX + (_isHorizontal ? obj.CurrentWidth : 0),
                        posY - (_isVertical ? obj.CurrentHeight : 0));

                    totalSize += ((_isVertical) ? obj.CurrentHeight : obj.CurrentWidth) + _spacingY;
                    currentIndex++;
                } while (currentIndex < _infoList.Count &&
                         (_isVertical && totalSize < (height * 2f)) ||
                         (_isHorizontal && totalSize < (width * 2f)));

                canDrag = (_isHorizontal && totalSize > width) ||
                          (_isVertical && totalSize > height);
            }

            ToggleScroll(canDrag);
        }

        private void DisableGridComponents() {
            if (_verticalLayoutGroup != null)
                _verticalLayoutGroup.enabled = false;

            if (_horizontalLayoutGroup != null)
                _horizontalLayoutGroup.enabled = false;

            if (_contentSizeFitter != null)
                _contentSizeFitter.enabled = false;

            if (_gridLayoutGroup != null) {
                _gridLayoutGroup.enabled = false;
                return;
            }

            ScrollRect.content.anchorMax = Vector2.one;
            ScrollRect.content.anchorMin = Vector2.zero;
            ScrollRect.content.offsetMax = Vector2.zero;
            ScrollRect.content.offsetMin = Vector2.zero;
        }

        private int GetFirstIndex(T1 objects) {
            var currentIndex = objects.CurrentIndex;
            if (objects is DynamicObject last) {
                while (last.PrevObject != null) {
                    last = last.PrevObject;
                }
                currentIndex = last.CurrentIndex;
            }
            return currentIndex;
        }
        
        private int GetLastIndex(T1 objects) {
            var currentIndex = objects.CurrentIndex;
            if (objects is DynamicObject last) {
                while (last.NextObject != null) {
                    last = last.NextObject;
                }
                currentIndex = last.CurrentIndex;
            }
            return currentIndex;
        }
        
        private void ReleaseRow(DynamicObject first) {
            while (first.PrevObject) {
                first = first.PrevObject;
            }
            while (first != null) {
                _objectPool.Release(first as T1);
                first = first.NextObject;
            }
        }

        private void CollectBottomRow(int currentIndex, T1 lowestObj) {
            var lowestRect = lowestObj.RectTransform;
            var lastX = _firstPosX;
            DynamicObject prev = null;
            currentIndex += 1;
            for (var i = 0; i < Column; i++) {
                if (currentIndex >= _infoList.Count) return;
                var obj = _objectPool.Collect();
                if (obj is DynamicObject dynamicObject) {
                    if (prev) prev.NextObject = dynamicObject;
                    dynamicObject.PrevObject = prev;
                }
                obj.UpdateScrollObject(_infoList[currentIndex], currentIndex, SelectId);
                obj.transform.SetAsLastSibling();
        
                _newAnchoredPosition = lowestRect.anchoredPosition;
                var posX = lastX;
                _newAnchoredPosition.x = posX;
                _newAnchoredPosition.y += -lowestObj.CurrentHeight - _spacingX;
        
                obj.RectTransform.anchoredPosition = _newAnchoredPosition;
                prev = obj as DynamicObject;
                if (prev) prev.NextObject = null;
                lastX += obj.CurrentWidth + _spacingX;
                currentIndex++;
            }
        }

        private void CollectTopRow(int currentIndex, T1 highestObj) {
            var highestRect = highestObj.RectTransform;
            var lastX = _firstPosX;
            DynamicObject prev = null;
            currentIndex -= Column;
            for (var i = 0; i < Column; i++) {
                if (currentIndex < 0) return;
                var obj = _objectPool.Collect();
                if (obj is DynamicObject dynamicObject) {
                    if (prev) prev.NextObject = dynamicObject;
                    dynamicObject.PrevObject = prev;
                }
                obj.UpdateScrollObject(_infoList[currentIndex], currentIndex, SelectId);
                obj.transform.SetAsFirstSibling();

                _newAnchoredPosition = highestRect.anchoredPosition;
                var posX = lastX;
                _newAnchoredPosition.x = posX;
                _newAnchoredPosition.y += obj.CurrentHeight + _spacingX;

                obj.RectTransform.anchoredPosition = _newAnchoredPosition;
                prev = obj as DynamicObject;
                if (prev) prev.NextObject = null;
                lastX += obj.CurrentWidth + _spacingX;
                currentIndex++;
            }
        }
        
        private void OnGridScroll(Vector2 pos) {
            var anchoredPosition = ScrollRect.content.anchoredPosition;
            _scrollVelocity = anchoredPosition - _lastPos;
            _lastPos = anchoredPosition;

            OnDragEvent?.Invoke(_scrollVelocity);

            ScrollRect.needElasticReturn = false;
            _lastInvalidDirections = LimitScroll();
            if ((_lastInvalidDirections & ScrollDirection.None) != ScrollDirection.None) {
                ScrollRect.needElasticReturn = true;
                return;
            }

            if (_isDragging)
                ScrollRect.needElasticReturn = false;

            var lowestObj = GetLowest();
            var lowestRect = lowestObj.RectTransform;
            var highestObj = GetHighest();
            var highestRect = highestObj.RectTransform;
            {
                if (_scrollVelocity.y > 0) {
                    while (highestRect.anchoredPosition.y + ScrollRect.content.anchoredPosition.y
                           > ScrollRect.viewport.rect.height) {
                        // Lấy index của obj cuối của hàng dưới cùng
                        var currentIndex = GetLastIndex(lowestObj);

                        // Nếu có thêm hàng cuối thì release hàng đầu...
                        if (currentIndex + 1 < _infoList.Count) {
                            // Release top row
                            if (highestObj is DynamicObject first) {
                                ReleaseRow(first);
                            }
                        } else {
                            return;
                        }
                        //.. và thêm hàng cuối.
                        CollectBottomRow(currentIndex, lowestObj);
                        
                        ResetObjects();
                    }
                } else if (_scrollVelocity.y < 0) {
                    while (lowestRect.anchoredPosition.y + ScrollRect.content.anchoredPosition.y
                           < -ScrollRect.viewport.rect.height) {
                        // Lấy index của obj đầu tiên của hàng trên cùng
                        var currentIndex = GetFirstIndex(highestObj);

                        // Nếu có thêm hàng đầu thì release hàng cuối...
                        if (currentIndex - 1 >= 0) {
                            // Release bottom row
                            if (lowestObj is DynamicObject first) {
                                ReleaseRow(first);
                            }
                        } else {
                            return;
                        }

                        //... và thêm hàng đầu
                        CollectTopRow(currentIndex, highestObj);
                        
                        ResetObjects();
                    }
                }
            }

            void ResetObjects() {
                lowestObj = GetLowest();
                lowestRect = lowestObj.RectTransform;
                highestObj = GetHighest();
                highestRect = highestObj.RectTransform;
            }
        }

        private void OnScroll(Vector2 pos) {
            var anchoredPosition = ScrollRect.content.anchoredPosition;
            _scrollVelocity = anchoredPosition - _lastPos;
            _lastPos = anchoredPosition;

            OnDragEvent?.Invoke(_scrollVelocity);

            _lastInvalidDirections = LimitScroll();

            if ((_lastInvalidDirections & ScrollDirection.None) != ScrollDirection.None) {
                ScrollRect.needElasticReturn = true;
                return;
            }

            if (_isDragging)
                ScrollRect.needElasticReturn = false;

            var lowestObj = GetLowest();
            var lowestRect = lowestObj.RectTransform;
            var highestObj = GetHighest();
            var highestRect = highestObj.RectTransform;

            if (_isHorizontal) {
                if (_scrollVelocity.x > 0) {
                    while (highestRect.anchoredPosition.x + ScrollRect.content.anchoredPosition.x
                           > ScrollRect.viewport.rect.width + (highestObj.CurrentWidth * 0.1f)) {
                        var nextIndex = lowestObj.CurrentIndex - 1;
                        if (nextIndex < 0) return;
                        _objectPool.Release(highestObj);
                        var obj = _objectPool.Collect();
                        obj.UpdateScrollObject(_infoList[nextIndex], nextIndex);
                        obj.transform.SetAsFirstSibling();

                        _newAnchoredPosition = lowestRect.anchoredPosition;
                        _newAnchoredPosition.x += -lowestObj.CurrentWidth - _spacingX;

                        obj.RectTransform.anchoredPosition = _newAnchoredPosition;
                        ResetObjects();
                    }
                } else if (_scrollVelocity.x < 0) {
                    while (lowestRect.anchoredPosition.x + ScrollRect.content.anchoredPosition.x +
                           (lowestObj.CurrentWidth / 2f)
                           < (-ScrollRect.viewport.rect.width / 2f) - (lowestObj.CurrentWidth * 0.1f)) {
                        var nextIndex = highestObj.CurrentIndex + 1;
                        if (nextIndex >= _infoList.Count) return;
                        _objectPool.Release(lowestObj);
                        var obj = _objectPool.Collect();
                        obj.UpdateScrollObject(_infoList[nextIndex], nextIndex);
                        obj.transform.SetAsFirstSibling();

                        _newAnchoredPosition = highestRect.anchoredPosition;
                        _newAnchoredPosition.x += obj.CurrentWidth + _spacingX;

                        obj.RectTransform.anchoredPosition = _newAnchoredPosition;
                        ResetObjects();
                    }
                }
            } else if (_isVertical) {
                if (_scrollVelocity.y > 0) {
                    while (highestRect.anchoredPosition.y + ScrollRect.content.anchoredPosition.y -
                           (highestObj.CurrentHeight / 2f)
                           > ((ScrollRect.viewport.rect.height / 2f) + highestObj.CurrentHeight * 0.1f)) {
                        var nextIndex = lowestObj.CurrentIndex + 1;
                        if (nextIndex >= _infoList.Count) return;
                        _objectPool.Release(highestObj);
                        var obj = _objectPool.Collect();
                        obj.UpdateScrollObject(_infoList[nextIndex], nextIndex);
                        obj.transform.SetAsLastSibling();

                        _newAnchoredPosition = lowestRect.anchoredPosition;
                        _newAnchoredPosition.y += -lowestObj.CurrentHeight - _spacingY;

                        obj.RectTransform.anchoredPosition = _newAnchoredPosition;
                        ResetObjects();
                    }
                } else if (_scrollVelocity.y < 0) {
                    while (lowestRect.anchoredPosition.y + ScrollRect.content.anchoredPosition.y +
                           (highestObj.CurrentHeight / 2f)
                           < -(ScrollRect.viewport.rect.height + lowestObj.CurrentHeight * 0.1f)) {
                        var nextIndex = highestObj.CurrentIndex - 1;
                        if (nextIndex < 0) return;
                        _objectPool.Release(lowestObj);
                        var obj = _objectPool.Collect();
                        obj.UpdateScrollObject(_infoList[nextIndex], nextIndex);
                        obj.transform.SetAsFirstSibling();

                        _newAnchoredPosition = highestRect.anchoredPosition;
                        _newAnchoredPosition.y += obj.CurrentHeight + _spacingY;

                        obj.RectTransform.anchoredPosition = _newAnchoredPosition;
                        ResetObjects();
                    }
                }
            }

            void ResetObjects() {
                lowestObj = GetLowest();
                lowestRect = lowestObj.RectTransform;
                highestObj = GetHighest();
                highestRect = highestObj.RectTransform;
            }
        }

        private void OnBeginDrag(PointerEventData pointData) {
            _isDragging = true;
            _forceMoveTween?.Kill();
            OnBeginDragEvent?.Invoke(pointData);
        }

        private void OnEndDrag(PointerEventData pointData) {
            _isDragging = false;
            OnEndDragEvent?.Invoke(pointData);
        }

        private void OnStopMoving(PointerEventData arg0) {
            var contentPos = ScrollRect.content.anchoredPosition;
            var highestObj = GetHighest();
            if (highestObj.CurrentIndex == 0) {
                var objPosY = contentPos.y + _spacingY;
                if (objPosY < 0) {
                    MoveToTop();
                }
            }
        }

        private void StopScrollAndChangeContentPosition(Vector2 pos) {
            ScrollRect.StopMovement();
            ScrollRect.enabled = false;
            ScrollRect.content.anchoredPosition = pos;
            ScrollRect.enabled = true;
        }

        private ScrollDirection LimitScroll() {
            var invalidDirections = ScrollDirection.None;
            var lowestObj = GetLowest();
            var lowestPos = lowestObj.RectTransform.anchoredPosition;
            var highestObj = GetHighest();
            var highestPos = highestObj.RectTransform.anchoredPosition;
            var contentPos = ScrollRect.content.anchoredPosition;

            if (_isVertical) {
                if (highestObj.CurrentIndex ==  0) {
                    //Going Down
                    var objPosY = contentPos.y + _spacingY;
                    var limit = 0;

                    if (objPosY < limit) {
                        _clampedPosition = new Vector2(contentPos.x, ScrollRect.viewport.rect.height - highestPos.y);
                        ScrollRect.clampedPosition = _clampedPosition;

                        if (_movementType == UnityEngine.UI.ScrollRect.MovementType.Clamped)
                            StopScrollAndChangeContentPosition(_clampedPosition);
                        invalidDirections |= ScrollDirection.Down;
                        invalidDirections &= ~ScrollDirection.None;
                    }
                }
                if (lowestObj.CurrentIndex >= _infoList.Count - Column) {
                    //Going Up
                    var objPosY = contentPos.y + lowestPos.y + ScrollRect.viewport.rect.height - _spacingY;
                    var limit = lowestObj.CurrentHeight;

                    if (objPosY > limit) {
                        _clampedPosition = new Vector2(contentPos.x, contentPos.y + limit - objPosY);
                        ScrollRect.clampedPosition = _clampedPosition;
                        _forceMoveTween?.Kill();

                        if (_movementType == UnityEngine.UI.ScrollRect.MovementType.Clamped)
                            StopScrollAndChangeContentPosition(_clampedPosition);
                        invalidDirections |= ScrollDirection.Up;
                        invalidDirections &= ~ScrollDirection.None;
                    }
                }
            }
            // else if (mIsHorizontal)
            // {
            //     if (highestObj.CurrentIndex == _infoList.Count - 1)
            //     {
            //         //Going Left
            //         var objPosX = ScrollRect.content.anchoredPosition.x + highestPos.x + spacing + highestObj.CurrentWidth;
            //         var limit = ScrollRect.viewport.rect.width;
            //         if (objPosX < limit)
            //         {
            //             mClampedPosition = new Vector2(contentPos.x + limit - objPosX, contentPos.y);
            //             ScrollRect.clampedPosition = mClampedPosition;
            //             forceMoveTween?.Kill();
            //
            //             if (mMovementType == UnityEngine.UI.ScrollRect.MovementType.Clamped)
            //                 StopScrollAndChangeContentPosition(mClampedPosition);
            //             invalidDirections |= ScrollDirection.LEFT;
            //             invalidDirections &= ~ScrollDirection.NONE;
            //         }
            //     }
            //     if (lowestObj.CurrentIndex == 0)
            //     {
            //         //Going Right
            //         var objPosX = ScrollRect.content.anchoredPosition.x + lowestPos.x;
            //         var limit = 0;
            //
            //         if (objPosX > limit)
            //         {
            //             mClampedPosition = new Vector2(contentPos.x + limit - objPosX, contentPos.y);
            //             forceMoveTween?.Kill();
            //
            //             if (mMovementType == UnityEngine.UI.ScrollRect.MovementType.Clamped)
            //                 StopScrollAndChangeContentPosition(mClampedPosition);
            //             invalidDirections |= ScrollDirection.RIGHT;
            //             invalidDirections &= ~ScrollDirection.NONE;
            //         }
            //     }
            // }
            return invalidDirections;
        }

        public void ToggleScroll(bool active) {
            ScrollRect.enabled = active;
            if (_gridLayoutGroup == null) {
                ScrollRect.viewport.anchorMin = new Vector2(0, 0);
                ScrollRect.viewport.anchorMax = new Vector2(1, 1);
                ScrollRect.viewport.offsetMin = new Vector2(0, 0);
                ScrollRect.viewport.offsetMax = new Vector2(0, 0);
                ScrollRect.viewport.pivot = new Vector2(0.5f, 0.5f);
            }

            if (!active)
                ScrollRect.content.anchoredPosition = Vector2.zero;
        }

        private void MoveToTop() {
            _forceMoveTween = ScrollRect.content.DOAnchorPosY(0, 2).SetEase(Ease.OutQuint);
        }

        public void MoveToIndex(int index) {
            if (index < Column) {
                MoveToTop();
                return;
            }

            if (index >= _infoList.Count)
                throw new Exception("Invalid index to move: " + index);

            var refObject = _objectPool.GetAllWithState(true)[0];
            index = Mathf.Clamp(index / Column, 0, _infoList.Count - 1);
            _forceMoveTween?.Kill();
            ScrollRect.StopMovement();
            ScrollRect.needElasticReturn = false;

            const int time = 2;
            var pos = _isHorizontal
                ? -((index * (refObject.CurrentWidth + _spacingX)) - (ScrollRect.viewport.rect.width / 2f) +
                    (refObject.CurrentWidth / 2f))
                : ((index * (refObject.CurrentHeight + _spacingY)) - (ScrollRect.viewport.rect.height / 2f) +
                   (refObject.CurrentHeight / 2f));
            _forceMoveTween =
                (_isHorizontal
                    ? ScrollRect.content.DOAnchorPosX(pos, time)
                    : ScrollRect.content.DOAnchorPosY(pos, time)).SetEase(Ease.OutQuint);
        }

        private T1 GetLowest() {
            var min = float.MaxValue;
            T1 lowestObj = null;
            var objs = _objectPool.GetAllWithState(true);

            foreach (var t in objs) {
                var anchoredPosition = t.RectTransform.anchoredPosition;
                if ((!_isVertical || !(anchoredPosition.y < min)) && (!_isHorizontal || !(anchoredPosition.x < min))) {
                    continue;
                }
                min = _isVertical ? anchoredPosition.y : anchoredPosition.x;
                lowestObj = t;
            }

            return lowestObj;
        }

        private T1 GetHighest() {
            var max = float.MinValue;
            T1 highestObj = null;
            var objs = _objectPool.GetAllWithState(true);
            foreach (var t in objs) {
                var anchoredPosition = t.RectTransform.anchoredPosition;
                if ((!_isVertical || !(anchoredPosition.y > max)) && (!_isHorizontal || !(anchoredPosition.x > max))) {
                    continue;
                }
                max = _isVertical ? anchoredPosition.y : anchoredPosition.x;
                highestObj = t;
            }

            return highestObj;
        }
    }
}