using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI.Custom
{
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class CustomContentSizeFitter : UIBehaviour, ILayoutSelfController
    {
        private static bool SetStruct<T>(ref T currentValue, T newValue) where T : struct
        {
            if (EqualityComparer<T>.Default.Equals(currentValue, newValue)) {
                return false;
            }

            currentValue = newValue;
            return true;
        }
        /// <summary>
        /// The size fit modes avaliable to use.
        /// </summary>
        public enum FitMode
        {
            /// <summary>
            /// Don't perform any resizing.
            /// </summary>
            Unconstrained,
            /// <summary>
            /// Resize to the minimum size of the content.
            /// </summary>
            MinSize,
            /// <summary>
            /// Resize to the preferred size of the content.
            /// </summary>
            PreferredSize
        }

        [SerializeField] protected FitMode m_HorizontalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the width.
        /// </summary>
        private FitMode horizontalFit { get { return m_HorizontalFit; } set { if (SetStruct(ref m_HorizontalFit, value)) {
                SetDirty();
            }
        } }

        [SerializeField] protected FitMode m_VerticalFit = FitMode.Unconstrained;

        /// <summary>
        /// The fit mode to use to determine the height.
        /// </summary>
        private FitMode verticalFit { get { return m_VerticalFit; } set { if (SetStruct(ref m_VerticalFit, value)) {
                SetDirty();
            }
        } }

        [System.NonSerialized] private RectTransform m_Rect;
        private bool _requestAutoLayout = false;
        private Action _onAutoLayoutDone = null;
        
        // public Rect rect =>  m_Rect.rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null) {
                    m_Rect = GetComponent<RectTransform>();
                }
                return m_Rect;
            }
        }

        // field is never assigned warning
        #pragma warning disable 649
        private DrivenRectTransformTracker m_Tracker;
        #pragma warning restore 649

        protected CustomContentSizeFitter()
        {}

        protected override void OnEnable()
        {
            base.OnEnable();
            SetDirty();
        }

        protected override void OnDisable()
        {
            m_Tracker.Clear();
    #if UNITY_EDITOR
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    #endif
            base.OnDisable();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private void HandleSelfFittingAlongAxis(int axis)
        {
            var fitting = (axis == 0 ? horizontalFit : verticalFit);
            if (fitting == FitMode.Unconstrained)
            {
                // Keep a reference to the tracked transform, but don't control its properties:
                m_Tracker.Add(this, rectTransform, DrivenTransformProperties.None);
                return;
            }

            m_Tracker.Add(this, rectTransform, (axis == 0 ? DrivenTransformProperties.SizeDeltaX : DrivenTransformProperties.SizeDeltaY));

            // Set size to min or preferred size
            if (fitting == FitMode.MinSize) {
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetMinSize(m_Rect, axis));
            } else {
                rectTransform.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, LayoutUtility.GetPreferredSize(m_Rect, axis));
            }
        }

        /// <summary>
        /// Calculate and apply the horizontal component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(0);
        }

        /// <summary>
        /// Calculate and apply the vertical component of the size to the RectTransform
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(1);
        }

        protected void SetDirty() {
            if (!IsActive()) {
                return;
            }
#if UNITY_EDITOR
            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
    #endif
        }

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }

    #endif
        
        public void AutoLayoutVertical(Action onAutoLayoutDone = null) {
            verticalFit = FitMode.PreferredSize;
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
            this._requestAutoLayout = true;
            this._onAutoLayoutDone = onAutoLayoutDone;
        }
        
        public void AutoLayoutHorizontal(Action onAutoLayoutDone = null) {
            horizontalFit = FitMode.PreferredSize;
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.rectTransform);
            this._requestAutoLayout = true;
            this._onAutoLayoutDone = onAutoLayoutDone;
        }
        
        protected void LateUpdate() {
            if(!this._requestAutoLayout) {
                return;
            }
            var tRoot = this.transform;
            
            if (tRoot.childCount <= 0) {
                this._requestAutoLayout = false;
                return;
            }
            if (verticalFit == FitMode.PreferredSize || verticalFit == FitMode.MinSize) {
                this.SnapVertical();
            }
            if (horizontalFit == FitMode.PreferredSize || horizontalFit == FitMode.MinSize) {
                this.SnapHorizontal();
            }
            this._requestAutoLayout = false;
            this._onAutoLayoutDone?.Invoke();
            this._onAutoLayoutDone = null;
        }

        public void ForceSnapVertical() {
            // Todo: update late
        }

        private void SnapVertical() {
            var contentTransform = this.transform;
            var verticalLayoutGroup = contentTransform.GetComponent<VerticalLayoutGroup>();
            if(!verticalLayoutGroup) {
                return;
            }
            float childTotalHeights = 0;
            var activeChildren = 0;
            for (var i = 0; i < contentTransform.childCount; i++)
            {
                if (contentTransform.GetChild(i).gameObject.activeInHierarchy)
                {
                    childTotalHeights += ((RectTransform)contentTransform.GetChild(i)).sizeDelta.y;
                    activeChildren++;
                }
            }
            var spacingTotal = (activeChildren - 1) * verticalLayoutGroup.spacing;
            var totalHeight = childTotalHeights + spacingTotal + verticalLayoutGroup.padding.top + verticalLayoutGroup.padding.bottom;

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, totalHeight);
        }
        
        public void ForceSnapHorizontal() {
            // Todo: update late
        }
        
        private void SnapHorizontal() {
            var contentTransform = this.transform;
            var horizLayoutGroup = contentTransform.GetComponent<HorizontalLayoutGroup>();
            if(!horizLayoutGroup) {
                return;
            }
            float childTotalWidths = 0;
            var activeChildren = 0;
            for (var i = 0; i < contentTransform.childCount; i++)
            {
                if (contentTransform.GetChild(i).gameObject.activeInHierarchy)
                {
                    childTotalWidths += ((RectTransform)contentTransform.GetChild(i)).sizeDelta.x;
                    activeChildren++;
                }
            }
            var spacingTotal = (activeChildren - 1) * horizLayoutGroup.spacing;
            var totalWidth = childTotalWidths + spacingTotal + horizLayoutGroup.padding.left + horizLayoutGroup.padding.right;

            rectTransform.sizeDelta = new Vector2(totalWidth, rectTransform.sizeDelta.y);
        }
    }
}
