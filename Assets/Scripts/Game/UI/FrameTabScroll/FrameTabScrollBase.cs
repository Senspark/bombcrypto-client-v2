using System;
using System.Collections.Generic;
using System.Linq;

using Game.UI.Custom;
using Game.UI.FrameTabScroll;

using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.FrameTabScroll {
    public class FrameTabScrollBase<TabMenu> : MonoBehaviour {
        public class CTabMenu : TabMenuBase<TabMenu> {
        };

        public class CSegment : SegmentBase<TabMenu> {
        };

        [SerializeField]
        protected Transform tabList;

        [SerializeField]
        protected Transform segmentList;

        [SerializeField]
        private CustomScrollRect scroller;

        private bool _isInitSegments = false;
        protected bool IsInitSegments => _isInitSegments;

        private CSegment[] _segments = null;
        private Dictionary<TabMenu, CTabMenu> _dicTabs;
        private Dictionary<TabMenu, CSegment> _dicSegments;
        protected Dictionary<TabMenu, CSegment> DicSegments => _dicSegments;
        protected Dictionary<TabMenu, CTabMenu> DicTabs => _dicTabs;

        private CSegment _cSegmentsLast;
        private TabMenu _typeTabSelect;
        private float _scrollerMoveTo = -1;
        private bool _isWaitScrollDone = false;
        private const bool IsDisableSnapAfterScroll = true;

        protected virtual void Awake() {
            scroller.OnBeginScroll = BeginScroll;
        }

        private void BeginScroll() {
            if (!_isInitSegments) {
                return;
            }
            _isWaitScrollDone = true;
            if (IsDisableSnapAfterScroll) {
                _scrollerMoveTo = -1;
            }
        }

        protected virtual void LateUpdate() {
            if (!_isInitSegments) {
                if (InitSegments()) {
                    _isInitSegments = true;
                }
                return;
            }
            if (IsDisableSnapAfterScroll) {
                return;
            }
            if (_isWaitScrollDone && !scroller.IsDragging && !scroller.IsScrolling) {
                var p = scroller.verticalNormalizedPosition;
                var v = scroller.velocity.y;
                if (p >= -0.01 && p <= 1.01 && Mathf.Abs(v) < 4.5f) {
                    _isWaitScrollDone = false;
                    // Snap Segment
                    FindSnapSegment();
                    return;
                }
            }
            if (_isWaitScrollDone || scroller.IsDragging || scroller.IsScrolling ||
                scroller.velocity.y != 0) {
                return;
            }
            MoveScrollerUpdate();
            return;
        }

        private bool InitSegments() {
            _segments = segmentList.GetComponentsInChildren<CSegment>();
            // this._segments = this.GetSegmentList();
            _cSegmentsLast = _segments.Last();
            var viewportSize = segmentList.transform.parent.GetComponent<RectTransform>().rect;
            var sizeLastSegment = _cSegmentsLast.gameObject.GetComponent<RectTransform>().sizeDelta;
            var ySegmentsLast = _cSegmentsLast.transform.localPosition.y;
            var viewportSizeHeight = viewportSize.height;
            if (Math.Abs(ySegmentsLast) <= viewportSizeHeight) {
                return false;
            }
            _dicSegments = new Dictionary<TabMenu, CSegment>();
            _dicTabs = new Dictionary<TabMenu, CTabMenu>();
            foreach (var segment in _segments) {
                _dicSegments.Add(segment.TypeMenu, segment);
                segment.ForceSelectMenu = () => { SetSelectMenu(segment.TypeMenu); };
            }
            var tabs = tabList.GetComponentsInChildren<CTabMenu>();
            foreach (var tab in tabs) {
                tab.OnSelectMenu = OnClickMenuTab;
                _dicTabs.Add(tab.TypeMenu, tab);
            }
            // Set first init
            OnInitSegmentsDone();
            return true;
        }

        //Call after hide an Segment 
        protected void ReloadSegmentsLast() {
            _segments = segmentList.GetComponentsInChildren<CSegment>();
            _cSegmentsLast = _segments.Last();
            var viewportSize = segmentList.transform.parent.GetComponent<RectTransform>().rect;
            var isFindTabFirst = true;
            foreach (var tab in _dicTabs.Where(tab => tab.Value.gameObject.activeSelf)) {
                if (isFindTabFirst) {
                    tab.Value.UiSelect(true);
                    isFindTabFirst = false;
                } else {
                    tab.Value.UiSelect(false);
                }
            }
        }

        protected virtual void OnInitSegmentsDone() {
        }

        private void MoveScrollerUpdate() {
            if (_scrollerMoveTo <= -1) {
                return;
            }
            var offset = _scrollerMoveTo - scroller.verticalNormalizedPosition;
            var abs = Math.Abs(offset);
            if (abs > 0.002) {
                scroller.verticalNormalizedPosition += offset * 0.05f;
            } else {
                scroller.verticalNormalizedPosition = _scrollerMoveTo;
                UpdateUiTabSelect(_typeTabSelect);
            }
        }

        protected void UpdateUiTabSelect(TabMenu typeTabSelect) {
            foreach (var tab in _dicTabs) {
                tab.Value.UiSelect(tab.Key.Equals(typeTabSelect));
            }
        }

        private void FindSnapSegment() {
            var targetTab = _typeTabSelect;
            var ySegmentsLast = _cSegmentsLast.transform.localPosition.y;
            var currentPosY = (1 - scroller.verticalNormalizedPosition) * ySegmentsLast;
            var offsetCheck = float.MaxValue;
            foreach (var segment in _segments) {
                var r = segment.Rect.localPosition;
                var offset = Math.Abs(r.y - currentPosY);
                if (offset < offsetCheck) {
                    offsetCheck = offset;
                    targetTab = segment.TypeMenu;
                }
            }
            if (!_dicTabs.ContainsKey(targetTab)) {
                return;
            }
            _dicTabs[targetTab].ForceSelect();
            if (IsDisableSnapAfterScroll) {
                _scrollerMoveTo = -1;
            }
        }

        protected virtual void OnClickMenuTab(TabMenu typeMenu) {
            UpdateUiTabSelect(typeMenu);
            SetSelectMenu(typeMenu);
        }

        private void SetSelectMenu(TabMenu typeMenu) {
            if (!_dicSegments.ContainsKey(typeMenu)) {
                return;
            }
            var rt = segmentList.GetComponent<RectTransform>();
            var heightMax = rt.rect.height;
            if (heightMax == 0) {
                return;
            }
            var rtView = scroller.viewport.GetComponent<RectTransform>();
            heightMax -= rtView.rect.height;
            var segmentTarget = _dicSegments[typeMenu];
            var position = segmentTarget.gameObject.transform.localPosition;
            // var heightMax = this._cSegmentsLast.transform.localPosition.y + this._padingBottom;
            // var mu = Math.Abs(position.y / heightMax);
            var mu = Math.Abs(position.y / heightMax);
            _scrollerMoveTo = 1 - mu;
            scroller.StopMovement();
            _isWaitScrollDone = false;
            _typeTabSelect = typeMenu;
        }
    }
}