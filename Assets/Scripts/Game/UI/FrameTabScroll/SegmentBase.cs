using System;

using UnityEngine;

namespace Game.UI.FrameTabScroll {
    public class SegmentBase <TabMenu> : MonoBehaviour {
        
        [SerializeField]
        protected TabMenu typeMenu;
        
        [SerializeField]
        public GameObject frameContent;

        public TabMenu TypeMenu => typeMenu;
        
        public Action ForceSelectMenu = null;
        
        protected RectTransform m_Rect = null;
        
        public RectTransform Rect
        {
            get
            {
                if (m_Rect == null) {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        protected virtual void Awake() {
        }
    }
}