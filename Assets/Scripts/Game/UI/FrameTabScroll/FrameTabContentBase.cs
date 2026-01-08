using System.Collections.Generic;

using UnityEngine;

namespace Game.UI.FrameTabScroll {
    public abstract class FrameTabContentBase <TabMenu> : MonoBehaviour
    {
        public class CTabMenu : TabMenuBase<TabMenu> {
        };

        public class CContent : SlotBase<TabMenu> {
        };
        
        [SerializeField]
        private Transform tabList;
    
        [SerializeField]
        private Transform contentList;
        
        [SerializeField]
        private TabMenu currentTabSelect;
        public TabMenu CurrentTabSelect => currentTabSelect;
        
        private CContent[] _contents = null;
        
        private static Dictionary<TabMenu, CTabMenu> _disTabs;
        private static Dictionary<TabMenu, CContent> _disContent;
        public Dictionary<TabMenu, CTabMenu> DisTabs => _disTabs;
        protected Dictionary<TabMenu, CContent> DisContent => _disContent;
        protected CContent[] Contents => _contents;
        
        private bool _isInitSegments = false;
        protected bool IsInitSegments => _isInitSegments;

        protected virtual void LateUpdate() {
            if (!_isInitSegments) {
                if (InitSegments()) {
                    _isInitSegments = true;
                    // Done 
                }
                return;
            }
        }

        private bool InitSegments() {
            _contents = contentList.GetComponentsInChildren<CContent>();
            // this._segments = this.GetSegmentList();
            _disContent = new Dictionary<TabMenu, CContent>();
            _disTabs = new Dictionary<TabMenu, CTabMenu>();
            foreach (var segment in _contents) {
                _disContent.Add(segment.TypeMenu, segment);
            }
            var tabs = tabList.GetComponentsInChildren<CTabMenu>();
            // var tabs = this.GetTabList();
            foreach (var tab in tabs) {
                tab.OnSelectMenu = SetSelectMenu;
                _disTabs.Add(tab.TypeMenu, tab);
            }
            // Set first init
            SetSelectMenu(currentTabSelect);
            OnInitUiDone();
            return true;
        }

        protected virtual void OnInitUiDone() {
        }
        
        protected virtual void OnChangeTabMenu() {
        }
        
        private void SetSelectMenu(TabMenu typeMenu) {
            currentTabSelect = typeMenu;
            foreach (var p in _disTabs) {
                p.Value.UiSelect(p.Key.Equals(typeMenu));
            }
            for (var idx = 0; idx < _contents.Length; idx++) {
                var s = _contents[idx];
                if (s.TypeMenu.Equals(typeMenu)) {
                    s.ShowContent();
                } else {
                    s.HideContent();
                }
            }
            OnChangeTabMenu();
        }
    }
}
